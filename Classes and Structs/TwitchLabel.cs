using System;
using System.Drawing;
using System.Windows.Forms;

namespace TwitchChatCoroutines.ClassesAndStructs
{
    class TwitchLabel : TextBox
    {
        private ChatForm owner;

        public TwitchLabel(ChatForm form)
        {
            owner = form;
            BorderStyle = BorderStyle.None;
            BackColor = owner.BackColor;
            ReadOnly = true;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            try
            {
                base.OnHandleCreated(e);
            } catch { }
            AutoSize = false;
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
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
