#include <d_master.h>

struct s_drone_data drone_data;

int main() {
    printf("Initializing Drone\n");

    init_drone_data(&drone_data);

    init_server_socket(&drone_data);

    drone_logic_loop();

    while (TRUE) {
        ;
    }
}

void drone_logic_loop() {
    while (TRUE) {
        // Calculate motor values based on drone sensor data

        motor_output(1, 2, 3, 4, &drone_data);
        
        // Receive respone from server - position, distance sensors etc.

        usleep(16000); // sleep for 16 milliseconds - 60HZ
    }
}

void motor_output(double fl, double fr, double br, double bl, struct s_drone_data* drone) {
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