#include <s_drone_data.h>

void init_drone_from_socket (struct s_drone_data* drone, int socket) {
    drone->socket = socket;
    // Do any other initialization for drone before spawning in unity sim
}