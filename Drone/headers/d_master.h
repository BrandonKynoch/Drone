#include <stdlib.h>

#include <d_networking.h>

void drone_logic_loop();

void read_sensor_data(struct drone_data* drone, char* json_string);
void motor_output(double fl, double fr, double br, double bl, struct drone_data* drone);