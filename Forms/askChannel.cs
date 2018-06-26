using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TwitchChatCoroutines
{
    public partial class askChannel : Form
    {
        public string theChannel;
        public askChannel()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            theChannel = textBox1.Text.ToLower();
            Close();
        }
    }
}
