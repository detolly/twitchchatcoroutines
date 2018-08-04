using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System;

using TwitchChatCoroutines.ClassesAndStructs;

namespace TwitchChatCoroutines.Controls
{
    class MessageControl : UserControl {}

    class SentUserMessage : MessageControl
    {
        private TwitchMessage twitchMessage;
        private int panelBorder;
        private int border;
        private int desiredWidth;

        public SentUserMessage(TwitchMessage TwitchMessage, int PanelBorder, int border, int desiredWidth)
        {
            twitchMessage = TwitchMessage;
            panelBorder = PanelBorder;
            this.border = border;
            this.desiredWidth = desiredWidth;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            string theMessage = twitchMessage.username + ": " + twitchMessage.message;
            e.Graphics.DrawString(theMessage, Font, new SolidBrush((Color)new ColorConverter().ConvertFromString(twitchMessage.color)), new Point(border, panelBorder));
            Size = new Size(desiredWidth, Math.Max(TextRenderer.MeasureText(theMessage, Font).Height + 2*panelBorder, 28));
            base.OnPaint(e);
        }
    }

    class TwitchUserMessage : MessageControl
    {
        public List<Image> badges;

        public SortedList<int, ImageAndInts> emotes;
        public TwitchMessage twitchMessage;

        public int DesiredWidth { get; set; }
        public int EmoteSpacing { get; set; }
        public bool DoSplitter { get; set; }
        public bool IsAction { get; set; }
        public int PanelBorder { get; set; }

        private ColorConverter cc;
        private bool ready = false;

        private List<Tuple<ImageAndInts, Point>> listOfImagesToDraw = new List<Tuple<ImageAndInts, Point>>();
        private List<Tuple<string, Point, Color>> listOfTextToDraw = new List<Tuple<string, Point, Color>>();

        List<ImageAndInts> currently = new List<ImageAndInts>();

        public void Init()
        {
            cc = new ColorConverter();
            ready = true;
        }
        
        public TwitchUserMessage(TwitchMessage message, List<Image> badges, SortedList<int, ImageAndInts> emotes, Font font, bool doSplitter, Color foreColor, Color backColor, int desiredWidth, int panelBorder, int emoteSpacing)
        {
            twitchMessage = message;
            Font = font;
            DoSplitter = doSplitter;
            ForeColor = foreColor;
            BackColor = backColor;
            DesiredWidth = desiredWidth;
            PanelBorder = panelBorder;
            EmoteSpacing = emoteSpacing; 

            this.badges = badges;
            this.emotes = emotes;

            Init();
            CalculateTextAndEmotes();
        }

        public void CalculateTextAndEmotes()
        {
            listOfImagesToDraw.Clear();
            listOfTextToDraw.Clear();

            int border = 5;
            int tStart = border;
            int yoffset = 0;

            int highest = 0;
            int lowest = int.MaxValue;

            bool exists = false;

            foreach (var currentBadge in badges)
            {
                exists = true;
                var theLocation = new Point(tStart, PanelBorder);
                listOfImagesToDraw.Add(new Tuple<ImageAndInts, Point>(new ImageAndInts() { img = currentBadge }, theLocation));
                tStart += currentBadge.Size.Width + border;
                if (currentBadge.Height > highest)
                {
                    highest = currentBadge.Height;
                }
            }

            var UserNameColor = (Color)cc.ConvertFromString(twitchMessage.color == "" ? ChatForm.getRandomColor() : twitchMessage.color);
            ForeColor = IsAction ? UserNameColor : ForeColor;
            Brush foreColorBrush = new SolidBrush(ForeColor);
            Brush usernameBrush = new SolidBrush(UserNameColor);

            string usernameText = twitchMessage.display_name + (twitchMessage.username != twitchMessage.display_name.ToLower() ? " (" + twitchMessage.username + ")" : "");
            Size s = GetTextSize(usernameText, Font);
            var location = new Point(tStart, PanelBorder + (exists ? badges[0].Size.Height / 2 - s.Height / 2 : 0));
            yoffset = location.Y;
            if (location.Y < lowest)
                lowest = location.Y;
            listOfTextToDraw.Add(new Tuple<string, Point, Color>(usernameText, location, UserNameColor));
            
            string text = twitchMessage.message;
            Size theTextSize = GetTextSize(text, Font);

            int currentOffset = 0;
            int theWidth = DesiredWidth - 2 * border;

            int lastX = location.X + s.Width;

            bool first = true;

            for (int i = 0; i < emotes.Count + 1; i++)
            {
                var thing = i != emotes.Count ? emotes.Values[i] : new ImageAndInts() /* To avoid errors on compile time */;
                var theTuple = thing.ints;
                int next = i != emotes.Count ? thing.ints.Item1 : text.Length;
                string offsetText = text.Substring(currentOffset > text.Length - 1 ? text.Length - 1 : currentOffset, (next - currentOffset < 0 ? 0 : next - currentOffset));
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
                        if (old != "")
                            x--;
                        listOfTextToDraw.Add(new Tuple<string, Point, Color>(old, new Point(lastX, yoffset), ForeColor));
                        yoffset += theTextSize.Height + (28 / 2 - theTextSize.Height / 2);
                        lastX = border;
                        current = "";
                    }
                    if (x == args.Count - 1)
                    {
                        if (currentTextWidth.Height + yoffset > highest)
                        {
                            highest = currentTextWidth.Height + yoffset;
                        }
                        if (current != "" && current != " ")
                        {
                            listOfTextToDraw.Add(new Tuple<string, Point, Color>(current, new Point(lastX, yoffset), ForeColor));
                            lastX += GetTextSize(current, Font).Width;
                        }
                    }
                    else if (!wasInside)
                        current += " ";
                }
                if (i != emotes.Count)
                {
                    if (thing.img == null) return;
                    Size theSize = thing.preferredSize.Width != 0 && thing.preferredSize.Height != 0 ? thing.preferredSize : new Size(thing.img.Size.Width, thing.img.Size.Height);
                    if (lastX + theSize.Width > theWidth)
                    {
                        lastX = border;
                        yoffset += theTextSize.Height + (28 / 2 - theTextSize.Height / 2);
                    }
                    var pa = new Point(lastX + EmoteSpacing, yoffset + theTextSize.Height / 2 - theSize.Height / 2);
                    if (pa.Y < lowest)
                        lowest = pa.Y;
                    listOfImagesToDraw.Add(new Tuple<ImageAndInts, Point>(thing, pa));
                    if (yoffset + theSize.Height + theTextSize.Height / 2 - theSize.Height / 2 > highest)
                    {
                        highest = yoffset + theSize.Height + theTextSize.Height / 2 - theSize.Height / 2;
                    }
                    lastX += theSize.Width;
                    lastX += 2 * EmoteSpacing;
                }
                else
                    break;
            }
            if (lowest < PanelBorder)
            {
                List<Tuple<ImageAndInts, Point>> tempList = new List<Tuple<ImageAndInts, Point>>();
                foreach (var t in listOfImagesToDraw)
                {
                    tempList.Add(new Tuple<ImageAndInts, Point>(t.Item1, new Point(t.Item2.X, t.Item2.Y + Math.Abs(lowest) + PanelBorder)));
                }
                listOfImagesToDraw = tempList;
                List<Tuple<string, Point, Color>> tempList2 = new List<Tuple<string, Point, Color>>();
                foreach (var t in listOfTextToDraw)
                {
                    tempList2.Add(new Tuple<string, Point, Color>(t.Item1, new Point(t.Item2.X, t.Item2.Y + Math.Abs(lowest) + PanelBorder), t.Item3));
                }
                listOfTextToDraw = tempList2;
            }
            DrawContent(CreateGraphics());
            Size = new Size(DesiredWidth * 2, Math.Max(highest - lowest + 2 * PanelBorder, 28));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!ready) return;

            DrawContent(e.Graphics);
            //base.OnPaint(e);
        }

        public void DrawContent(Graphics g)
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.Clear(BackColor);
            if (DoSplitter)
                g.DrawImage(Properties.Resources.splitter2, 0, 0, DesiredWidth*2, 1);
            foreach (var t in listOfTextToDraw)
            {
                TextRenderer.DrawText(g, t.Item1, Font, t.Item2, t.Item3, BackColor, TextFormatFlags.NoPadding);
            }
            foreach (var t in listOfImagesToDraw)
            {
                Size size = t.Item1.preferredSize.Width != 0 && t.Item1.preferredSize.Height != 0 ? t.Item1.preferredSize : new Size(t.Item1.img.Size.Width, t.Item1.img.Size.Height);
                g.DrawImage(t.Item1.img, t.Item2.X, t.Item2.Y, size.Width, size.Height);
            }
        }

        private Size GetTextSize(string text, Font font)
        {
            Size padSize = TextRenderer.MeasureText(".", font);
            Size textSize = TextRenderer.MeasureText(text + ".", font);
            return new Size(textSize.Width - padSize.Width, textSize.Height);
        }
    }
}
