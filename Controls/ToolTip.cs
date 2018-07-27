using System;
using System.Drawing;
using System.Windows.Forms;
using TwitchChatCoroutines.ClassesAndStructs;

namespace TwitchChatCoroutines.Controls
{
    public class ToolTip : Panel
    {
        public static readonly int border = 5;
        public static readonly int spacing = 10;

        private string text;
        public override string Text
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
        private Control customParent;
        public Control CustomParent { get
            {
                return customParent;
            }
            set
            {
                customParent = value;
                UpdateSizeAndLocation();
            }
        }
        public bool IsCreated { get; set; }

        public ToolTip() { }

        public ToolTip(Control parent) {
            CustomParent = parent;
        }

        private void UpdateSizeAndLocation()
        {
            int middleOfParentX = CustomParent.Location.X + CustomParent.Width / 2;
            PictureBox b = new PictureBox
            {
                Image = Image,
                SizeMode = PictureBoxSizeMode.AutoSize,
            };
            b.Location = new Point(border, border);
            Controls.Add(b);
            TwitchLabel l = new TwitchLabel(BackColor)
            {
                Text = Text,
            };
            l.ForeColor = ForeColor;
            l.Location = new Point(b.Right+border, b.Location.Y+b.Size.Height/2-l.Size.Height/2);
            Controls.Add(l);
            Size = new Size(l.Right + border, Math.Max(l.Bottom, b.Bottom)+border);
        }
    }
}
