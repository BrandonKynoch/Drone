#include <stdlib.h>
#include <string.h>
#include <stdio.h>

#include <server.h>

static struct drone_data* drones[MAX_DRONE_CONNECTIONS];

static int unity_socket = -1;
char network_message[NETWORK_STD_MSG_LEN]; // implement locking here

pthread_t threads[MAX_DRONE_CONNECTIONS];
static int threads_index = 0;

int main() {
    connect_to_unity_server();
    await_connections();
}

// When a drone connects the server responds by sending the drone its droneID
void await_connections () {
    // Create the server socket
    int server_socket = socket(AF_INET, SOCK_STREAM, 0);

    // define the server address
    struct sockaddr_in server_address;
    server_address.sin_family = AF_INET;
    server_address.sin_port = htons(DRONE_SERVER_SOCKET);
    server_address.sin_addr.s_addr = INADDR_ANY;

    // bind the socket to our specified IP and port
    bind(server_socket, (struct socketaddr*) &server_address, sizeof(server_address));

    listen(server_socket, MAX_DRONE_CONNECTIONS);

    // Wait for new drones to connect
    while (TRUE) {
        // Second parameter can tell us where the client connection is coming from
        int client_socket = accept(server_socket, NULL, NULL);

        struct drone_data* new_drone = (struct drone_data*) calloc(1, sizeof(struct drone_data));
        add_drone_to_c_server(new_drone);
        new_drone->socket = client_socket;
        printf("Drone %d connected on socket %d with id %d\n", get_drone_count(), new_drone->socket, new_drone->id);

        // Respond with drone id
        struct json_object* json_out = json_object_new_object();
        json_object_object_add(json_out, "id", json_object_new_uint64(new_drone->id));
        char* json_string = json_object_to_json_string_ext(json_out, JSON_C_TO_STRING_PLAIN);

        threads_index++;
        pthread_create(&threads[threads_index-1], NULL, listen_to_drone, new_drone);

        strcpy(network_message, json_string);
        send(new_drone->socket, network_message, sizeof(network_message), 0); // last param is optional flags

        fflush(stdout);

        json_object_put(json_out);
    }
}

void add_drone_to_c_server(struct drone_data* drone) {
    // find free index
    int i = 0;
    while (i < MAX_DRONE_CONNECTIONS) {
        if (drones[i] == NULL) {
            break;
        }
        i++;
    }

    drones[i] = drone;
    drone->id = i;
}

// Listen to the c-drone code and forward data to unity simulation
void listen_to_drone(struct drone_data* drone) {
    while (TRUE) {
        recv(drone->socket, &network_message, sizeof(network_message), 0);
        send_server_fixed_message();
    }
}

// Send a message to the c-drone
void send_drone_message(struct drone_data* drone, char* message) {
    send(drone->socket, message, NETWORK_STD_MSG_LEN, 0);
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

    printf("Connected to Unity server with unity_socket fd: %d\n\n", unity_socket);

    threads_index++;
    pthread_create(&threads[threads_index-1], NULL, receive_server_messages, NULL);
}

int server_is_connected() {
    return unity_socket != -1;
}



void send_server_message(const char* msg) {
    if (unity_socket == -1) {
        printf("Failed to send message, sim server not connected\n");
        return;
    }

    sprintf(network_message, msg);
    send(unity_socket, network_message, sizeof(network_message), 0);
    clear_message_buffer();
}

void send_server_fixed_message() {
    send(unity_socket, network_message, sizeof(network_message), 0);
    clear_message_buffer();
}

void receive_server_messages() {
    char unity_received_message[NETWORK_STD_MSG_LEN];

    // First byte of message is drone ID
    while (TRUE) {
        recv(unity_socket, &unity_received_message, sizeof(unity_received_message), 0);

        struct json_object* json_in = json_tokener_parse(unity_received_message);
        struct json_object* json_id;
        json_object_object_get_ex(json_in, "id", &json_id);

        int target_drone_id = json_object_get_int(json_id);
        // Check that target drone id is valid before dereferrencing
        send_drone_message(drones[target_drone_id], &unity_received_message);

        json_object_put(json_in);
    }
}

void clear_message_buffer() {
    for (int i = 0; i < NETWORK_STD_MSG_LEN; i++) {
        network_message[i] = '\0';
    }
}

// ############################################################################################################
// ######    END SEVER SIMULATION    ##########################################################################
// ############################################################################################################