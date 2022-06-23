#include <stdio.h>
#include <time.h>
#include <stdlib.h>
#include "cblas.h"
#include "cblas_f77.h"

// ############################################################################
// #######      INITIALIZERS      #############################################
// ############################################################################

/// Initialize a matrix where each successive element is one greater than the previous element
void init_mat_counting(double* matrix, int row, int column, int start);

// Initialize an identity matrix
void init_mat_identity(double* matrix, int size);

// Initialize matrix with each element set to some constant value
void init_mat_const(double* matrix, int row, int column, double value);

void init_mat_random(double* matrix, int rows, int columns, double min_val, double max_val);

// ############################################################################
// #######      INITIALIZERS      #############################################
// ############################################################################





// ############################################################################
// #######      OPERATIONS      ###############################################
// ############################################################################

// Convenient dgemm
// Using row major, alpha = 1, beta = 1
// D <- AB + C
// A & B are unchanged after calling this function
// The result is stored in C
void mat_dgemm(double* A, double* B, double* C, int rowsA, int colsB, int common);

void mat_set(double* A, int rowSize, int row, int col, double val);

// ############################################################################
// #######      OPERATIONS      ###############################################
// ############################################################################





// ############################################################################
// #######      UTILITIES      ################################################
// ############################################################################

// Debug print matrix
void print(const char* name, const double* matrix, int row_size, int column_size);

// ############################################################################
// #######      UTILITIES      ################################################
// ############################################################################