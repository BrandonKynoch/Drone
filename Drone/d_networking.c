#include <globals.h>
#include <d_networking.h>

char network_message[NETWORK_STD_MSG_LEN];

void init_server_socket(struct s_drone_data* drone_data) {
    // Create a socket
    // AF_INET -> inetnet socket (This parameter defines the domain of the socket)
    // SOCK_STREAM for TCP (Type of socket)
    // 0 (Protocol - 0 for default protocol)
    int network_socket = socket(AF_INET, SOCK_STREAM, 0);

    // Specify an address for the socket
    struct sockaddr_in server_address;
    server_address.sin_family = AF_INET;
    server_address.sin_port = htons(8060); // htons converts integer port number to appropriate data format
    server_address.sin_addr.s_addr = INADDR_ANY; // INADDR_ANY for localport (i.e. on same machine) = equal to 0.0.0.0

    int connection_status = connect(network_socket, (struct sockaddr *) &server_address, sizeof(server_address)); // returns 0 for success

    // Print error if connection fails
    if (connection_status != 0) {
        if (connection_status == -1) {
            printf("Failled to connect to socket\n\n");
        } else {
            printf("Socket connection status: %d", connection_status);
        }
        return;
    }

    // Receive data from the server - First message should be debug message
    recv(network_socket, &network_message, sizeof(network_message), 0); // Last parameter is for flags, this is optional
    printf(network_message);

    printf("\n\nUsing socket %d\n\n", network_socket);

    init_drone_from_socket(drone_data, network_socket);

    // Get x, y, z coords 
    spawn_in_unity_server(drone_data);
}

void spawn_in_unity_server(struct s_drone_data* drone_data) {
    network_message[0] = (char) CODE_SPAWN_DRONE;
    network_message[1] = '\0';
    send_server_fixed_message(drone_data);
    receive_server_message(drone_data);
    printf("Received message from unity sim:\n\t%s\n", network_message);
}



void send_server_fixed_message(struct s_drone_data* drone) {
    send(drone->socket, network_message, sizeof(network_message), 0);
}

void send_server_message(struct s_drone_data* drone, const char* msg) {
    sprintf(network_message, msg);
    send(drone->socket, network_message, sizeof(network_message), 0);
}

void receive_server_message(struct s_drone_data* drone) {
    recv(drone->socket, &network_message, sizeof(network_message), 0);
}