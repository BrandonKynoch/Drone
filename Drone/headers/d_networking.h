#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <string.h>

#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>

// Project code
#include <globals.h>

#include <s_drone_data.h>

// Libs
#include <json-c/json.h>

// Connect to socket on server
void init_server_socket(struct s_drone_data* drone_data);
void spawn_in_unity_server(struct s_drone_data* drone_data);

void send_server_fixed_message(struct s_drone_data* drone);
void send_server_message(struct s_drone_data* drone, const char* msg);
void receive_server_message(struct s_drone_data* drone);