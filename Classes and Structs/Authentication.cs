using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace TwitchChatCoroutines.ClassesAndStructs
{
    public class Auth
    {
        public string username;
        public string oauth;
    }

    public static class Authentication
    {

        public static void Remove()
        {

        }

        public static void Add(Auth a)
        {
            dynamic json = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText("settings.json"));
            json.authentication[a.username] = a.oauth;
            string toString = JsonConvert.SerializeObject(json);
            File.WriteAllText("settings.json", toString);
        }

        public static string[] GetLogins()
        {
            List<string> returns = new List<string>();
            dynamic json = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText("settings.json"));
            Dictionary<string, string> a = json.authentication.ToObject<Dictionary<string, string>>();
            foreach (var s in a)
            {
                returns.Add(s.Key);
            }
            return returns.ToArray();
        }

        public static string GetOauth(string key)
        {
            List<string> returns = new List<string>();
            dynamic json = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText("settings.json"));
            return json.authentication[key];
        }
    }
}
