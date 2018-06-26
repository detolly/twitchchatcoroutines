using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TwitchChatCoroutines
{
    internal struct TwitchMessage
    {
        //@badges=premium/1;color=;display-name=wangstar1;emotes=;id=b0167ef4-58a8-4591-b094-cd6d9da90c55;mod=0;room-id=192805101;subscriber=0;tmi-sent-ts=1529454707822;turbo=0;user-id=184083783;user-type= :wangstar1!wangstar1@wangstar1.tmi.twitch.tv PRIVMSG #nhl :message here
        public string badges;
        public string color;
        public string display_name;
        public string emotes;
        public string id;
        public byte mod;
        public long room_id;
        public byte subscriber;
        public long tmi_sent_ts;
        public byte turbo;
        public long user_id;
        //public string user_type;
        public string message;
    }

    internal class MessageControl
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

    internal struct PictureBoxAndInts
    {
        public PictureBox pb;
        public Tuple<int, int> ints;
    }

    internal class TwitchLabel : Label
    {
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            AutoSize = false;
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            FlatStyle = FlatStyle.System;
            Size = GetTextSize();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Size = GetTextSize();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Size = GetTextSize();
        }

        public Size GetTextSize()
        {
            Size padSize = TextRenderer.MeasureText(".", Font);
            Size textSize = TextRenderer.MeasureText(Text + ".", Font);
            return new Size(textSize.Width - padSize.Width, textSize.Height);
        }
    }
}