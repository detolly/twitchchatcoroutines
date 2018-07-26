using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System;

namespace TwitchChatCoroutines.ClassesAndStructs
{
    class MessageControl : Panel
    {
        public List<Image> badges;
        public string username;
        public SortedList<int, ImageAndInts> emotes;
        public TwitchMessage twitchMessage;

        public Font Font { get; set; }
        public Color ForeColor { get; set; }
        public Color BackColor { get; set; }

        public int DesiredWidth { get; set; }
        public bool DoSplitter { get; set; }
        public bool IsAction { get; set; }
        public int PanelBorder { get; set; }

        public Image splitter;

        public ColorConverter cc;

        public void Init()
        {
            cc = new ColorConverter();
        }

        public MessageControl(TwitchMessage message, List<Image> badges, SortedList<int, ImageAndInts> emotes)
        {
            Init();
            this.badges = badges;
            this.emotes = emotes;
            twitchMessage = message;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            int tStart = 0;
            int border = 5;
            bool exists = false;

            foreach (var badge in badges)
            {
                exists = true;
                e.Graphics.DrawImage(badge, new Point(tStart + border, PanelBorder));
                tStart += badge.Size.Width + border;
            }

            var UserNameColor = (Color)cc.ConvertFromString(twitchMessage.color == "" ? ChatForm.getRandomColor() : twitchMessage.color);
            Brush foreColorBrush = new SolidBrush(ForeColor);
            Brush usernameBrush = new SolidBrush(UserNameColor);

            string usernameText = twitchMessage.display_name + (twitchMessage.username != twitchMessage.display_name.ToLower() ? " (" + twitchMessage.username + ")" : "");
            Size s = TextRenderer.MeasureText(usernameText, Font);
            var location = new Point(tStart + border, (exists ? badges[0].Size.Height / 2 - s.Height / 2 : PanelBorder));
            e.Graphics.DrawString(usernameText, Font, usernameBrush, location);

            string text = twitchMessage.message;

            bool first = true;
            int textOffset = 0;

            for (int i = 0; i < emotes.Count+1; i++)
            {
                string currentText = "";
                if (first)
                {
                    first = false;
                    currentText += ": ";
                }
                int start = textOffset;

                if (i != emotes.Count)
                {
                    var thing = emotes.Values[i];
                    var theTuple = thing.ints;
                }
            }
        }

        
    }
}
