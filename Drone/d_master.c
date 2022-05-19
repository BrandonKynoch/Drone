#include <d_master.h>

struct s_drone_data drone_data;

int main() {
    printf("Initializing Drone\n");

    init_server_socket(&drone_data);

    drone_logic_loop();

    while (TRUE) {
        ;
    }
}

void drone_logic_loop() {

}