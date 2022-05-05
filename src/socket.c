#include <socket.h>

void socket_test() {
    printf("Socket test func\n\n");

    init_socket();
}

void init_socket() {
    // Create a socket
    int network_socket;
    // AF_INET -> inetnet socket (This parameter defines the domain of the socket)
    // SOCK_STREAM for TCP (Type of socket)
    // 0 (Protocol - 0 for default protocol)
    network_socket = socket(AF_INET, SOCK_STREAM, 0);

    // Specify an address for the socket
    struct sockaddr_in server_address;
    server_address.sin_family = AF_INET;
    server_address.sin_port = htons(8060); // htons converts integer port number to appropriate data format
    server_address.sin_addr.s_addr = INADDR_ANY; // INADDR_ANY for localport (i.e. on same machine) = equal to 0.0.0.0

    int connection_status = connect(network_socket, (struct sockaddr *) &server_address, sizeof(server_address)); // returns 0 for success

    if (connection_status != 0) {
        if (connection_status == -1) {
            printf("Failled to connect to socket\n\n");
        } else {
            printf("Socket connection status: %d", connection_status);
        }
    }

    // Receive data from the server
    char server_response[256];
    recv(network_socket, &server_response, sizeof(server_response), 0); // Last parameter is for flags, this is optional

    printf("Server response: %s\n", server_response);

    close(network_socket);
}