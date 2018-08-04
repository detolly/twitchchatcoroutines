using System;
using System.Drawing;
using System.Windows.Forms;

namespace TwitchChatCoroutines.ClassesAndStructs
{
    public class ChatFormSettings
    {
        public event EventHandler Changed;

        private ChatForm current = null;
        public ChatForm Current { get { return current; } set { current = value; Changed?.Invoke(this, new EventArgs()); } }
        private Font font;
        public Font Font { get { return font; } set { font = value; Changed?.Invoke(this, new EventArgs()); } }
        private int emoteSpacing;
        public int EmoteSpacing { get { return emoteSpacing; } set { emoteSpacing = value; Changed?.Invoke(this, new EventArgs()); } }
        private bool animations;
        public bool Animations { get { return animations; } set { animations = value; Changed?.Invoke(this, new EventArgs()); } }
        private bool splitter;
        public bool Splitter { get { return splitter; } set { splitter = value; Changed?.Invoke(this, new EventArgs()); } }
        private string channel;
        public string Channel { get { return channel; } set { channel = value; Changed?.Invoke(this, new EventArgs()); } }
        private Color backgroundColor;
        public Color BackgroundColor { get { return backgroundColor; } set { backgroundColor = value; Changed?.Invoke(this, new EventArgs()); } }
        private int panelBorder;
        public int PanelBorder { get { return panelBorder; } set { panelBorder = value; Changed?.Invoke(this, new EventArgs()); } }
        private Color foregroundColor;
        public Color ForegroundColor { get { return foregroundColor; } set { foregroundColor = value; Changed?.Invoke(this, new EventArgs()); } }
        private ChatMode chatMode;
        public ChatMode ChatMode { get { return chatMode; } set { chatMode = value; Changed?.Invoke(this, new EventArgs()); } }
        private FormBorderStyle borderStyle;
        public FormBorderStyle BorderStyle { get { return borderStyle; } set { borderStyle = value; Changed?.Invoke(this, new EventArgs()); } }

        public static ChatFormSettings Default()
        {
            ChatFormSettings settings = new ChatFormSettings
            {
                ForegroundColor = Color.White,
                BackgroundColor = Color.FromArgb(255, 40, 40, 50),
                Animations = false,
                Font = new Font("Segoe UI", 9.75f),
                EmoteSpacing = 3,
                PanelBorder = 6,
                Channel = "forsen",
                Splitter = true,
                ChatMode = new ChatMode()
                {
                    currentIndex = 0
                },
                BorderStyle = FormBorderStyle.Sizable
            };
            return settings;
        }

    }
}
