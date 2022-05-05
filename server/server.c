#include <server.h>

int main() {
    char server_message[256] = "Hello Dingus\n";

    // Create the server socket
    int server_socket;
    server_socket = socket(AF_INET, SOCK_STREAM, 0);

    // define the server address
    struct sockaddr_in server_address;
    server_address.sin_family = AF_INET;
    server_address.sin_port = htons(8060);
    server_address.sin_addr.s_addr = INADDR_ANY;

    // bind the socket to our specified IP and port
    bind(server_socket, (struct socketaddr *) &server_address, sizeof(server_address));

    listen(server_socket, 5); // Number of available connections

    int client_socket;
    // Second parameter can tell us where the client connection is coming from
    client_socket = accept(server_socket, NULL, NULL);

    send(client_socket, server_message, sizeof(server_message), 0); // last param is optional flags

    //close(client_socket);
    close(server_socket);

    return 0;
}