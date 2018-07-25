using System.Collections.Generic;
using System.Windows.Forms;

namespace TwitchChatCoroutines.ClassesAndStructs
{
    class MessageControl
    {
        public SortedList<int, PictureBoxAndInts> emotes;
        public List<PictureBox> badges;
        public List<TwitchLabel> messages;
        public List<Controls.ToolTip> tooltips;
        public Panel panel;
        public TwitchLabel username;

        public bool isAction;

        public TwitchMessage twitchMessage;
        public PictureBox splitter;
    }
}
