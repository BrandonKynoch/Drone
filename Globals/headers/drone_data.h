#include <stdlib.h>

// Libs
#include <json-c/json.h>

#include <globals.h>

#define CODE_SPAWN_DRONE 0x1
#define CODE_MOTOR_OUTPUT 0x2

struct drone_data {
    uint64_t id;
    int socket;

    double sensor_array[DRONE_SENSOR_COUNT];

    double m_fl, m_fr, m_br, m_bl; // Motor outputs
    struct json_object* m_json; // Motor json for unity sim

    // Used strictly in server.c
    char unity_received_message[NETWORK_STD_MSG_LEN];
};

void init_drone_data(struct drone_data* d);

void pack_msg_with_standard_header(struct json_object* json, struct drone_data* drone, int opcode);