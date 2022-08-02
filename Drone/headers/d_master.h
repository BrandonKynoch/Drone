#include <stdlib.h>
#include <sys/time.h>

#include <d_networking.h>
#include <d_neural.h>
#include <globals.h>
#include <utilities.h>

#include "time_buffer.h"

// ############################################################################
// #######      DRONE MAIN      ###############################################
// ############################################################################

void drone_logic_loop();

// Reset all sensor readings
void init_drone_sensor_data(struct drone_data* drone);

// Read sensor data from simulation
void read_sensor_data(struct drone_data* drone, struct json_object* json_in);

// Sets the input layer for sensor data NN
void set_NN_input_from_sensor_data(struct drone_data* drone);

// Convert vector direction (x_in, y_in) & limit_scaler & power_scaler to 4 motor output values
// Used to convert output layer of neural network to motor values
void calculate_motor_output_from_controller(
    struct drone_data* drone,
    double x_in,
    double y_in,
    double limit_scaler,
    double power_scaler
);

// Compute the motor output value based on:
//      - the input direction vector: (direction_x, direction_y)
//      - the actual position of the motor relative to the center of the drone (m_pos_x, m_pos_y)
double compute_motor_output_from_offset(double direction_x, double direction_y, double m_pos_x, double m_pos_y);

// Compute the motor output by remapping motor values based on
//      - m_mean :: The mean motor output before remapping
//      - power_scaler :: Determines the amplitude of motor output
//      - limit_scaler :: Determines the lower bound, and therefore the range of motor outputs
double compute_motor_output_from_scalers(double m_in, double m_mean, double power_scaler, double limit_scaler);

void calculate_motor_output_autopilot_correction(struct drone_data* drone, double amount);

// Set drone motor json either send message to simulation or write to gpio pins
void motor_output(struct drone_data* drone);

void print_motor_output(struct drone_data* drone);

// ############################################################################
// #######      DRONE MAIN      ###############################################
// ############################################################################




// ############################################################################
// #######      SIMULATION      ###############################################
// ############################################################################

// Initialize and attempt to load the given NN file
void init_and_test_NN_from_folder(char* folder);

// Send a request to the server to provide the drone with a NN file to load
// returns the file address of the NN to load
char* request_target_NN_folder_from_server();

void assign_double_from_json(struct json_object* json_in, const char* field_name, double* d);

void test_motor_controller();

// ############################################################################
// #######      SIMULATION      ###############################################
// ############################################################################