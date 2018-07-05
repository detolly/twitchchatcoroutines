using System;
using System.Collections.Generic;

namespace TwitchChatCoroutines.ClassesAndStructs
{
    public class ChatMode
    {
        public int currentIndex = 0;
    }
    public enum ChatModes
    {
        Anonymous = 0,
        ChatUser = 1
    }
}