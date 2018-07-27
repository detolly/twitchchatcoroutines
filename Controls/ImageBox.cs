using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Linq;

namespace TwitchChatCoroutines.Controls
{
    public static class CurrentAnimations
    {
        public static List<Image> CurrentlyAnimated { get; set; } = new List<Image>();
    }
    public class ImageBox : Control
    {
        public Image Image { get; set; }
        public ToolTip ToolTip { get; set; }

        public ImageBox(Image image)
        {
            Image = image;
            if (!CurrentAnimations.CurrentlyAnimated.Contains(image))
                if (ImageAnimator.CanAnimate(Image))
                {
                    CurrentAnimations.CurrentlyAnimated.Add(image);
                    ImageAnimator.Animate(Image, OnFrameChanged);
                }
            Size = new Size(Image.Size.Width, Image.Size.Height);
            MouseEnter += ImageBox_MouseEnter;
            MouseLeave += ImageBox_MouseLeave;
        }

        ~ImageBox()
        {
            if (CurrentAnimations.CurrentlyAnimated.Contains(Image))
                CurrentAnimations.CurrentlyAnimated.Remove(Image);
            if (ImageAnimator.CanAnimate(Image))
                ImageAnimator.StopAnimate(Image, OnFrameChanged);
        }

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
