#include <stdlib.h>
#include <string.h>
#include <stdio.h>

#include <globals.h>
#include <server.h>
#include <s_drone_data.h>

struct s_drone_data* drones[MAX_DRONE_CONNECTIONS];

int unity_socket = -1;
char network_message[NETWORK_STD_MSG_LEN];

int main() {
    int fork_val = fork();
    if (fork_val == -1) {
        printf("Failed to fork while connecting to unity server\n");
        return -1;
    }

    if (fork_val == 0) {
        connect_to_unity_server();
    } else {
        await_connections(); // Wait for drones to connect
    }
}

void await_connections () {
    // Create the server socket
    int server_socket = socket(AF_INET, SOCK_STREAM, 0);

    // define the server address
    struct sockaddr_in server_address;
    server_address.sin_family = AF_INET;
    server_address.sin_port = htons(8060);
    server_address.sin_addr.s_addr = INADDR_ANY;

    // bind the socket to our specified IP and port
    bind(server_socket, (struct socketaddr*) &server_address, sizeof(server_address));

    listen(server_socket, MAX_DRONE_CONNECTIONS);

    // Wait for new drones to connect
    while (1) {
        // Second parameter can tell us where the client connection is coming from
        int client_socket = accept(server_socket, NULL, NULL);

        struct s_drone_data* new_drone = init_drone_from_socket(client_socket);
        add_drone_to_server(new_drone);
        printf("Drone %d connected on socket %d\n", get_drone_count(), client_socket);

        // Debug message
        sprintf(network_message, "You are drone #%d\n", get_drone_count());
        send(new_drone->socket, network_message, sizeof(network_message), 0); // last param is optional flags

        fflush(stdout);
    }
}

void add_drone_to_server(struct s_drone_data* drone) {
    /// 1: find free index ///
    int i = 0;
    while (i < MAX_DRONE_CONNECTIONS) {
        if (drones[i] == NULL) {
            break;
        }
        i++;
    }
    /// 1

    drones[i] = drone;
}





// ############################################################################################################
// ######    UTILS    #########################################################################################
// ############################################################################################################
int get_drone_count() {
    int count = 0;
    for (int i = 0; i < MAX_DRONE_CONNECTIONS; i++) {
        if (drones[i] != NULL)
            count++;
    }
    return count;
}

// ############################################################################################################
// ######    END UTILS    #####################################################################################
// ############################################################################################################


// ############################################################################################################
// ######    SEVER SIMULATION    ##############################################################################
// ############################################################################################################

void connect_to_unity_server() {
    printf("Trying to connect to Unity server\n");
    fflush(stdout);

    if ((unity_socket = socket(AF_INET, SOCK_STREAM, 0)) < 0) {
        perror("cannot create socket\n");
    }
    
    struct sockaddr_in servaddr;    /* server address */

    /* fill in the server's address and data */
    memset((char*)&servaddr, 0, sizeof(servaddr));
    servaddr.sin_family = AF_INET;
    servaddr.sin_port = htons(SIMULATION_SERVER_SOCKET);
    servaddr.sin_addr.s_addr = INADDR_ANY;
    
    /* connect to server */
    if (connect(unity_socket, (struct sockaddr *)&servaddr, sizeof(servaddr)) < 0) {
        perror("Failed to connect to Unity server\n");
        return;
    }

    printf("Connected to Unity server\n\n");
}

int server_is_connected() {
    return unity_socket != -1;
}



void send_server_message(const char* msg) {
    if (unity_socket == -1) {
        printf("Failed to send message, sim server not connected");
        return;
    }

    sprintf(network_message, msg);
    send(unity_socket, network_message, sizeof(network_message), 0);
}

void receive_response_from_server() {
    // // Receive data from the server
    // char server_response[NETWORK_STD_MSG_LEN];
    // recv(fd, &server_response, sizeof(server_response), 0); // Last parameter is for flags, this is optional
}

// ############################################################################################################
// ######    END SEVER SIMULATION    ##########################################################################
// ############################################################################################################































/*
void old_server_test() {
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

    close(server_socket);

    return 0;
}*/