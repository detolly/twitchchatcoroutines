using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace TwitchChatCoroutines.Forms
{
    public partial class MainForm : Form
    {
        static List<ChatForm> chatforms = new List<ChatForm>();
        static List<ChatForm> toRemove = new List<ChatForm>();
        Dictionary<ChatForm, Thread> threads = new Dictionary<ChatForm, Thread>();

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
                while (true)
                {
                    if (a.hasClosed)
                        break;
                    Application.DoEvents();
                    a.CustomUpdate();
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
