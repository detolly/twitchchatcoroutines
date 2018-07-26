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
        public SortedList<int, ImageAndInts> emotes;
        public TwitchMessage twitchMessage;

        public Font Font { get; set; }
        public Color ForeColor { get; set; }
        public Color BackColor { get; set; }

        public int DesiredWidth { get; set; }
        public int EmoteSpacing { get; set; }
        public bool DoSplitter { get; set; }
        public bool IsAction { get; set; }
        public int PanelBorder { get; set; }

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
            //MouseClick += (o, e) => Visible = !Visible;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            int border = 5;
            int tStart = border;
            int highest = 0;
            int yoffset = 0;
            bool exists = false;
            e.Graphics.DrawImage(TwitchChatCoroutines.Properties.Resources.splitter2, 0, 0, DesiredWidth, 1);
            yoffset += 1;

            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            foreach (var badge in badges)
            {
                exists = true;
                e.Graphics.DrawImage(badge, new Point(tStart, PanelBorder));
                tStart += badge.Size.Width + border;
                if (badge.Height > highest)
                {
                    highest = badge.Height;
                }
            }

            var UserNameColor = (Color)cc.ConvertFromString(twitchMessage.color == "" ? ChatForm.getRandomColor() : twitchMessage.color);
            Brush foreColorBrush = new SolidBrush(ForeColor);
            Brush usernameBrush = new SolidBrush(UserNameColor);

            string usernameText = twitchMessage.display_name + (twitchMessage.username != twitchMessage.display_name.ToLower() ? " (" + twitchMessage.username + ")" : "");
            Size s = TextRenderer.MeasureText(usernameText, Font);
            var location = new Point(tStart, PanelBorder + (exists ? badges[0].Size.Height / 2 - s.Height / 2 : 0));
            yoffset += location.Y;
            e.Graphics.DrawString(usernameText, Font, usernameBrush, location);

            string text = twitchMessage.message;
            Size theTextSize = TextRenderer.MeasureText(text, Font);

            int currentOffset = 0;
            int theWidth = DesiredWidth - 2 * border;

            int lastX = location.X + s.Width - border;
            //e.Graphics.FillRectangle(foreColorBrush, new Rectangle(new Point(lastX, location.Y), new Size(1,theTextSize.Height)));

            bool first = true;

            for (int i = 0; i < emotes.Count + 1; i++)
            {
                var thing = i != emotes.Count ? emotes.Values[i] : new ImageAndInts() /* To avoid errors on compile time */;
                var theTuple = thing.ints;
                int next = i != emotes.Count ? thing.ints.Item1 : text.Length;
                string offsetText = text.Substring(currentOffset, next - currentOffset);
                if (first)
                {
                    first = false;
                    offsetText = ": " + offsetText;
                }
                if (offsetText.Length > 0)
                    if (offsetText[0] == ' ')
                        offsetText = offsetText.Substring(1);
                if (offsetText.Length > 0)
                    if (offsetText[offsetText.Length - 1] == ' ')
                        offsetText = offsetText.Substring(0, offsetText.Length - 1);
                currentOffset = i != emotes.Count ? thing.ints.Item2 + 1 : 0 /* this 0 shouldn't matter beacuse we're exiting the loop */;

                string[] args = offsetText.Split(' ');

                string current = "";
                for (int x = 0; x < args.Length; x++)
                {
                    bool wasInside = false;
                    string old = current;
                    current += args[x];
                    Size currentTextWidth = TextRenderer.MeasureText(current, Font);
                    if (currentTextWidth.Width + lastX > theWidth)
                    {
                        wasInside = true;
                        Size currentArgsWidth = TextRenderer.MeasureText(args[x], Font);
                        if (currentArgsWidth.Width > theWidth)
                        {
                            //some things that I can't really remember goes here.
                        }
                        e.Graphics.DrawString(old, Font, foreColorBrush, new Point(lastX, yoffset));
                        lastX = border;
                        yoffset += theTextSize.Height + (28 / 2 - theTextSize.Height / 2);
                        current = "";
                        x--;
                    }
                    if (x == args.Length - 1)
                    {
                        if (currentTextWidth.Height + yoffset > highest)
                        {
                            highest = currentTextWidth.Height + yoffset;
                        }
                        if (current != "" && current != " ")
                        {
                            e.Graphics.DrawString(current, Font, foreColorBrush, new Point(lastX, yoffset));
                            lastX += TextRenderer.MeasureText(current, Font).Width;
                        }
                    }
                    else if (!wasInside)
                        current += " ";
                }
                if (i != emotes.Count)
                {
                    if (lastX + thing.img.Width + EmoteSpacing > theWidth)
                    {
                        lastX = border;
                        yoffset += theTextSize.Height + (28 / 2 - theTextSize.Height / 2);
                    }
                    e.Graphics.DrawImage(thing.img, lastX + EmoteSpacing, yoffset + theTextSize.Height / 2 - thing.img.Size.Height / 2, thing.img.Size.Width, thing.img.Size.Height);
                    if (yoffset + thing.img.Size.Height + theTextSize.Height / 2 - thing.img.Size.Height / 2 > highest)
                    {
                        highest = yoffset + thing.img.Size.Height + theTextSize.Height / 2 - thing.img.Size.Height / 2;
                    }
                    lastX += thing.img.Size.Width;
                    lastX += 2 * EmoteSpacing;
                }
                else
                    break;
            }
            Size = new Size(DesiredWidth, Math.Max(highest + 2 * PanelBorder, 28));
        }
    }
}
