#include <json-c/json.h>
#include <string.h>
#include <stdlib.h>
#include <stdio.h>

int main() {
    // Create simple JSON
    // struct json_object* json = json_object_new_object();
    // json_object_object_add(json, "port", json_object_new_int(5821));
    // json_object_object_add(json, "New Key", json_object_new_string("Brandon"));
    // printf("json:\n%s", json_object_to_json_string_ext(json, JSON_C_TO_STRING_SPACED | JSON_C_TO_STRING_PRETTY));

    struct json_object* json = json_object_new_object();
    json_object_object_add(json, "opcode", json_object_new_int(0x4));
    char* json_string = json_object_to_json_string_ext(json, JSON_C_TO_STRING_PLAIN);
    printf(json_string);

    printf("\n\n\n");


    struct json_object* json_in = json_tokener_parse(json_string);
    struct json_object* json_id;
    json_object_object_get_ex(json_in, "opcode", &json_id);
    int id = json_object_get_int(json_id);

    // printf("json_in:\n%s\n\n", json_object_to_json_string_ext(json_in, JSON_C_TO_STRING_SPACED | JSON_C_TO_STRING_PRETTY));
    // printf("json_id:\n%s\n\n", json_object_to_json_string_ext(json_id, JSON_C_TO_STRING_SPACED | JSON_C_TO_STRING_PRETTY));

    printf("Opcode: %d\n", id);

    printf("\n\n");


    json_object_put(json);
    json_object_put(json_in);
    json_object_put(json_id);
}