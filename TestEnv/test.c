#include <stdio.h>
#include <time.h>
#include <stdlib.h>

int main() {
  
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