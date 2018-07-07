using Newtonsoft.Json;
using System.Net;

namespace TwitchChatCoroutines.ClassesAndStructs
{
    public static class HelperFunctions
    {
        public static dynamic jsonGet(string url, string[] headers = null)
        {
            using (WebClient client = new WebClient())
            {
                if (headers != null)
                    for(int i = 0; i < headers.Length; i++)
                    {
                        client.Headers.Add(headers[i]);
                    }
                return JsonConvert.DeserializeObject<dynamic>(client.DownloadString(url));
            }
        }
    }
}
