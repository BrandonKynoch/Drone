// Author: Brandon Kynoch
// 23 June 2022

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

// This method should not be used for initializing the entire matrix, rather use
// one of the matrix initializers
// Set the value of a sigle element in a matrix
void mat_set(double* A, int rowSize, int row, int col, double val);

// Copy the values in one matrix to another
// Both matrices must share the same dimensions
void mat_copy(double* from, double* to, int row, int col);

// ############################################################################
// #######      OPERATIONS      ###############################################
// ############################################################################





// ############################################################################
// #######      UTILITIES      ################################################
// ############################################################################

// Debug print matrix
void print_matrix(const char* name, const double* matrix, int row_size, int column_size);

// ############################################################################
// #######      UTILITIES      ################################################
// ############################################################################