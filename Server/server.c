#include <stdlib.h>
#include <string.h>
#include <stdio.h>

#include <server.h>

static struct s_drone_data* drones[MAX_DRONE_CONNECTIONS];

static int unity_socket = -1;
char network_message[NETWORK_STD_MSG_LEN]; // implement locking here

pthread_t threads[MAX_DRONE_CONNECTIONS];
static int threads_index = 0;

int main() {
    connect_to_unity_server();
    await_connections();

    // NOTE:
    //      This code is working as expected, sending messages to unity server
    //      TODO: clean up code and remove test messages, use actual drone codes
    //              implement message passing between drone and unity server
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
    while (TRUE) {
        // Second parameter can tell us where the client connection is coming from
        int client_socket = accept(server_socket, NULL, NULL);

        // Send request for new unity/drone port

        struct s_drone_data* new_drone = (struct s_drone_data*) calloc(1, sizeof(struct s_drone_data));
        init_drone_from_socket(new_drone, client_socket);
        add_drone_to_server(new_drone);
        printf("Drone %d connected on socket %d\n", get_drone_count(), client_socket);

        // Debug message
        sprintf(network_message, "You are drone #%d\n", get_drone_count());
        send(new_drone->socket, network_message, sizeof(network_message), 0); // last param is optional flags

        fflush(stdout);

        printf("Unity socket in await connections: %d\n", unity_socket);

        threads_index++;
        pthread_create(&threads[threads_index-1], NULL, listen_to_drone, new_drone);
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

void listen_to_drone(struct s_drone_data* drone) {
    while (TRUE) {
        recv(drone->socket, &network_message, sizeof(network_message), 0);

        switch (network_message[0]) {
        case CODE_SPAWN_DRONE:
            send_server_message(network_message);
            break;
        default:
            printf("Network code not implemented\n");
            break;
        }
    }
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

    printf("Connected to Unity server with unity_socket fd = %d\n\n", unity_socket);
}

int server_is_connected() {
    return unity_socket != -1;
}



void send_server_message(const char* msg) {
    unity_socket = 3;
    if (unity_socket == -1) {
        printf("Failed to send message, sim server not connected\n");
        return;
    }

    sprintf(network_message, msg);
    send(unity_socket, network_message, sizeof(network_message), 0);
}

void receive_server_messages() {
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