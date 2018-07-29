using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System;

using TwitchChatCoroutines.ClassesAndStructs;

namespace TwitchChatCoroutines.Controls
{
    class MessageControl : Panel
    {
        public List<Image> badges;
        public SortedList<int, ImageAndInts> emotes;
        public TwitchMessage twitchMessage;

        public Font Font { get; set; }

        public int DesiredWidth { get; set; }
        public int EmoteSpacing { get; set; }
        public bool DoSplitter { get; set; }
        public bool IsAction { get; set; }
        public int PanelBorder { get; set; }

        private ColorConverter cc;
        private bool initial = false;
        private bool ready = false;

        private List<ImageBox> badgeControlList = new List<ImageBox>();
        private List<ImageBox> emoteControlList = new List<ImageBox>();

        public void Init()
        {
            cc = new ColorConverter();
            foreach(var b in badges)
            {
                var currentBadge = new ImageBox(b);
                Controls.Add(currentBadge);
                badgeControlList.Add(currentBadge);
            }
            foreach(var e in emotes)
            {
                var theImage = new ImageBox(e.Value.img);
                Controls.Add(theImage);
                emoteControlList.Add(theImage);
            }
            ready = true;
        }

        public MessageControl(TwitchMessage message, List<Image> badges, SortedList<int, ImageAndInts> emotes)
        {
            this.badges = badges;
            this.emotes = emotes;
            twitchMessage = message;
            Init();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!ready) return;
            e.Graphics.Clear(BackColor);

            int border = 5;
            int tStart = border;
            int highest = 0;
            int yoffset = 0;
            bool exists = false;
            e.Graphics.DrawImage(Properties.Resources.splitter2, 0, 0, DesiredWidth, 1);
            //yoffset += 1;

            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            foreach (var currentBadge in badgeControlList)
            {
                exists = true;
                currentBadge.Location = new Point(tStart, PanelBorder);
                tStart += currentBadge.Image.Size.Width + border;
                if (currentBadge.Image.Height > highest)
                {
                    highest = currentBadge.Image.Height;
                }
            }

            var UserNameColor = (Color)cc.ConvertFromString(twitchMessage.color == "" ? ChatForm.getRandomColor() : twitchMessage.color);
            Brush foreColorBrush = new SolidBrush(ForeColor);
            Brush usernameBrush = new SolidBrush(UserNameColor);

            string usernameText = twitchMessage.display_name + (twitchMessage.username != twitchMessage.display_name.ToLower() ? " (" + twitchMessage.username + ")" : "");
            Size s = GetTextSize(usernameText, Font);
            //int padding = getPadding(Font);
            var location = new Point(tStart, PanelBorder + (exists ? badges[0].Size.Height / 2 - s.Height / 2 : 0));
            yoffset = location.Y;
            e.Graphics.DrawString(usernameText, Font, usernameBrush, location.X, location.Y);

            string text = twitchMessage.message;
            Size theTextSize = GetTextSize(text, Font);

            int currentOffset = 0;
            int theWidth = DesiredWidth - 2 * border;

            int lastX = location.X + s.Width;
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

                List<string> args = new List<string>(offsetText.Split(' '));

                string current = "";
                for (int x = 0; x < args.Count; x++)
                {
                    bool wasInside = false;
                    string old = current;
                    current += args[x];
                    Size currentTextWidth = GetTextSize(current, Font);
                    if (currentTextWidth.Width + lastX > theWidth)
                    {
                        wasInside = true;
                        Size currentArgsWidth = GetTextSize(args[x], Font);
                        if (currentArgsWidth.Width > theWidth)
                        {
                            string gurrent = "";
                            for (int j = 0; j < args[x].Length; j++)
                            {
                                string anotherold = gurrent;
                                gurrent += args[x][j];
                                if (GetTextSize(gurrent + old, Font).Width + lastX > theWidth)
                                {
                                    j = j < 0 ? 0 : j;
                                    args.Insert(x + 1, args[x].Substring(j));
                                    args[x] = args[x].Substring(0, j);
                                    old += anotherold;
                                    x++;
                                    break;
                                }
                            }
                        }
                        e.Graphics.DrawString(old, Font, foreColorBrush, lastX, yoffset);
                        lastX = border;
                        yoffset += theTextSize.Height + (28 / 2 - theTextSize.Height / 2);
                        current = "";
                        x--;
                    }
                    if (x == args.Count - 1)
                    {
                        if (currentTextWidth.Height + yoffset > highest)
                        {
                            highest = currentTextWidth.Height + yoffset;
                        }
                        if (current != "" && current != " ")
                        {
                            e.Graphics.DrawString(current, Font, foreColorBrush, lastX, yoffset);
                            lastX += GetTextSize(current, Font).Width;
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
                    var theimage = emoteControlList[i];
                    theimage.Location = new Point(lastX + EmoteSpacing, yoffset + theTextSize.Height / 2 - thing.img.Size.Height / 2);
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
            base.OnPaint(e);
        }

        public static int getPadding(Font font)
        {
            Size theOg = TextRenderer.MeasureText("asdgreetings", font);
            Size padSize = TextRenderer.MeasureText(".", font);
            Size textSize = TextRenderer.MeasureText("asdgreetings" + ".", font);
            return (theOg.Width+padSize.Width)-textSize.Width;
        }

        private Size GetTextSize(string text, Font font)
        {
            Size padSize = TextRenderer.MeasureText(".", font);
            Size textSize = TextRenderer.MeasureText(text + ".", font);
            return new Size(textSize.Width - padSize.Width, textSize.Height);
        }
    }
}
