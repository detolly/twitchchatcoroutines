using System;
using System.Windows.Forms;
using System.Threading;
using CoroutineSystem;

using TwitchChatCoroutines.Forms;

namespace TwitchChatCoroutines
{
    static class Program
    {
        internal static MainForm mainForm;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

    }
}
