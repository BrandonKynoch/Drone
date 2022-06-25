#include <stdlib.h>
#include <stdbool.h>
#include "custom_BLAS.h"

#define MAX_NEURAL_LAYERS 10

// Note: this network data is never freed anywhere
struct network_data {
    int weights_matrix_count;

    // At each layer index:
    //  Weights matrix row count = weights_row_count
    //  Weights matrix column count = weights_col_count
    //  Bias row count = weights_col_count
    //  Bias col count = 1

    double* weights_layers[MAX_NEURAL_LAYERS];
    double* biases_layers[MAX_NEURAL_LAYERS];
    double* biases_layers_original[MAX_NEURAL_LAYERS];
    
    int weights_row_count[MAX_NEURAL_LAYERS];
    int weights_col_count[MAX_NEURAL_LAYERS];

    double* input_layer;
    double* output_layer; // This is just a convenience pointer - it points to last element of bias array
};

// Initialize a network_data structure and allocate memory for the given network design
// IMPORTANT: layer_count must equal the number of elements in 'neural_design' parameter
struct network_data* init_matrices_from_network_design(int layer_count, int neural_design[]);

// Perform one feed forward on the network
// Output is stored in network_data->output_layer
void feed_forward_network(struct network_data* network);