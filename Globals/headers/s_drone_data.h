#include <stdlib.h>

#define CODE_SPAWN_DRONE 0x1

struct s_drone_data {
    int x, y, z;
    int socket;
};

void init_drone_from_socket (struct s_drone_data* drone, int socket);