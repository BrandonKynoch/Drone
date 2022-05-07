#include <stdlib.h>

struct s_drone_data {
    int x, y, z;
    int socket;
};

struct s_drone_data* init_drone_from_socket (int socket);