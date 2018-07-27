using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace TwitchChatCoroutines.Controls
{
    public partial class ImageBox : Control
    {
        public Image Image { get; set; }
        public ToolTip ToolTip { get; set; }

        public ImageBox(Image image)
        {
            Image = image;
            if (ImageAnimator.CanAnimate(Image))
                ImageAnimator.Animate(Image, OnFrameChanged);
            this.Size = new Size(Image.Size.Width, Image.Size.Height);
            MouseEnter += ImageBox_MouseEnter;
            MouseLeave += ImageBox_MouseLeave;
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

        private void OnFrameChanged(object o, EventArgs e)
        {
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            ImageAnimator.UpdateFrames();
            e.Graphics.DrawImage(Image, 0, 0, Image.Size.Width, Image.Size.Height);
            Size = new Size(Image.Size.Width, Image.Size.Height);
        }
    }
}
