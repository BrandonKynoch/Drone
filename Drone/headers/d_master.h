#include <stdlib.h>

#include <d_networking.h>
#include <d_neural.h>

// ############################################################################
// #######      DRONE MAIN      ###############################################
// ############################################################################

void drone_logic_loop();
void read_sensor_data(struct drone_data* drone, char* json_string);
void motor_output(double fl, double fr, double br, double bl, struct drone_data* drone);

// ############################################################################
// #######      DRONE MAIN      ###############################################
// ############################################################################




// ############################################################################
// #######      SIMULATION      ###############################################
// ############################################################################

// Send a request to the server to provide the drone with a NN file to load
// returns the file address of the NN to load
char* request_target_NN_from_server();

// ############################################################################
// #######      SIMULATION      ###############################################
// ############################################################################