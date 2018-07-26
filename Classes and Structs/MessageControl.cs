using System.Collections.Generic;
using System.Windows.Forms;

namespace TwitchChatCoroutines.ClassesAndStructs
{
    class MessageControl : Panel
    {
        public SortedList<int, PictureBoxAndInts> emotes;
        public List<PictureBox> badges;
        public List<TwitchLabel> messages;
        public List<Controls.ToolTip> tooltips;
        public TwitchLabel username;

        public bool isAction;
        public bool doSplitter = false;

        public TwitchMessage twitchMessage;
        public PictureBox splitter;

        protected override void OnPaint(PaintEventArgs e)
        {
            int tStart = 0;
            bool exists = false;
            foreach (var s in m.badges)
            {
                exists = true;
                s.Location = new Point(tStart + border, 100);
                tStart += s.Size.Width + border;
            }
            TwitchLabel userNameLabel = new TwitchLabel(this.BackColor)
            {
                Font = font,
                Text = m.twitchMessage.display_name + (m.twitchMessage.username != m.twitchMessage.display_name.ToLower() ? " (" + m.twitchMessage.username + ")" : ""),
                ForeColor = (Color)cc.ConvertFromString(m.twitchMessage.color == "" ? getRandomColor() : m.twitchMessage.color)
            };
            userNameLabel.Location = new Point(tStart + border, 100 + (exists ? m.badges[0].Size.Height / 2 - userNameLabel.Size.Height / 2 : 0));
            p.Controls.Add(userNameLabel);
            string text = m.twitchMessage.message;

            int nextStart = 0;
            int lastLocation = userNameLabel.Right;
            bool first = true;
            int yoffset = 0;

            foreach (var pbandInt in emoteBoxes)
            {
                Tuple<int, int> ints = pbandInt.Value.ints;
                PictureBox pb = pbandInt.Value.pb;
                TwitchLabel thel = new TwitchLabel(this.BackColor);
                if (first)
                {
                    thel.Text = ": ";
                    first = false;
                }
                thel.Font = font;
                thel.ForeColor = m.isAction ? (Color)cc.ConvertFromString(m.twitchMessage.color == "" ? "#FFFFFF" : m.twitchMessage.color) : textColor;
                thel.Text += text.Substring(nextStart, (ints.Item1 - nextStart) < 0 ? 0 : ints.Item1 - nextStart);
                TwitchLabel comparison = thel;
                if (thel.Text != "" && thel.Text != " ")
                {
                    if (thel.Text[0] == ' ')
                        thel.Text = thel.Text.Substring(1);
                    if (thel.Text[thel.Text.Length - 1] == ' ')
                        thel.Text = thel.Text.Substring(0, thel.Text.Length - 1);
                    p.Controls.Add(thel);
                    thel.Location = new Point(lastLocation, userNameLabel.Location.Y + yoffset);
                    int startingLoc = comparison.Location.X + border + TextRenderer.MeasureText(".", font).Width * 2;
                    if (TextRenderer.MeasureText(comparison.Text, font).Width + startingLoc > Width - (vScrollBar1.Visible ? vScrollBar1.Width : 0))
                    {
                        var args = new List<string>(thel.Text.Split(' '));
                        string upTillNow = "";
                        for (int i = 0; i < args.Count; i++)
                        {
                            startingLoc = comparison.Location.X + border + TextRenderer.MeasureText(".", font).Width * 2;
                            string old = upTillNow;
                            upTillNow += args[i];
                            if (TextRenderer.MeasureText(upTillNow, font).Width + startingLoc > Width - (vScrollBar1.Visible ? vScrollBar1.Width : 0))
                            {
                                var a = TextRenderer.MeasureText(args[i], font).Width;
                                if (a + 2 * border > Width - vScrollBar1.Width)
                                {
                                    string current = "";
                                    for (int x = 0; x < args[i].Length; x++)
                                    {
                                        string anotherold = current;
                                        current += args[i][x];
                                        if (TextRenderer.MeasureText(current + old, font).Width + startingLoc > Width - (vScrollBar1.Visible ? vScrollBar1.Width : 0))
                                        {
                                            x = x < 0 ? 0 : x;
                                            args.Insert(i + 1, args[i].Substring(x));
                                            args[i] = args[i].Substring(0, x);
                                            old += anotherold;
                                            i++;
                                            break;
                                        }
                                    }
                                }
                                comparison.Text = old;
                                yoffset += comparison.Height + (28 / 2 - comparison.Size.Height / 2);
                                TwitchLabel newLabel = new TwitchLabel(this.BackColor)
                                {
                                    Font = font,
                                    ForeColor = m.isAction ? (Color)cc.ConvertFromString(m.twitchMessage.color == "" ? "#FFFFFF" : m.twitchMessage.color) : textColor
                                };
                                p.Controls.Add(newLabel);
                                newLabel.Location = new Point(border, userNameLabel.Location.Y + yoffset);
                                comparison = newLabel;
                                labelsToAdd.Add(newLabel);
                                lastLocation = newLabel.Right;
                                upTillNow = "";
                                i--;
                            }
                            else
                            {
                                upTillNow += " ";
                            }
                            if (i == args.Count - 1)
                            {
                                comparison.Text = upTillNow.Substring(0, upTillNow.Length - 1);
                            }
                        }
                    }
                    int rightborder = comparison.Right + pb.Width;
                    lastLocation = rightborder > Width ? border : rightborder - pb.Width + emoteSpacing;
                    yoffset += rightborder > Width ? 28 / 2 : 0;
                    labelsToAdd.Add(thel);
                }
                nextStart = ints.Item2 + 1;
                pb.Parent = comparison;
                int theOr = lastLocation + (pb.Size.Width * 2) + border;
                yoffset += theOr > Width ? Math.Max(28, comparison.Height) : 0;
                pb.Location = new Point(theOr > Width ? border : lastLocation, userNameLabel.Location.Y + userNameLabel.Size.Height / 2 - pb.Size.Height / 2 + yoffset);
                lastLocation = pb.Right + emoteSpacing;
                p.Controls.Add(pb);
            }
            TwitchLabel lastLabel = new TwitchLabel(this.BackColor)
            {
                ForeColor = m.isAction ? (Color)cc.ConvertFromString(m.twitchMessage.color == "" ? "#FFFFFF" : m.twitchMessage.color) : textColor,
                Font = font
            };
            lastLabel.MaximumSize = new Size(Width - 20 - lastLabel.Size.Width - userNameLabel.Size.Width, 0);
            string theT = text.Substring(nextStart > text.Length ? text.Length - 1 : nextStart, (text.Length - nextStart < 0) ? 0 : text.Length - nextStart);
            lastLabel.Text = first ? ": " + theT : theT;
            if (first || theT.Length > 0)
            {
                if (lastLabel.Text[0] == ' ')
                    lastLabel.Text = lastLabel.Text.Substring(1);
                p.Controls.Add(lastLabel);
                lastLabel.Location = new Point(lastLocation, userNameLabel.Location.Y + yoffset);
                labelsToAdd.Add(lastLabel);
                TwitchLabel labelToCompare = lastLabel;
                var args = new List<string>(lastLabel.Text.Split(' '));
                string stringCompare = "";
                for (int i = 0; i < args.Count; i++)
                {
                    int startingLoc = labelToCompare.Location.X + border + TextRenderer.MeasureText(".", font).Width * 2;
                    string old = stringCompare;
                    stringCompare += args[i];
                    if (TextRenderer.MeasureText(stringCompare, font).Width + startingLoc > Width - (vScrollBar1.Visible ? vScrollBar1.Width : 0))
                    {
                        var a = TextRenderer.MeasureText(args[i], font).Width;
                        if (a + 2 * border > Width - (vScrollBar1.Visible ? vScrollBar1.Width : 0))
                        {
                            string current = "";
                            for (int x = 0; x < args[i].Length; x++)
                            {
                                string anotherold = current;
                                current += args[i][x];
                                if (TextRenderer.MeasureText(current + old, font).Width + startingLoc > Width - (vScrollBar1.Visible ? vScrollBar1.Width : 0))
                                {
                                    x = x < 0 ? 0 : x;
                                    args.Insert(i + 1, args[i].Substring(x));
                                    args[i] = args[i].Substring(0, x);
                                    old += anotherold;
                                    i++;
                                    break;
                                }
                            }
                        }
                        labelToCompare.Text = old;
                        yoffset += labelToCompare.Height + (28 / 2 - labelToCompare.Size.Height / 2);
                        TwitchLabel l = new TwitchLabel(this.BackColor)
                        {
                            Location = new Point(border, userNameLabel.Location.Y + yoffset),
                            Font = font,
                            ForeColor = m.isAction ? (Color)cc.ConvertFromString(m.twitchMessage.color == "" ? "#FFFFFF" : m.twitchMessage.color) : textColor,
                            Parent = labelToCompare
                        };
                        p.Controls.Add(l);
                        i--;
                        stringCompare = "";
                        labelsToAdd.Add(l);
                        labelToCompare = l;
                    }
                    else
                    {
                        stringCompare += " ";
                    }
                    if (i == args.Count - 1)
                        labelToCompare.Text = stringCompare;
                }
            }
            int highest = 0;
            int lowest = 1000;
            PictureBox splitterbox = null;
            splitterbox = new PictureBox
            {
                Image = splitter,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = new Size(2 * Width, 1)
            };
            Control lowestC = null;
            int lowestCS = 1000;
            if (!doSplitter)
                splitterbox.Visible = false;
            for (int i = 0; i < p.Controls.Count; i++)
            {
                if (p.Controls[i].Size.Height + p.Controls[i].Location.Y > highest)
                    highest = p.Controls[i].Bottom;
                if (p.Controls[i].Location.Y < lowest)
                    lowest = p.Controls[i].Location.Y;
                if (p.Controls[i] is TwitchLabel)
                {
                    if (p.Controls[i].Location.Y < lowestCS)
                    {
                        lowestC = p.Controls[i];
                        lowestCS = p.Controls[i].Location.Y;
                    }
                }
            }
            if (highest - lowest + panelBorder < 28)
            {
                int diff = highest - lowest;
                lowest -= (28 - diff) / 2;
                highest += (28 - diff) / 2;
            }
            p.Size = new Size(2 * Width, highest - lowest + panelBorder);
            splitterbox.Location = new Point(0, lowest /* can be lowestCS */ - panelBorder / 2);
            lowest = lowest > splitterbox.Location.Y ? splitterbox.Top : lowest;
            p.Controls.Add(splitterbox);
            splitterbox.SendToBack();
            for (int i = 0; i < p.Controls.Count; i++)
            {
                p.Controls[i].Location = new Point(p.Controls[i].Location.X, p.Controls[i].Location.Y - lowest + panelBorder / 2);
            }
        }
    }
}
