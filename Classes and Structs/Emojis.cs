using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Resources;
using TwitchChatCoroutines.Properties;

namespace TwitchChatCoroutines
{
    static class Emojis
    {
        public static Dictionary<string, Image> codeToEmoji;

        static Emojis()
        {
            codeToEmoji = new Dictionary<string, Image>();
            ResourceManager MyResourceClass = new ResourceManager(typeof(Resources /* Reference to your resources class -- may be named differently in your case */));

            ResourceSet resourceSet = MyResourceClass.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            foreach (DictionaryEntry entry in resourceSet)
            {
                string resourceKey = entry.Key.ToString().Replace("_", "");
                if (resourceKey.Contains("splitter"))
                    continue;
                Image resource = (Image)entry.Value;
                try
                {
                    var s = char.ConvertFromUtf32(int.Parse(resourceKey, NumberStyles.AllowHexSpecifier));
                    codeToEmoji.Add(s, resource);
                }
                catch { }
            }
        }
    }
}
