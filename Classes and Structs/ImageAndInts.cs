using System;
using System.Drawing;
using TwitchChatCoroutines.Controls;

namespace TwitchChatCoroutines.ClassesAndStructs
{
    struct ImageAndInts
    {
        public Image img;
        public Size preferredSize;
        public ToolTip tooltip;
        public Tuple<int, int> ints;
    }
}
