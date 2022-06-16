using System;
using Newtonsoft.Json;

namespace SimServer {
    public class JSONFactory {

        public static void Add(JsonWriter json, string key, string value) {
            json.WritePropertyName(key);
            json.WriteValue(value);
        }
    }
}

