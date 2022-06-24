#include <stdlib.h>
#include "custom_BLAS.h"

#define MAX_NEURAL_LAYERS 10

struct network_data {
    int weights_matrix_count;
    double* weights_layers[MAX_NEURAL_LAYERS];
    double* biases_layers[MAX_NEURAL_LAYERS];
    
    int weights_row_count[MAX_NEURAL_LAYERS];
    int weights_col_count[MAX_NEURAL_LAYERS];
};

struct network_data* init_matrices_from_network_design(int layer_count, int neural_design[]);