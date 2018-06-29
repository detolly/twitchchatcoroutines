using System;
using System.Drawing;

namespace TwitchChatCoroutines.ClassesAndStructs
{
    public class ChatFormSettings
    {
        public event EventHandler changed;

        private ChatForm current = null;
        public ChatForm Current { get { return current; } set {  current = value; changed?.Invoke(this, new EventArgs()); } }
        private Font font;
        public Font Font { get { return font; } set {  font = value; changed?.Invoke(this, new EventArgs()); } }
        private int emoteSpacing;
        public int EmoteSpacing { get { return emoteSpacing; } set {  emoteSpacing = value; changed?.Invoke(this, new EventArgs()); } }
        private bool animations;
        public bool Animations { get { return animations; } set {  animations = value; changed?.Invoke(this, new EventArgs()); } }
        private bool splitter;
        public bool Splitter { get { return splitter; } set { splitter = value; changed?.Invoke(this, new EventArgs()); } }
        private string channel;
        public string Channel { get { return channel; } set {  channel = value; changed?.Invoke(this, new EventArgs()); } }
        private Color backgroundColor;
        public Color BackgroundColor { get { return backgroundColor; } set { backgroundColor = value; changed?.Invoke(this, new EventArgs()); } }
        private Color foregroundColor;
        public Color ForegroundColor { get { return foregroundColor; } set { foregroundColor = value; changed?.Invoke(this, new EventArgs()); } }
    }
}
