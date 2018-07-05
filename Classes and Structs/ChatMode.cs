using System;
using System.Collections.Generic;

namespace TwitchChatCoroutines.ClassesAndStructs
{
    public class ChatMode
    {
        public static string[] available = new string[]
        {
            "Anonymous",
            "Chat User",
            "Color Key Mode"
        };
        public int currentIndex = 0;
    }
}