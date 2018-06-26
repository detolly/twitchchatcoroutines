using System;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Forms;
using System.Threading;
using CoroutineSystem;
using System.Diagnostics;

namespace TwitchChatCoroutines
{
    static class Program
    {
        internal static ChatForm mainForm;
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
            mainForm = new ChatForm();
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
