using Newtonsoft.Json;
using System.Net;

namespace TwitchChatCoroutines.ClassesAndStructs
{
    public static class HelperFunctions
    {
        public static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        public static dynamic JsonGet(string url, string[] headers = null)
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

        public static string CustomSubstring(this string theString, int firstIndex, int length)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            for (int i = firstIndex; i < length; i++)
            {
                builder.Append(theString[i]);
                //emoji stuff
                //if (theString[i])
                //{

                //}
            }
            return builder.ToString();
        }
    }
}
