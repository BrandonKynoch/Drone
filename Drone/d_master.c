#include <d_networking.h>

struct s_drone_data drone_data;

int main() {
    printf("Initializing Drone\n");

    init_server_socket(&drone_data);

    while (TRUE) {
        ;
    }
}