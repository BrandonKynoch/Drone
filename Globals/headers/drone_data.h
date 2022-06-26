#include <stdlib.h>
#include <globals.h>

// Libs
#include <json-c/json.h>

// OPCODES FOR SENDING MESSAGES TO SERVER
#define CODE_SPAWN_DRONE 0x1
#define CODE_MOTOR_OUTPUT 0x2
#define CODE_REQUEST_TARGET_NN_FROM_SERVER 0x3

// OPCODES FOR RESPONSES FROM SERVER
#define RESPONSE_CODE_SENSOR_DATA 0x4
#define RESPONSE_CODE_LOAD_NN 0x5 // Reload the provided neural network

struct drone_data {
    uint64_t id;
    int socket;

    struct neural_data* neural;

    double circle_sensor_array[DRONE_CIRCLE_SENSOR_COUNT];
    double sensor_top;
    double sensor_bottom;

    double m_fl, m_fr, m_br, m_bl; // Motor outputs
    struct json_object* m_json; // Motor json for unity sim

    // Used strictly in server.c
    char unity_received_message[NETWORK_STD_MSG_LEN];
};

void init_drone_data(struct drone_data* d);

void pack_msg_with_standard_header(struct json_object* json, struct drone_data* drone, int opcode);