#ifndef HEADER_DRONE_DATA_H
#define HEADER_DRONE_DATA_H

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


#define TARGET_TICK_DURATION 32000 // Used to keep constant refresh rate in main loop (VSync) (Target time per itteration in milliseconds)
#define ROTATION_NEURAL_SET_TICKER_STRIDE 1
#define DISTANCE_NEURAL_SET_TICKER_STRIDE 6 // Only set dist_to_target_buffer (timebuffer) every n ticks
#define VELOCITY_NEURAL_SET_TICKER_STRIDE 3

struct drone_data {
    uint64_t id;
    int socket;

    struct neural_data* sensor_neural;
    struct neural_data* rotation_neural;
    struct neural_data* distance_neural;
    struct neural_data* velocity_neural;

    struct neural_data* combine_neural;

    /// SENSOR READINGS /////////////////////////////////////////
    double circle_sensor_array[DRONE_CIRCLE_SENSOR_COUNT];
    double sensor_top;
    double sensor_bottom;
    /// SENSOR READINGS /////////////////////////////////////////

    /// ROTATION READINGS ///////////////////////////////////////
    double rotation_x;
    double rotation_y;
    double rotation_z;
    /// ROTATION READINGS ///////////////////////////////////////

    /// DISTANCE READINGS ///////////////////////////////////////
    double dist_to_target;
    /// DISTANCE READINGS ///////////////////////////////////////    

    /// VELOCITY READINGS ///////////////////////////////////////
    double velocity;
    /// VELOCITY READINGS ///////////////////////////////////////

    /// NN INPUTS ///////////////////////////////////////////////
    struct time_buffer* sensor_time_buffer; // Set every 32 milliseconds
    struct time_buffer* rotation_time_buffer; // Set every 32 milliseconds
    struct time_buffer* dist_to_target_buffer; // Set every 192 milliseconds
    struct time_buffer* velocity_time_buffer; // Set every 96 milliseconds
    /// NN INPUTS ///////////////////////////////////////////////

    double m_fl, m_fr, m_br, m_bl; // Motor outputs
    struct json_object* m_json; // Motor json for unity sim

    uint32_t ticker; // Ticks for each iteration in the main loop - Is reset to zero periodically in the main loop to prevent overflow

    // Used strictly in server.c
    char unity_received_message[NETWORK_STD_MSG_LEN];
};

void init_drone_data(struct drone_data* d);

void pack_msg_with_standard_header(struct json_object* json, struct drone_data* drone, int opcode);

#endif