using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

using TwitchChatCoroutines.ClassesAndStructs;

namespace TwitchChatCoroutines.Forms
{
    public partial class MainForm : Form
    {
        static List<ChatForm> chatforms = new List<ChatForm>();
        static List<ChatForm> toRemove = new List<ChatForm>();

        public SortedList<string, Image> cachedBTTVEmotes = new SortedList<string, Image>();
        public SortedList<string, Image> cachedFFZEmotes = new SortedList<string, Image>();
        public SortedList<string, Image> cachedTwitchEmotes = new SortedList<string, Image>();

        public static ChatFormSettings chatFormSetting;

        public static Font defaultFont = new Font("Segoe UI", 10f);

        public string channel;

        public bool hasClosed
        {
            get; private set;
        }

        public MainForm()
        {
            InitializeComponent();
            label3.Text = defaultFont.Name + ", " + defaultFont.Size;
            chatFormSetting = new ChatFormSettings();
            chatFormSetting.font = defaultFont;
            chatFormSetting.emoteSpacing = 0;
            chatFormSetting.animations = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            chatFormSetting.animations = checkBox1.Checked;
            chatFormSetting.emoteSpacing = (int)numericUpDown1.Value;
            chatFormSetting.channel = textBox1.Text;
            Thread t = new Thread(() =>
            {
                var settings = chatFormSetting;
                var a = new ChatForm(settings);
                a.Show();
                Stopwatch w = new Stopwatch();
                while (true)
                {
                    if (a.hasClosed)
                        break;
                    w.Start();
                    Application.DoEvents();
                    a.CustomUpdate();
                    w.Stop();
                    if (w.ElapsedMilliseconds > 5000)
                        a.Controls.Clear();
                    w.Reset();
                    Thread.Sleep(1);
                }
            });
            t.Start();
            textBox1.Text = "";
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            hasClosed = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            fontDialog1.ShowDialog();
            chatFormSetting.font = fontDialog1.Font;
            label3.Text = fontDialog1.Font.Name + ", " + fontDialog1.Font.Size;
        }
    }
}
