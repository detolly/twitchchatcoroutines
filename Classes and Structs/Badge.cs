using System;
using System.Collections.Generic;
using System.Drawing;

namespace TwitchChatCoroutines.ClassesAndStructs
{
    class Badge
    {
        public Dictionary<string, BadgeVersion> versions { get; set; }
    }

    class BadgeVersion
    {
        public Image image { get; set; }

        public string url_1x { get; set; }
        public string url_2x { get; set; }
        public string url_4x { get; set; }

        public string description { get; set; }
        public string title { get; set; }
    }
}
