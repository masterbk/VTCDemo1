using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Demo1.Helper
{
    public static class JsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static T ParseTo<T>(this string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return default(T);
            }

            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        public static string ToJson<T>(this T obj)
        {
            return JsonSerializer.Serialize(obj, _jsonOptions);
        }
    }
}
