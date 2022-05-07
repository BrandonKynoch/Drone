#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>

#include <sys/types.h>
#include <sys/socket.h>

#include <netinet/in.h>

#define MAX_DRONE_CONNECTIONS 25

void await_connections ();
void add_drone_to_server ();

// Utils
int get_drone_count();