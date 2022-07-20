#include <d_neural.h>

#include <stdio.h>
#include <stdlib.h>

#define WEIGHT_INIT_MIN_VAL -1.0
#define WEIGHT_INIT_MAX_VAL 1.0

#define BIAS_INIT_MIN_VAL -1.0
#define BIAS_INIT_MAX_VAL 1.0

// ############################################################################
// #######      INITIALIZATION      ###########################################
// ############################################################################

void init_all_neural_data_in_dir(const char* enclosing_dir, struct drone_data* drone) {
    char* file = malloc(MAX_FILE_PATH_LENGTH);

    // Sensor.NN
    sprintf(file, "%s/Sensor.NN\0", enclosing_dir);
    if (access(file, F_OK) == 0) {
        // File exists
        init_neural_data_from_file(file, &drone->sensor_neural);
    } else {
        // File does not exist
        init_neural_data_from_design(file, &drone->sensor_neural);
        int sensor_n_size = 4;
        int sensor_n_shape[] = {200, 100, 50, 20};
        int sensor_n_activations[] = {
            ACTIVATION_SIGMOID,
            ACTIVATION_SIGMOID,
            ACTIVATION_SIGMOID
        };
        drone->sensor_neural = init_matrices_from_network_design(sensor_n_size, sensor_n_shape, sensor_n_activations);
        write_neural_data_to_file(file, drone->sensor_neural);
    }
    


    // Rotation.NN
    sprintf(file, "%s/Rotation.NN\0", enclosing_dir);
    if (access(file, F_OK) == 0) {
        // File exists
        init_neural_data_from_file(file, &drone->rotation_neural);
    } else {
        // File does not exist
        init_neural_data_from_design(file, &drone->rotation_neural);
        int rotation_n_size = 4;
        int rotation_n_shape[] = {40, 40, 30, 20};
        int rotation_n_activations[] = {
            ACTIVATION_SIGMOID,
            ACTIVATION_SIGMOID,
            ACTIVATION_SIGMOID
        };
        drone->rotation_neural = init_matrices_from_network_design(rotation_n_size, rotation_n_shape, rotation_n_activations);
        write_neural_data_to_file(file, drone->rotation_neural);
    }




    // Distance.NN
    sprintf(file, "%s/Distance.NN\0", enclosing_dir);
    if (access(file, F_OK) == 0) {
        // File exists
        init_neural_data_from_file(file, &drone->distance_neural);
    } else {
        // File does not exist
        init_neural_data_from_design(file, &drone->distance_neural);
        int distance_n_size = 3;
        int distance_n_shape[] = {30, 30, 20};
        int distance_n_activations[] = {
            ACTIVATION_SIGMOID,
            ACTIVATION_SIGMOID
        };
        drone->distance_neural = init_matrices_from_network_design(distance_n_size, distance_n_shape, distance_n_activations);
        write_neural_data_to_file(file, drone->distance_neural);
    }
    
    
    
    
    // Combine.NN
    sprintf(file, "%s/Combine.NN\0", enclosing_dir);
    if (access(file, F_OK) == 0) {
        // File exists
        init_neural_data_from_file(file, &drone->combine_neural);
    } else {
        // File does not exist
        init_neural_data_from_design(file, &drone->combine_neural);
        int combine_n_size = 5;
        int combine_n_shape[] = {60, 40, 30, 20, 4};
        int combine_n_activations[] = {
            ACTIVATION_SIGMOID,
            ACTIVATION_SIGMOID,
            ACTIVATION_SIGMOID,
            ACTIVATION_SIGMOID
        };
        drone->combine_neural = init_matrices_from_network_design(combine_n_size, combine_n_shape, combine_n_activations);
        write_neural_data_to_file(file, drone->combine_neural);
    }



    free(file);
}

void init_neural_data(const char* file, struct neural_data** nd) {
    if (access(file, F_OK) == 0) {
        // File exists
        init_neural_data_from_file(file, nd);
    } else {
        // File does not exist
        init_neural_data_from_design(file, nd);
    }
}

void init_neural_data_from_file(const char* file, struct neural_data** nd) {
    FILE* f = fopen(file, "rb");

    if (!f) {
        fprintf(stderr, "Failed to open NN for reading at: %s\n", file);
    } else {
        int size_rowsA, size_colsB, size_common;
        struct neural_data* network = malloc(sizeof(struct neural_data));
        *nd = network;

        // Read neural size
        int32_t neural_size;
        fread(&neural_size, sizeof(int32_t), 1, f);

        // Read neural shape
        int neural_shape[neural_size];
        fread(neural_shape, sizeof(int32_t), neural_size, f);

        // Read neural activations
        fread(&network->activations, sizeof(int32_t), neural_size - 1, f);

        network->weights_matrix_count = neural_size - 1;

        network->input_layer = calloc(neural_shape[0], sizeof(double));

        // Set num rows and columns
        for(int i = 0; i < network->weights_matrix_count; i++) {
            network->weights_row_count[i] = neural_shape[i + 1];
            network->weights_col_count[i] = neural_shape[i];
        }

        // Set weights and biases
        for(int i = 0; i < network->weights_matrix_count; i++) {
            network->weights_layers[i] = malloc(sizeof(double) * network->weights_row_count[i] * network->weights_col_count[i]);
            fread(network->weights_layers[i], sizeof(double), network->weights_row_count[i] * network->weights_col_count[i], f);

            network->biases_layers[i] = malloc(sizeof(double) * network->weights_row_count[i]);
            network->biases_layers_original[i] = malloc(sizeof(double) * network->weights_row_count[i]);
            fread(network->biases_layers[i], sizeof(double), network->weights_row_count[i], f);

            // copy bias mat
            mat_copy(
                network->biases_layers[i], // Copy from
                network->biases_layers_original[i], // Copy to
                network->weights_row_count[i], // Row count
                1); // Col count
        }

        network->output_layer = network->biases_layers[network->weights_matrix_count-1];

        fclose(f);
    }
}

void init_neural_data_from_design(const char* file, struct neural_data** nd) {
    int neural_size = 6;
    int neural_shape[] = {300, 250, 200, 100, 20, 4};
    int neural_activations[] = {
        ACTIVATION_RELU,
        ACTIVATION_RELU,
        ACTIVATION_RELU,
        ACTIVATION_RELU,
        ACTIVATION_SIGMOID
    };
    *nd = init_matrices_from_network_design(neural_size, neural_shape, neural_activations);
    write_neural_data_to_file(file, *nd);
}

// ############################################################################
// #######      INITIALIZATION      ###########################################
// ############################################################################



// ############################################################################
// #######      OPERATIONS      ###############################################
// ############################################################################

struct neural_data* init_matrices_from_network_design(int layer_count, int neural_design[], int activations[]) {
    int weights_count = layer_count - 1;

    int size_rowsA, size_colsB, size_common;
    struct neural_data* network = malloc(sizeof(struct neural_data));

    network->weights_matrix_count = weights_count;

    network->input_layer = calloc(neural_design[0], sizeof(double));
    
    for (int i = 0; i < weights_count; i++) {
        network->activations[i] = activations[i];
    }

    // Allocate memory & assign references to network struct
    for(int matrix_i = 0; matrix_i < weights_count; matrix_i++) {
        size_rowsA = neural_design[matrix_i + 1]; // Vector size we are going to
        // size_colsB = 1; // We want result to be a column vector
        size_common = neural_design[matrix_i]; // Vector size we are going from

        double* weights = calloc(size_rowsA * size_common, sizeof(double));
        double* biases = calloc(size_rowsA * 1, sizeof(double));
        double* biases_copy = calloc(size_rowsA * 1, sizeof(double));

        network->weights_layers[matrix_i] = weights;
        network->biases_layers[matrix_i] = biases;
        network->biases_layers_original[matrix_i] = biases_copy;

        network->weights_row_count[matrix_i] = size_rowsA;
        network->weights_col_count[matrix_i] = size_common;
    }

    for (int i = 0; i < weights_count; i++) {
        init_mat_random(
            network->weights_layers[i], // Weight matrix
            network->weights_row_count[i], // Row count
            network->weights_col_count[i], // Col count
            WEIGHT_INIT_MIN_VAL, // Random min
            WEIGHT_INIT_MAX_VAL); // Random max

        init_mat_random(
            network->biases_layers[i], // Bias vector
            network->weights_row_count[i], // Row count
            1, // Col count
            BIAS_INIT_MIN_VAL, // Random min
            BIAS_INIT_MAX_VAL); // Random max

        // copy bias mat
        mat_copy(
            network->biases_layers[i], // Copy from
            network->biases_layers_original[i], // Copy to
            network->weights_row_count[i], // Row count
            1); // Col count
    }

    network->output_layer = network->biases_layers[network->weights_matrix_count-1];

    return network;
}

void feed_forward_full_network(struct drone_data* drone) {
    feed_forward_network(drone->sensor_neural);
    feed_forward_network(drone->rotation_neural);
    feed_forward_network(drone->distance_neural);

    int c_i = 0;
    for (int i = 0; i < drone->sensor_neural->weights_row_count[drone->sensor_neural->weights_matrix_count-1]; i++) {
        drone->combine_neural->input_layer[c_i] = drone->sensor_neural->output_layer[i];
        c_i++;
    }
    for (int i = 0; i < drone->rotation_neural->weights_row_count[drone->rotation_neural->weights_matrix_count-1]; i++) {
        drone->combine_neural->input_layer[c_i] = drone->rotation_neural->output_layer[i];
        c_i++;
    }
    for (int i = 0; i < drone->distance_neural->weights_row_count[drone->distance_neural->weights_matrix_count-1]; i++) {
        drone->combine_neural->input_layer[c_i] = drone->distance_neural->output_layer[i];
        c_i++;
    }

    feed_forward_network(drone->combine_neural);
}

void feed_forward_network(struct neural_data* network) {
    double* W;
    double* V;
    double* B;

    // Copy original bias becase the result of each matrix multiplication is stored in the array of bias matrices
    for (int i = 0; i < network->weights_matrix_count; i++) {
        mat_copy(
            network->biases_layers_original[i],
            network->biases_layers[i],
            network->weights_row_count[i], // Row count
            1); // Col count
    }

    V = network->input_layer;
    for (int i = 0; i < network->weights_matrix_count; i++) {
        W = network->weights_layers[i];
        B = network->biases_layers[i];

        // Feed forward -> Result is stored in B
        mat_dgemm(W, V, B, // A B C
            network->weights_row_count[i],  // Row count
            1,                              // Col count
            network->weights_col_count[i]); // Common count

        for (int j = 0; j < network->weights_row_count[i]; j++) {
            *(B + j) = apply_activation(*(B + j), network->activations[i]);
        }

        V = B;
    }
    
    network->output_layer = network->biases_layers[network->weights_matrix_count - 1];
}

double apply_activation(double val, int activation) {
    switch(activation) {
        case ACTIVATION_RELU:
            return (val > 0) ? val : 0;
        case ACTIVATION_LEAKY_RELU:
            return (val > 0) ? val : val * 0.1;
        case ACTIVATION_SIGMOID:
            return 1.0 / (1.0 + exp2(-val));
    }
    return -1;
}

void write_neural_data_to_file(const char* file, struct neural_data* network) {
    FILE* f = fopen(file, "wb");

    if (!f) {
        fprintf(stderr, "Failed to open NN for writing at: %s\n", file);
    } else {
        // Write size
        int32_t neural_size = network->weights_matrix_count + 1;
        fwrite(&neural_size, sizeof(int32_t), 1, f);

        // Write shape
        for (int i = 0; i < network->weights_matrix_count; i++) {
            fwrite(&network->weights_col_count[i], sizeof(int32_t), 1, f);
        }
        fwrite(&network->weights_row_count[network->weights_matrix_count - 1], sizeof(int32_t), 1, f);

        // Write activations
        for (int i = 0; i < network->weights_matrix_count; i++) {
            fwrite(&network->activations[i], sizeof(int32_t), 1, f);
        }

        // Write weights and biases
        for (int i = 0; i < network->weights_matrix_count; i++) {
            fwrite(network->weights_layers[i], sizeof(double), network->weights_row_count[i] * network->weights_col_count[i], f);
            fwrite(network->biases_layers_original[i], sizeof(double), network->weights_row_count[i], f);
        }

        fclose(f);
    }
}

// ############################################################################
// #######      OPERATIONS      ###############################################
// ############################################################################




// ############################################################################
// #######      UTILITIES      ################################################
// ############################################################################

int network_input_layer_size(struct neural_data* network) {
    return network->weights_col_count[0];
}

// ############################################################################
// #######      UTILITIES      ################################################
// ############################################################################