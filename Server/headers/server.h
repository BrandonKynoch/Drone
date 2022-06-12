#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <pthread.h>

#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>

#include <json-c/json.h>

// Local code
#include <globals.h>
#include <drone_data.h>

#define MAX_DRONE_CONNECTIONS 25

void await_connections ();
void add_drone_to_c_server ();

void listen_to_drone(struct drone_data* drone);
void send_drone_message(struct drone_data* drone, char* message);

// Utils
int get_drone_count();

// Unity Sim
void connect_to_unity_server();
int server_is_connected();

void send_server_message(const char* msg);
void send_server_fixed_message();
void receive_server_messages();

void clear_message_buffer();