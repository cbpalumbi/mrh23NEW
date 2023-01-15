using UnityEngine;

namespace Recognissimo.Utils
{
    public static class Json
    {
        public static T Deserialize<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }

        public static string Serialize<T>(T item)
        {
            return JsonUtility.ToJson(item);
        }
    }
}