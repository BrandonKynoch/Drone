#include <stdlib.h>

#include <d_networking.h>
#include <d_neural.h>

// ############################################################################
// #######      DRONE MAIN      ###############################################
// ############################################################################

void drone_logic_loop();
void read_sensor_data(struct drone_data* drone, struct json_object* json_in);
void motor_output(double fl, double fr, double br, double bl, struct drone_data* drone);

// ############################################################################
// #######      DRONE MAIN      ###############################################
// ############################################################################




// ############################################################################
// #######      SIMULATION      ###############################################
// ############################################################################

// Initialize and attempt to load the given NN file
void init_and_test_NN_from_file(char* file);

// Send a request to the server to provide the drone with a NN file to load
// returns the file address of the NN to load
char* request_target_NN_from_server();

// ############################################################################
// #######      SIMULATION      ###############################################
// ############################################################################