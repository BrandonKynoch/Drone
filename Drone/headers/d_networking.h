#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>

#include <globals.h>

#include <sys/types.h>
#include <sys/socket.h>

#include <netinet/in.h>

#include <s_drone_data.h>

// Connect to socket on server
void init_server_socket(struct s_drone_data* drone_data);

void init_drone_data (struct s_drone_data* drone_data, int socket);