using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace TwitchChatCoroutines
{
    static class Emojis
    {
        public static Dictionary<string, Image> codeToEmoji;

        public static void Init()
        {
            codeToEmoji = new Dictionary<string, Image>();
            foreach (var file in Directory.GetFiles("Emojis"))
            {
                string[] fileArray = file.Split('.');
                string name = fileArray[0].Substring(fileArray[0].IndexOf("\\")+1);
                string type = fileArray[1];
                if (type == "png")
                {
                    //codeToEmoji.Add(string.Concat("\\", "U000", name), Image.FromFile(file));
                }
            }
        }
    }
}
