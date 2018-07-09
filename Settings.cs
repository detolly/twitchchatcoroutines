using Newtonsoft.Json;
using System.IO;

namespace TwitchChatCoroutines
{
    public static class Settings
    {
        private static dynamic json;

        private static string path = "settings.json";

        private static void SaveJson()
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(json));
        }

        private static void LoadJson()
        {
            json = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(path));
        }
    }
}
