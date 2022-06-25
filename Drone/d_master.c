#include <d_master.h>
#include <assert.h>
#include <time.h>

struct drone_data drone_data;

int main() {
    srand (time(NULL)); // Initalize seed for random numbers

    printf("Initializing Drone\n");

    init_drone_data(&drone_data);

    int neural_size = 4;
    int neural_shape[] = {8, 8, 8, 4};
    int neural_activations[] = {
        ACTIVATION_RELU,
        ACTIVATION_RELU,
        ACTIVATION_SIGMOID
    };
    drone_data.neural = init_matrices_from_network_design(neural_size, neural_shape, neural_activations);

    init_server_socket(&drone_data);
    pack_msg_with_standard_header(drone_data.m_json, &drone_data, CODE_MOTOR_OUTPUT);

    drone_logic_loop();
}

void drone_logic_loop() {
    double motor_fl, motor_fr, motor_br, motor_bl;
    motor_fl = 0;
    motor_fr = 0;
    motor_br = 0;
    motor_bl = 0;

    ASSERT(DRONE_SENSOR_COUNT == network_input_layer_size(drone_data.neural));

    while (TRUE) {
        motor_output(motor_fl, motor_fr, motor_br, motor_bl, &drone_data);

        // Receive sensore data respone from server - position, distance sensors etc.
        char* json_in = receive_server_message(&drone_data);

        // TODO: Implement VSync - subtract computation time from last cycle to keep refresh rate constant
        usleep(32000); // sleep for 32 milliseconds - 30HZ

        // Read sensor data from server response
        read_sensor_data(&drone_data, json_in);

        // Set input layer to neural network
        for (int i = 0; i < network_input_layer_size(drone_data.neural); i++) {
            drone_data.neural->input_layer[i] = drone_data.sensor_array[i];
            printf(", %f", drone_data.neural->input_layer[i]);
        }
        printf("\n");

        // Feed forward data
        feed_forward_network(drone_data.neural);

        // Set motors from output
        // TODO: Create motor controller to convert neural output to motor values
        motor_bl = drone_data.neural->output_layer[0];
        motor_br = drone_data.neural->output_layer[1];
        motor_fr = drone_data.neural->output_layer[2];
        motor_fl = drone_data.neural->output_layer[3];
        printf("motors: %f, %f, %f, %f", motor_bl, motor_br, motor_fr, motor_fl);

        printf("\n\n\n\n");
    }
}

void read_sensor_data(struct drone_data* drone, char* json_string) {
    struct json_object* json_in = json_tokener_parse(json_string);

    struct json_object* sensor_data_array;
    struct json_object* sensor_data;
    json_object_object_get_ex(json_in, "sensorData", &sensor_data_array);
    
    size_t sensor_data_count = json_object_array_length(sensor_data_array);

    ASSERT(sensor_data_count == DRONE_SENSOR_COUNT);


    for (int i = 0; i < DRONE_SENSOR_COUNT; i++) {
        sensor_data = json_object_array_get_idx(sensor_data_array, i);
        drone->sensor_array[i] = json_object_get_double(sensor_data);
    }

    json_object_put(json_in);
}

void motor_output(double fl, double fr, double br, double bl, struct drone_data* drone) {
    struct json_object* json_fl;
    struct json_object* json_fr;
    struct json_object* json_br;
    struct json_object* json_bl;

    json_object_object_get_ex(drone->m_json, "motor_fl", &json_fl);
    json_object_object_get_ex(drone->m_json, "motor_fr", &json_fr);
    json_object_object_get_ex(drone->m_json, "motor_br", &json_br);
    json_object_object_get_ex(drone->m_json, "motor_bl", &json_bl);

    json_object_set_double(json_fl, fl);
    json_object_set_double(json_fr, fr);
    json_object_set_double(json_br, br);
    json_object_set_double(json_bl, bl);

    // Send server message
    send_server_json(drone, drone->m_json);
}