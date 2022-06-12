#include <stdlib.h>

// Libs
#include <json-c/json.h>

#define CODE_SPAWN_DRONE 0x1
#define CODE_MOTOR_OUTPUT 0x2

struct drone_data {
    uint64_t id;

    int x, y, z;
    int socket;

    double m_fl, m_fr, m_br, m_bl; // Motor outputs
    struct json_object* m_json; // Motor json for unity sim
};

void init_drone_data(struct drone_data* d);

void pack_msg_with_standard_header(struct json_object* json, struct drone_data* drone, int opcode);