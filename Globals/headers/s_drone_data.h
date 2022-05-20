#include <stdlib.h>

#define CODE_SPAWN_DRONE 0x1

struct s_drone_data {
    uint64_t id;

    int x, y, z;
    int socket;
};