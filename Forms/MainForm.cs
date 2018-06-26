using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace TwitchChatCoroutines.Forms
{
    public partial class MainForm : Form
    {
        static List<ChatForm> chatforms = new List<ChatForm>();
        static List<ChatForm> toRemove = new List<ChatForm>();
        Dictionary<ChatForm, Thread> threads = new Dictionary<ChatForm, Thread>();

        public SortedList<string, Image> cachedBTTVEmotes = new SortedList<string, Image>();
        public SortedList<string, Image> cachedFFZEmotes = new SortedList<string, Image>();
        public SortedList<string, Image> cachedTwitchEmotes = new SortedList<string, Image>();

        public string channel;

        public bool hasClosed
        {
            get; private set;
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            channel = textBox1.Text;
            Thread t = new Thread(() =>
            {
                var text = Program.mainForm.channel;
                var a = new ChatForm(text);
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
    }
}
