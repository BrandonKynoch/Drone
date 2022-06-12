#include <globals.h>
#include <d_networking.h>

char network_message[NETWORK_STD_MSG_LEN];

void init_server_socket(struct drone_data* drone_data) {
    // Create a socket
    // AF_INET -> inetnet socket (This parameter defines the domain of the socket)
    // SOCK_STREAM for TCP (Type of socket)
    // 0 (Protocol - 0 for default protocol)
    int network_socket = socket(AF_INET, SOCK_STREAM, 0);

    // Specify an address for the socket
    struct sockaddr_in server_address;
    server_address.sin_family = AF_INET;
    server_address.sin_port = htons(DRONE_SERVER_SOCKET); // htons converts integer port number to appropriate data format
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

    // Unpack server response
    recv(network_socket, &network_message, sizeof(network_message), 0); // Last parameter is for flags, this is optional
    // printf("\nServer Response:\n%s\n\n", network_message);
    
    struct json_object* json_in = json_tokener_parse(network_message);
    struct json_object* json_id;
    json_object_object_get_ex(json_in, "id", &json_id);
    uint64_t response_id  = json_object_get_uint64(json_id);
    json_object_put(json_in);

    drone_data->socket = network_socket;
    drone_data->id = response_id;

    printf("Connected to server on socket: %d\n\t\tWith drone ID: %d\n", drone_data->socket, drone_data->id);

    spawn_in_unity_server(drone_data);
}

void spawn_in_unity_server(struct drone_data* drone_data) {
    // Format message
    struct json_object* json = json_object_new_object();
    json_object_object_add(json, "opcode", json_object_new_int(CODE_SPAWN_DRONE));
    json_object_object_add(json, "id", json_object_new_uint64(drone_data->id));
    char* json_string = json_object_to_json_string_ext(json, JSON_C_TO_STRING_PLAIN);

    printf("Connecting to simulation\n");

    send_server_message(drone_data, json_string);
    receive_server_message(drone_data);

    printf("Connected to simulation\n");

    json_object_put(json);

    json = json_tokener_parse(network_message);
    char* json_string_receiveived = json_object_to_json_string_ext(json, JSON_C_TO_STRING_SPACED | JSON_C_TO_STRING_PRETTY);
    printf("Received spawn response from unity sim:\n%s\n", json_string_receiveived);

    json_object_put(json);
}



void send_server_fixed_message(struct drone_data* drone) {
    send(drone->socket, network_message, sizeof(network_message), 0);
}

void send_server_message(struct drone_data* drone, const char* msg) {
    sprintf(network_message, msg);
    send(drone->socket, network_message, sizeof(network_message), 0);
}

void send_server_json(struct drone_data* drone, struct json_object* json) {
    char* json_string = json_object_to_json_string_ext(json, JSON_C_TO_STRING_PLAIN);
    send_server_message(drone, json_string);
}

void receive_server_message(struct drone_data* drone) {
    recv(drone->socket, &network_message, sizeof(network_message), 0);
}