using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchChatCoroutines.ClassesAndStructs
{
    public class TwitchSettings
    {
        public bool twitchEmoteCaching;
        public bool bttvEmoteCaching;
        public bool ffzEmoteCaching;
        public bool emotesCaching;

		public static TwitchSettings Interpret(dynamic json)
        {
            TwitchSettings settings = new TwitchSettings();
            settings.twitchEmoteCaching = json.general.twitchEmoteCaching;
            settings.bttvEmoteCaching = json.general.bttvEmoteCaching;
            settings.ffzEmoteCaching = json.general.ffzEmoteCaching;
            settings.emotesCaching = json.general.emotesCaching;
            return settings;
        }
    }
}
