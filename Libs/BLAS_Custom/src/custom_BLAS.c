// Author: Brandon Kynoch
// 23 June 2022

#include <custom_BLAS.h>

// int main() {
//   printf("CUSTOM BLAS WRAPPER TESTING SUITE:\n\n");
// }

// ############################################################################
// #######      INITIALIZERS      #############################################
// ############################################################################
#pragma region INITIALIZERS

void init_mat_counting(double* matrix, int row, int column, int start) {
  double startD = (double) start;
  for (int j = 0; j < column; j++) {
    for (int i = 0; i < row; i++) {
      matrix[j * row + i] = startD;
      startD += 1.0;
    }
  }
}

void init_mat_identity(double* matrix, int size) {
  for (int i = 0; i < size * size; i++) {
    matrix[i] = 0.0;
  }

  for (int i = 0; i < size; i++) {
    matrix[(i * size) + i] = 1;
  }
}

void init_mat_const(double* matrix, int row, int column, double value) {
  for (int j = 0; j < column; j++) {
    for (int i = 0; i < row; i++) {
      matrix[j * row + i] = value;
    }
  }
}

void init_mat_random(double* matrix, int rows, int columns, double min_val, double max_val) {
  double range = (max_val - min_val); 
  double div = RAND_MAX / range;

  for (int i = 0; i < rows; i++) {
    for (int j = 0; j < columns; j++) {
      matrix[(i * columns) + j] = min_val + (rand() / div);
    }
  }
}

#pragma endregion
// ############################################################################
// #######      INITIALIZERS      #############################################
// ############################################################################



// ############################################################################
// #######      OPERATIONS      ###############################################
// ############################################################################
#pragma region OPERATIONS

void mat_dgemm(double* A, double* B, double* C, int rowsA, int colsB, int common) {
  cblas_dgemm(
    CblasRowMajor, // Order 
    CblasNoTrans, // Transpose A
    CblasNoTrans, // Transpose B
    rowsA, // Number of rows in A & C
    colsB, // Number of cols in B & C
    common, // Number of cols in A and rows in B
    1, // Scaling factor for the product of AB
    A, // Matrix A
    common, // The size of the first dimension of A
    B, // Matrix B
    colsB, // The size of the first dimension on B
    1, // Scaling factor for matrix C
    C, // Matrix C
    colsB); // The size of the first dimension of C
}

void mat_set(double* A, int rowSize, int row, int col, double val) {
  A[(row * rowSize) + col] = val;
}

void mat_copy(double* from, double* to, int row, int col) {
  int size = row * col;
  for (int i = 0; i < size; i++) {
    to[i] = from[i];
  }
}

#pragma endregion
// ############################################################################
// #######      OPERATIONS      ###############################################
// ############################################################################



// ############################################################################
// #######      UTILITIES      ################################################
// ############################################################################
#pragma region UTILITIES

void print_matrix(const char* name, const double* matrix, int row_size, int column_size) {
  printf("Matrix %s has %d rows and %d columns:\n", name, row_size, column_size);
  for (int i = 0; i < row_size; i++) {
    for (int j = 0; j < column_size; j++) {
      printf("%.3f\t", matrix[(i * column_size) + j]);
    }
    printf("\n");
  }
  printf("\n");
}

#pragma endregion
// ############################################################################
// #######      UTILITIES      ################################################
// ############################################################################

























// ############################################################################
// #######      TESTING      ##################################################
// ############################################################################

/*

 int rowsA = 3;
  int colsB = 3;
  int common = 2;

  double* A = calloc(rowsA * common, sizeof(double)); // A = 2x3
  double* B = calloc(common * colsB, sizeof(double)); // B = 3x3
  double* C = calloc(rowsA * colsB, sizeof(double)); // Product = 2x3

  init_mat_random(A, rowsA, common, -1.0, 1.0);
  init_mat_random(B, common, colsB, -1.0, 1.0);
  init_mat_const(C, rowsA, colsB, 10);

  print("A", A, rowsA, common);
  print("B", B, common, colsB);
  print("C", C, rowsA, colsB);

  mat_dgemm(
    A, B, C,
    rowsA,
    colsB,
    common
  );

  printf("\n\nPost mult:\n\n");
  print("A", A, rowsA, common);
  print("B", B, common, colsB);
  print("C", C, rowsA, colsB);

  free(A);
  free(B);
  free(C);
*/