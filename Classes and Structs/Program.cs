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
        internal static uint count = 0;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            CoroutineManager.Init();
            mainForm = new MainForm();
            if (!mainForm.IsDisposed) mainForm.Show();
            while (true)
            {
                if (mainForm.hasClosed) break;
                Application.DoEvents();
                mainForm.CustomUpdate();
                CoroutineManager.Interval();
                Thread.Sleep(1);
                count++;
                if (count % 30000 == 0)
                {
                    GC.Collect();
                    count = 8;
                }
            }
        }

    }
}
