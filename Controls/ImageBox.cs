using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Linq;
using TwitchChatCoroutines.ClassesAndStructs;

namespace TwitchChatCoroutines.Controls
{

    public class ImageBox : Control
    {
        public Image Image { get; set; }
        public ToolTip ToolTip { get; set; }

        public ImageBox(Image image)
        {
            Image = image;
            Size = new Size(Image.Size.Width, Image.Size.Height);
            MouseEnter += ImageBox_MouseEnter;
            MouseLeave += ImageBox_MouseLeave;
        }

        //~ImageBox()
        //{
        //    if (CurrentAnimations.RegisteredControls.Contains(this))
        //        CurrentAnimations.RegisteredControls.Remove(this);
        //}

        protected override void Dispose(bool disposing)
        {
            foreach (Control c in Controls)
                c.Dispose();
            base.Dispose(disposing);
        }

        private void ImageBox_MouseLeave(object sender, EventArgs e)
        {
            if (ToolTip != null)
                ToolTip.Visible = false;
        }

        private void ImageBox_MouseEnter(object sender, EventArgs e)
        {
            if (ToolTip != null)
                ToolTip.Visible = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(Image, 0, 0, Image.Size.Width, Image.Size.Height);
            Size = new Size(Image.Size.Width, Image.Size.Height);
        }
    }
}
