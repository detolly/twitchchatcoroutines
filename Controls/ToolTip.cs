using System;
using System.Drawing;
using System.Windows.Forms;

namespace TwitchChatCoroutines.Controls
{
    public class ToolTip : Control
    {
        private static readonly int border = 0;
        private string text;

        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
            }
        }

        public Image Image { get; set; }
        public Control Parent { get; set; }

        public ToolTip() { }

        public ToolTip(Control parent) {
            Parent = parent;
        }

        private void UpdateSizeAndLocation()
        {
            //todo alot
        }
    }
}
