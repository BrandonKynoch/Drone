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
    char* target_NN_file = request_target_NN_from_server();
    init_and_test_NN_from_file(target_NN_file);
#else
    // TODO: load NN directly from file on device
#endif

    drone_logic_loop();
}

void init_and_test_NN_from_file(char* file) {
    init_neural_data(file, &drone_data.neural);
    printf("NN init check\n");
    feed_forward_network(drone_data.neural);
    printf("NN feedforward check\n");
    print_matrix("NN output", drone_data.neural->output_layer, drone_data.neural->weights_row_count[drone_data.neural->weights_matrix_count-1], 1);
    printf("\n\n");

    printf("Creating sensor timebuffer\n");

    // input layer size 
    // drone_data.neural->weights_row_count[0]
    int sensor_input_size = DRONE_CIRCLE_SENSOR_COUNT + 2;
    ASSERT((int) (drone_data.neural->weights_row_count[0]) % sensor_input_size == 0);

    int timesteps = (int) (drone_data.neural->weights_row_count[0] / sensor_input_size) - 1; // IMPORTANT: Remember to -1
    drone_data.sensor_time_buffer = init_timebuffer(sensor_input_size, timesteps);
}

void drone_logic_loop() {
    ASSERT(network_input_layer_size(drone_data.neural) % (DRONE_CIRCLE_SENSOR_COUNT + 2) == 0);

    char* server_response;
    struct json_object* json_response;
    struct json_object* response_opcode_json;
    int32_t response_opcode;

    struct json_object* response_NN_file_json;
    char* response_NN_file;

    while (TRUE) {
        motor_output(&drone_data);

        // Receive sensore data respone from server - position, distance sensors etc.
        server_response = receive_server_message(&drone_data);

        // TODO: Implement VSync - subtract computation time from last cycle to keep refresh rate constant
        // usleep(32000); // sleep for 32 milliseconds - 30HZ
        usleep(16000);

        // Decode server response
        json_response = json_tokener_parse(server_response);

        json_object_object_get_ex(json_response, "opcode", &response_opcode_json);
        
        response_opcode = json_object_get_int(response_opcode_json);

        switch (response_opcode) {
            case RESPONSE_CODE_LOAD_NN:
                free(drone_data.neural);

                json_object_object_get_ex(json_response, "file", &response_NN_file_json);
                
                response_NN_file = json_object_get_string(response_NN_file_json);

                init_and_test_NN_from_file(response_NN_file);
                break;
            case RESPONSE_CODE_SENSOR_DATA:
                // Read sensor data from server response
                read_sensor_data(&drone_data, json_response);

                // Set input layer to neural network
                set_NN_input_from_sensor_data(&drone_data);

                // Feed forward data
                feed_forward_network(drone_data.neural);

                // Set motors from output
                motor_output_from_controller(
                    &drone_data,
                    drone_data.neural->output_layer[0], // X_in
                    drone_data.neural->output_layer[1], // Y_in
                    drone_data.neural->output_layer[2], // limit_scaler
                    drone_data.neural->output_layer[3]  // power_scaler
                );

                printf("motors: %f, %f, %f, %f\n", drone_data.m_fl, drone_data.m_fr, drone_data.m_br, drone_data.m_bl);
                break;
            default:
                fprintf(stderr, "Unhandled server response");
                return;
        }

        json_object_put(json_response);
    }
}

void read_sensor_data(struct drone_data* drone, struct json_object* json_in) {
    struct json_object* sensor_data_array;
    struct json_object* sensor_data;
    json_object_object_get_ex(json_in, "circleSensorData", &sensor_data_array);
    
    size_t cirlce_sensor_data_count = json_object_array_length(sensor_data_array);

    ASSERT(cirlce_sensor_data_count == DRONE_CIRCLE_SENSOR_COUNT);

    for (int i = 0; i < DRONE_CIRCLE_SENSOR_COUNT; i++) {
        sensor_data = json_object_array_get_idx(sensor_data_array, i);
        drone->circle_sensor_array[i] = json_object_get_double(sensor_data);
    }

    struct json_object* sensor_top;
    json_object_object_get_ex(json_in, "sensorTop", &sensor_top);
    drone->sensor_top = json_object_get_double(sensor_top);

    struct json_object* sensor_bottom;
    json_object_object_get_ex(json_in, "sensorBottom", &sensor_bottom);
    drone->sensor_bottom = json_object_get_double(sensor_bottom);
}

void set_NN_input_from_sensor_data(struct drone_data* drone) {
    timebuffer_set(drone->sensor_time_buffer, drone_data.circle_sensor_array, DRONE_CIRCLE_SENSOR_COUNT, 0);
    drone->sensor_time_buffer->buffer[DRONE_CIRCLE_SENSOR_COUNT] = drone_data.sensor_top;
    drone->sensor_time_buffer->buffer[DRONE_CIRCLE_SENSOR_COUNT + 1] = drone_data.sensor_bottom;
    timebuffer_increment(drone->sensor_time_buffer);

    timebuffer_copy_corrected(drone->sensor_time_buffer, drone_data.neural->input_layer);
}

void motor_output_from_controller(struct drone_data* drone, double x_in, double y_in, double limit_scaler, double power_scaler) {
    // Remap values from    0 - 1      to      -1 - 1
    double x_out = (2 * x_in) - 1;
    double y_out = (2 * y_in) - 1;

    double v_length = sqrt(pow(x_out, 2) + pow(y_out, 2));

    // Normalize
    // if (v_length)
    // x_out = x_out / v_length;
    // y_out = y_out / v_length;

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
    return sqrt(pow(m_pos_x - direction_x, 2) + pow(m_pos_y - direction_y , 2));
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






#if IS_SIMULATION
char* request_target_NN_from_server() {
    // Send request to server
    struct json_object* request_json = json_object_new_object();
    pack_msg_with_standard_header(request_json, &drone_data, CODE_REQUEST_TARGET_NN_FROM_SERVER);
    send_server_json(&drone_data, request_json);
    
    // Receive response from server
    struct json_object* response_json = receive_server_json(&drone_data);

    // Decode response
    struct json_object* json_file_addr;
    json_object_object_get_ex(response_json, "file", &json_file_addr);
    char* target_file = json_object_get_string(json_file_addr);

    // Reallocate and copy result
    char* target_file_copy = malloc(strlen(target_file));
    sprintf(target_file_copy, target_file);

    // Free memory
    json_object_put(request_json);
    json_object_put(response_json);

    printf("NN target file:\n\t%s\n", target_file_copy);

    return target_file_copy;
}
#endif