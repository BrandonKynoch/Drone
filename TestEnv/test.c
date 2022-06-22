#include <stdio.h>
#include <time.h>
#include <stdlib.h>
#include "cblas.h"
#include "cblas_f77.h"

#define INVALID -1

void init(double* matrix, int row, int column)
{
  for (int j = 0; j < column; j++){
    for (int i = 0; i < row; i++){
      matrix[j*row + i] = ((double)rand())/RAND_MAX;
    }
  }
}

void print(const char * name, const double* matrix, int row, int column)
{
  printf("Matrix %s has %d rows and %d columns:\n", name, row, column);
  for (int i = 0; i < row; i++){
    for (int j = 0; j < column; j++){
      printf("%.3f\t", matrix[j*row + i]);
    }
    printf("\n");
  }
  printf("\n");
}

int main(int argc, char * argv[])
{
  int rowsA, colsB, common;
  int i,j,k;

  if (argc != 4){
    printf("Using defaults\n");
    rowsA = 2; colsB = 4; common = 6;
  }
  else{
    rowsA = atoi(argv[1]); colsB = atoi(argv[2]);common = atoi(argv[3]);
  }

  double A[rowsA * common]; double B[common * colsB];
  double C[rowsA * colsB]; double D[rowsA * colsB];

  char transA = 'N', transB = 'N';
  double one = 1.0, zero = 0.0;

  srand(time(NULL));

  init(A, rowsA, common); init(B, common, colsB);

  dgemm_(&transA, &transB, &rowsA, &colsB, &common, &one, A,
         &rowsA, B, &common, &zero, C, &rowsA);

  for(i=0;i<colsB;i++){
    for(j=0;j<rowsA;j++){
      D[i*rowsA+j]=0;
      for(k=0;k<common;k++){
        D[i*rowsA+j]+=A[k*rowsA+j]*B[k+common*i];
      }
    }
  }

  print("A", A, rowsA, common); print("B", B, common, colsB);
  print("C", C, rowsA, colsB); print("D", D, rowsA, colsB);

  return 0;
}


void json_test() {
/*
// Create simple JSON
    // struct json_object* json = json_object_new_object();
    // json_object_object_add(json, "port", json_object_new_int(5821));
    // json_object_object_add(json, "New Key", json_object_new_string("Brandon"));
    // printf("json:\n%s", json_object_to_json_string_ext(json, JSON_C_TO_STRING_SPACED | JSON_C_TO_STRING_PRETTY));

    // struct json_object* json = json_object_new_object();
    // json_object_object_add(json, "opcode", json_object_new_int(0x4));
    // char* json_string = json_object_to_json_string_ext(json, JSON_C_TO_STRING_PLAIN);
    // printf(json_string);

    // printf("\n\n\n");


    // struct json_object* json_in = json_tokener_parse(json_string);
    // struct json_object* json_id;
    // json_object_object_get_ex(json_in, "opcode", &json_id);
    // int id = json_object_get_int(json_id);

    // // printf("json_in:\n%s\n\n", json_object_to_json_string_ext(json_in, JSON_C_TO_STRING_SPACED | JSON_C_TO_STRING_PRETTY));
    // // printf("json_id:\n%s\n\n", json_object_to_json_string_ext(json_id, JSON_C_TO_STRING_SPACED | JSON_C_TO_STRING_PRETTY));

    // printf("Opcode: %d\n", id);

    // printf("\n\n");


    // json_object_put(json);
    // json_object_put(json_in);
    // json_object_put(json_id);







    struct json_object* m_json = json_object_new_object();
    json_object_object_add(m_json, "opcode", json_object_new_int(0x1));
    json_object_object_add(m_json, "id", json_object_new_uint64(5));

    json_object_object_add(m_json, "motor_fl", json_object_new_double(0));
    json_object_object_add(m_json, "motor_fr", json_object_new_double(0));
    json_object_object_add(m_json, "motor_br", json_object_new_double(0));
    json_object_object_add(m_json, "motor_bl", json_object_new_double(0));

    char* json_string = json_object_to_json_string_ext(m_json, JSON_C_TO_STRING_PLAIN);
    printf(json_string);
    printf("\n\n");
    fflush(stdout);


    struct json_object* json_fl;
    struct json_object* json_fr;
    struct json_object* json_br;
    struct json_object* json_bl;

    printf("\n\nTry get values\n\n");

    json_object_object_get_ex(m_json, "motor_fl", &json_fl);
    json_object_object_get_ex(m_json, "motor_fr", &json_fr);
    json_object_object_get_ex(m_json, "motor_br", &json_br);
    json_object_object_get_ex(m_json, "motor_bl", &json_bl);

    json_object_set_double(json_fl, 1);
    json_object_set_double(json_fr, 2);
    json_object_set_double(json_br, 3);
    json_object_set_double(json_bl, 4);

    // json_object_put(json_fl);
    // json_object_put(json_fr);
    // json_object_put(json_br);
    // json_object_put(json_bl);

    // Can't fetch values after setting
    char* json_string2 = json_object_to_json_string_ext(m_json, JSON_C_TO_STRING_PLAIN);
    printf(json_string2);
    printf("\n\n");
    fflush(stdout);
*/
}