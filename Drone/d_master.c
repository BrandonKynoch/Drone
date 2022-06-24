#include <d_master.h>
#include <assert.h>
#include <time.h>

struct drone_data drone_data;

int main() {
    srand (time(NULL)); // Initalize seed for random numbers

    printf("Initializing Drone\n");

    init_drone_data(&drone_data);

    int neural_design[] = {4, 2, 8, 3, 2, 10, 9};
    struct network_data* network = init_matrices_from_network_design(7, neural_design);

    for (int i = 0; i < network->weights_matrix_count; i++) {
        char name[10];
        sprintf(name, "%d", i);

        print_matrix(
            name,
            network->weights_layers[i],
            network->weights_row_count[i],
            network->weights_col_count[i]
            );
    }

    // init_server_socket(&drone_data);
    // pack_msg_with_standard_header(drone_data.m_json, &drone_data, CODE_MOTOR_OUTPUT);

    // drone_logic_loop();

    // while (TRUE) {
    //     ;
    // }
}

void drone_logic_loop() {
    double motor_fl, motor_fr, motor_br, motor_bl;
    motor_fl = 0;
    motor_fr = 0;
    motor_br = 0;
    motor_bl = 0;

    while (TRUE) {
        motor_output(motor_fl, motor_fr, motor_br, motor_bl, &drone_data);

        // Receive respone from server - position, distance sensors etc.
        char* json_in = receive_server_message(&drone_data);

        // TODO: Implement VSync - subtract computation time from last cycle to keep refresh rate constant
        usleep(32000); // sleep for 32 milliseconds - 30HZ

        // Calculate motor values based on drone sensor data
        read_sensor_data(&drone_data, json_in);

        printf("%f", drone_data.sensor_array[0]);
        for (int i = 1; i < DRONE_SENSOR_COUNT; i++) {
            printf(", %f", drone_data.sensor_array[i]);
        }
        printf("\n");

        motor_bl = DRONE_SENSOR_RANGE - drone_data.sensor_array[7];
        motor_br = DRONE_SENSOR_RANGE - drone_data.sensor_array[1];
        motor_fr = DRONE_SENSOR_RANGE - drone_data.sensor_array[3];
        motor_fl = DRONE_SENSOR_RANGE - drone_data.sensor_array[5];
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