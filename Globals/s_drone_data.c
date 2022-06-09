#include <s_drone_data.h>

void init_drone_data(struct s_drone_data* d) {
    d->m_json = json_object_new_object();
    pack_msg_with_standard_header(d->m_json, d, CODE_MOTOR_OUTPUT);

    json_object_object_add(d->m_json, "motor_fl", json_object_new_double(0));
    json_object_object_add(d->m_json, "motor_fr", json_object_new_double(0));
    json_object_object_add(d->m_json, "motor_br", json_object_new_double(0));
    json_object_object_add(d->m_json, "motor_bl", json_object_new_double(0));
}

void pack_msg_with_standard_header(struct json_object* json, struct s_drone_data* drone, int opcode) {
    json_object_object_add(json, "opcode", json_object_new_int(opcode));
    json_object_object_add(json, "id", json_object_new_uint64(drone->id));
}