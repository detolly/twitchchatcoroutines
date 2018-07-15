using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

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
                string name = fileArray[0].Substring(fileArray[0].IndexOf("\\") + 1);
                string type = fileArray[1];
                if (type == "png")
                {
                    try
                    {
                        var s = char.ConvertFromUtf32(int.Parse(name, NumberStyles.AllowHexSpecifier));
                        codeToEmoji.Add(s, Image.FromFile(file));
                    }
                    catch { }
                }
            }
        }
    }
}
