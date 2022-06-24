#include <d_neural.h>

struct network_data* init_matrices_from_network_design(int layer_count, int neural_design[]) {
    int weights_count = layer_count - 1;

    int size_rowsA, size_colsB, size_common;
    struct network_data* network = malloc(sizeof(struct network_data));

    for(int matrix_i = 0; matrix_i < weights_count; matrix_i++) {
        size_rowsA = neural_design[matrix_i + 1]; // Vector size we are going to
        // size_colsB = 1; // We want result to be a column vector
        size_common = neural_design[matrix_i]; // Vector size we are going from

        double* weights = calloc(size_rowsA * size_common, sizeof(double));
        double* biases = calloc(size_rowsA * 1, sizeof(double));

        network->weights_layers[matrix_i] = weights;
        network->biases_layers[matrix_i] = biases;

        network->weights_row_count[matrix_i] = size_rowsA;
        network->weights_col_count[matrix_i] = size_common;
    }

    network->weights_matrix_count = weights_count;

    return network;




    // Load matrices from file
    // First 4 bytes of file should indicate hidden layer count (l)
    // The next (l * 4) bytes tell us the size of each layer
    // Read through data and initialize arrays
    //  double* H1 = calloc(rowsA * common, sizeof(double)); // A = 2x3
    // Feed forward using
    // WV + B   : where V is vector in, and num rows in W determines size of output vector
    // TODO: Make function to copy C before calling dgemm to keep original, then set back after
}