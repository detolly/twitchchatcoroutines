using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TwitchChatCoroutines.Forms
{
    public partial class MainForm : Form
    {
        List<ChatForm> chatforms = new List<ChatForm>();
        List<ChatForm> toRemove = new List<ChatForm>();

        public bool hasClosed
        {
            get; private set;
        }

        public MainForm()
        {
            InitializeComponent();
        }

        public void CustomUpdate()
        {
            for (int i = 0; i < chatforms.Count; i++)
            {
                if (chatforms[i].hasClosed)
                    toRemove.Add(chatforms[i]);
                chatforms[i].CustomUpdate();
            }
            for (int i = 0; i < toRemove.Count; i++)
            {
                chatforms.Remove(toRemove[i]);
            }
            toRemove.Clear();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var a = new ChatForm(textBox1.Text);
            chatforms.Add(a);
            a.Show();
            textBox1.Text = "";
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            hasClosed = true;
        }
    }
}
