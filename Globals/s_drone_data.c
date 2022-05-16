#include <s_drone_data.h>

struct s_drone_data* init_drone_from_socket (int socket) {
    struct s_drone_data* d = (struct s_drone_data*) calloc(1, sizeof(struct s_drone_data));
    d->socket = socket;
    return d;
}