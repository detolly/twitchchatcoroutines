using System.Collections.Generic;
using System.Windows.Forms;

namespace TwitchChatCoroutines.ClassesAndStructs
{
    class MessageControl
    {
        public SortedList<int, PictureBoxAndInts> emotes;
        public List<Label> messages;
        public Panel panel;
        public TwitchLabel username;

        public bool isAction;

        public TwitchMessage twitchMessage;
        public string oneMessage;
        public PictureBox splitter;
    }
}
