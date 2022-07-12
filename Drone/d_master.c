#include <d_master.h>
#include <assert.h>
#include <time.h>

#define LOAD_NN_FILE "test.NN"

struct drone_data drone_data;

double sqrt2;

int main() {
    srand (time(NULL) + getpid()); // Initalize seed for random numbers
    sqrt2 = sqrt(2.0);

    printf("Initializing Drone\n");

    init_drone_data(&drone_data);
    init_server_socket(&drone_data);
    pack_msg_with_standard_header(drone_data.m_json, &drone_data, CODE_MOTOR_OUTPUT);

#if IS_SIMULATION
    // Request target NN from server
    char* target_NN_folder = request_target_NN_folder_from_server();
    init_and_test_NN_from_folder(target_NN_folder);
#else
    // TODO: load NN directly from file on device
#endif

    init_drone_sensor_data(&drone_data);

    drone_logic_loop();
}

void init_and_test_NN_from_folder(char* folder) {
    init_all_neural_data_in_dir(folder, &drone_data);
    printf("NN init check\n");
    feed_forward_full_network(&drone_data);
    printf("NN feedforward check\n");
    print_matrix("NN output", drone_data.combine_neural->output_layer, drone_data.combine_neural->weights_row_count[drone_data.combine_neural->weights_matrix_count-1], 1);
    printf("\n\n");

    // INIT INPUT LAYERS
    int sensor_input_size = DRONE_CIRCLE_SENSOR_COUNT + 2; // Circle sensors + top sensor + bottom sensor
    ASSERT((int) network_input_layer_size(drone_data.sensor_neural) % sensor_input_size == 0);
    int sensor_timesteps = (int) (network_input_layer_size(drone_data.sensor_neural) / sensor_input_size);
    drone_data.sensor_time_buffer = init_timebuffer(sensor_input_size, sensor_timesteps);

    int rotation_input_size = 2; // x, y, z axis
    ASSERT((int) network_input_layer_size(drone_data.rotation_neural) % rotation_input_size == 0);
    int rotation_timesteps = (int) (network_input_layer_size(drone_data.rotation_neural) / rotation_input_size);
    drone_data.rotation_time_buffer = init_timebuffer(rotation_input_size, rotation_timesteps);
}

void drone_logic_loop() {
    ASSERT(network_input_layer_size(drone_data.sensor_neural) % (DRONE_CIRCLE_SENSOR_COUNT + 2) == 0);
    ASSERT(network_input_layer_size(drone_data.rotation_neural) % 2 == 0);

    char* server_response;
    struct json_object* json_response;
    struct json_object* response_opcode_json;
    int32_t response_opcode;

    struct json_object* response_NN_folder_json;
    char* response_NN_folder;

    struct timeval timea, timeb;
    gettimeofday(&timea, 0);

    while (TRUE) {
        motor_output(&drone_data);

        // Receive sensore data respone from server - position, distance sensors etc.
        server_response = receive_server_message(&drone_data);

        gettimeofday(&timeb, 0);

        useconds_t elapsed_time = timeb.tv_usec - timea.tv_usec;
        useconds_t sleep_time = 32000 - elapsed_time;
        if (sleep_time > 32000) {
            sleep_time = 32000;
        }
        printf("Sleep time: %d\n", (int) sleep_time);

        // usleep(32000); // sleep for 32 milliseconds - 30HZ
        usleep(sleep_time);

        gettimeofday(&timea, 0);

        // Decode server response
        json_response = json_tokener_parse(server_response);

        json_object_object_get_ex(json_response, "opcode", &response_opcode_json);
        
        response_opcode = json_object_get_int(response_opcode_json);

        switch (response_opcode) {
            case RESPONSE_CODE_LOAD_NN:
                free(drone_data.sensor_neural);
                free(drone_data.rotation_neural);
                free(drone_data.combine_neural);

                free(drone_data.sensor_time_buffer);
                free(drone_data.rotation_time_buffer);

                json_object_object_get_ex(json_response, "nnFolder", &response_NN_folder_json);
                
                response_NN_folder = json_object_get_string(response_NN_folder_json);

                init_and_test_NN_from_folder(response_NN_folder);

                init_drone_sensor_data(&drone_data);
                break;
            case RESPONSE_CODE_SENSOR_DATA:
                // Read sensor data from server response
                read_sensor_data(&drone_data, json_response);

                // Set input layer to neural network
                set_NN_input_from_sensor_data(&drone_data);

                // Feed forward data
                feed_forward_full_network(&drone_data);

                // Set motors from output
                motor_output_from_controller(
                    &drone_data,
                    drone_data.combine_neural->output_layer[0],
                    drone_data.combine_neural->output_layer[1],
                    drone_data.combine_neural->output_layer[2],
                    drone_data.combine_neural->output_layer[3]
                );

                // drone_data.m_fl = (drone_data.combine_neural->output_layer[0] * 2) - 1;
                // drone_data.m_fr = (drone_data.combine_neural->output_layer[1] * 2) - 1;
                // drone_data.m_br = (drone_data.combine_neural->output_layer[2] * 2) - 1;
                // drone_data.m_bl = (drone_data.combine_neural->output_layer[3] * 2) - 1;

                printf("\n\n\n\n");
                print_motor_output(&drone_data);
                break;
            default:
                fprintf(stderr, "Unhandled server response");
                return;
        }

        json_object_put(json_response);
    }
}

void init_drone_sensor_data(struct drone_data* drone) {
    /// INFARED SENSORS //////////////////////////////////////
    for (int i = 0; i < DRONE_CIRCLE_SENSOR_COUNT; i++) {
        drone->circle_sensor_array[i] = SENSOR_DEFAULT;
    }

    drone->sensor_top = SENSOR_DEFAULT;
    drone->sensor_bottom = 0;

    for (int i = 0; i < timebuffer_total_size(drone->sensor_time_buffer); i++) {
        drone->sensor_time_buffer->full_buffer[i] = SENSOR_DEFAULT;
    }
    /// INFARED SENSORS //////////////////////////////////////

    /// ROTATION DATA ////////////////////////////////////////
    drone->rotation_x = 0;
    drone->rotation_y = 0;
    drone->rotation_z = 0;

    for (int i = 0; i < timebuffer_total_size(drone->rotation_time_buffer); i++) {
        drone->rotation_time_buffer->full_buffer[i] = 0;
    }
    /// ROTATION DATA ////////////////////////////////////////
}

void read_sensor_data(struct drone_data* drone, struct json_object* json_in) {
    /// INFARED SENSORS //////////////////////////////////////
    struct json_object* sensor_data_array;
    struct json_object* sensor_data;
    json_object_object_get_ex(json_in, "circleSensorData", &sensor_data_array);
    
    size_t cirlce_sensor_data_count = json_object_array_length(sensor_data_array);

    ASSERT(cirlce_sensor_data_count == DRONE_CIRCLE_SENSOR_COUNT);

    for (int i = 0; i < DRONE_CIRCLE_SENSOR_COUNT; i++) {
        sensor_data = json_object_array_get_idx(sensor_data_array, i);
        drone->circle_sensor_array[i] = json_object_get_double(sensor_data);
    }

    assign_double_from_json(json_in, "sensorTop", &drone->sensor_top);
    assign_double_from_json(json_in, "sensorBottom", &drone->sensor_bottom);
    /// INFARED SENSORS //////////////////////////////////////

    /// ROTATION DATA ////////////////////////////////////////
    assign_double_from_json(json_in, "rotationX", &drone->rotation_x);
    assign_double_from_json(json_in, "rotationY", &drone->rotation_y);
    assign_double_from_json(json_in, "rotationZ", &drone->rotation_z);
    /// ROTATION DATA ////////////////////////////////////////
}

void set_NN_input_from_sensor_data(struct drone_data* drone) {
    /// INFARED SENSORS //////////////////////////////////////
    timebuffer_set(drone->sensor_time_buffer, drone_data.circle_sensor_array, DRONE_CIRCLE_SENSOR_COUNT, 0);
    drone->sensor_time_buffer->buffer[DRONE_CIRCLE_SENSOR_COUNT] = drone_data.sensor_top;
    drone->sensor_time_buffer->buffer[DRONE_CIRCLE_SENSOR_COUNT + 1] = drone_data.sensor_bottom;
    timebuffer_increment(drone->sensor_time_buffer);

    timebuffer_copy_corrected(drone->sensor_time_buffer, drone_data.sensor_neural->input_layer);
    /// INFARED SENSORS //////////////////////////////////////

    /// ROTATION DATA ////////////////////////////////////////
    drone->rotation_time_buffer->buffer[0] = drone_data.rotation_x / 90;
    drone->rotation_time_buffer->buffer[1] = drone_data.rotation_y / 90;
    // drone->rotation_time_buffer->buffer[2] = drone_data.rotation_z / 90;
    timebuffer_increment(drone->rotation_time_buffer);
    timebuffer_copy_corrected(drone->rotation_time_buffer, drone_data.rotation_neural->input_layer);
    /// ROTATION DATA ////////////////////////////////////////
}

void motor_output_from_controller(struct drone_data* drone, double x_in, double y_in, double limit_scaler, double power_scaler) {
    // Remap values from    0 - 1      to      -1 - 1
    double x_out = (2 * x_in) - 1;
    double y_out = (2 * y_in) - 1;
    power_scaler = (2 * power_scaler) - 1;

    double v_length = sqrt(pow(x_out, 2) + pow(y_out, 2));

    // Clamp output to 1 max
    if (v_length > 1) {
        x_out = x_out / v_length;
        y_out = y_out / v_length;
    }

    x_out = x_out * fabs(x_out);
    y_out = y_out * fabs(y_out);

    // Convert
    drone->m_fl = compute_motor_output_from_offset(x_out, y_out, -1, 1);
    drone->m_fr = compute_motor_output_from_offset(x_out, y_out, 1, 1);
    drone->m_br = compute_motor_output_from_offset(x_out, y_out, 1, -1);
    drone->m_bl = compute_motor_output_from_offset(x_out, y_out, -1, -1);

    double m_mean = (drone->m_fl + drone->m_fr + drone->m_br + drone->m_bl) / 4;

    // Shift & remap
    drone->m_fl = compute_motor_output_from_scalers(drone->m_fl, m_mean, power_scaler, limit_scaler);
    drone->m_fr = compute_motor_output_from_scalers(drone->m_fr, m_mean, power_scaler, limit_scaler);
    drone->m_br = compute_motor_output_from_scalers(drone->m_br, m_mean, power_scaler, limit_scaler);
    drone->m_bl = compute_motor_output_from_scalers(drone->m_bl, m_mean, power_scaler, limit_scaler);
}

double compute_motor_output_from_offset(double direction_x, double direction_y, double m_pos_x, double m_pos_y) {
    return sqrt(pow(m_pos_x - direction_x, 2) + pow(m_pos_y - direction_y , 2)) / sqrt2;
}

double compute_motor_output_from_scalers(double m_in, double m_mean, double power_scaler, double limit_scaler) {
    return power_scaler * (m_in - (m_mean * limit_scaler));
}

void motor_output(struct drone_data* drone) {
    struct json_object* json_fl;
    struct json_object* json_fr;
    struct json_object* json_br;
    struct json_object* json_bl;

    json_object_object_get_ex(drone->m_json, "motor_fl", &json_fl);
    json_object_object_get_ex(drone->m_json, "motor_fr", &json_fr);
    json_object_object_get_ex(drone->m_json, "motor_br", &json_br);
    json_object_object_get_ex(drone->m_json, "motor_bl", &json_bl);

    json_object_set_double(json_fl, drone->m_fl);
    json_object_set_double(json_fr, drone->m_fr);
    json_object_set_double(json_br, drone->m_br);
    json_object_set_double(json_bl, drone->m_bl);

    // Send server message
    send_server_json(drone, drone->m_json);
}

void print_motor_output(struct drone_data* drone) {
    printf("█████████████████████████████████████████████████\n");
    printf("█\tf_l\t\t\tf_r\t\t█\n█\t%f\t\t%f\t█\n█\t\t\t\t\t\t█\n█\t%f\t\t%f\t█\n█\tb_l\t\t\tb_r\t\t█\n", drone->m_fl, drone->m_fr, drone->m_bl, drone->m_br);
    printf("█████████████████████████████████████████████████\n");
}




#if IS_SIMULATION
char* request_target_NN_folder_from_server() {
    // Send request to server
    struct json_object* request_json = json_object_new_object();
    pack_msg_with_standard_header(request_json, &drone_data, CODE_REQUEST_TARGET_NN_FROM_SERVER);
    send_server_json(&drone_data, request_json);
    
    // Receive response from server
    struct json_object* response_json = receive_server_json(&drone_data);

    // Decode response
    struct json_object* json_folder_addr;
    json_object_object_get_ex(response_json, "nnFolder", &json_folder_addr);
    char* target_folder = json_object_get_string(json_folder_addr);

    // Reallocate and copy result
    char* target_folder_copy = malloc(strlen(target_folder));
    sprintf(target_folder_copy, target_folder);

    // Free memory
    json_object_put(request_json);
    json_object_put(response_json);

    printf("NN target file:\n\t%s\n", target_folder_copy);

    return target_folder_copy;
}

void assign_double_from_json(struct json_object* json_in, const char* field_name, double* d) {
    struct json_object* j;
    json_object_object_get_ex(json_in, field_name, &j);
    *d = json_object_get_double(j);
}

void test_motor_controller() {
    printf("Center full force\n");
    motor_output_from_controller(
        &drone_data,
        0.5, // X_in
        0.5, // Y_in
        0, // Limit scaler
        1  // Power scaler
    );
    print_motor_output(&drone_data);
    printf("\n\n");

    printf("Center upsidedown full force\n");
    motor_output_from_controller(
        &drone_data,
        0.5, // X_in
        0.5, // Y_in
        0, // Limit scaler
        1  // Power scaler
    );
    print_motor_output(&drone_data);
    printf("\n\n");

    printf("Resting\n");
    motor_output_from_controller(
        &drone_data,
        0.5, // X_in
        0.5, // Y_in
        0, // Limit scaler
        0  // Power scaler
    );
    print_motor_output(&drone_data);
    printf("\n\n");

    printf("Forward full force\n");
    motor_output_from_controller(
        &drone_data,
        0.5, // X_in
        1, // Y_in
        0, // Limit scaler
        1  // Power scaler
    );
    print_motor_output(&drone_data);
    printf("\n\n");

    printf("Forward full rotational\n");
    motor_output_from_controller(
        &drone_data,
        0.5, // X_in
        1, // Y_in
        1, // Limit scaler
        1  // Power scaler
    );
    print_motor_output(&drone_data);
    printf("\n\n");


    printf("Right full force\n");
    motor_output_from_controller(
        &drone_data,
        1, // X_in
        0.5, // Y_in
        0, // Limit scaler
        1  // Power scaler
    );
    print_motor_output(&drone_data);
    printf("\n\n");

    printf("Left full force\n");
    motor_output_from_controller(
        &drone_data,
        0, // X_in
        0.5, // Y_in
        0, // Limit scaler
        1  // Power scaler
    );
    print_motor_output(&drone_data);
    printf("\n\n");

    printf("Back full force\n");
    motor_output_from_controller(
        &drone_data,
        0.5, // X_in
        0, // Y_in
        0, // Limit scaler
        1  // Power scaler
    );
    print_motor_output(&drone_data);
    printf("\n\n");

    printf("Front right full force\n");
    motor_output_from_controller(
        &drone_data,
        1, // X_in
        1, // Y_in
        0, // Limit scaler
        1  // Power scaler
    );
    print_motor_output(&drone_data);
    printf("\n\n");

    printf("Front right full rotational\n");
    motor_output_from_controller(
        &drone_data,
        1, // X_in
        1, // Y_in
        1, // Limit scaler
        1  // Power scaler
    );
    print_motor_output(&drone_data);
    printf("\n\n");
}
#endif