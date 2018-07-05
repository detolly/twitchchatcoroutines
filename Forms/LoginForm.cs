using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TwitchChatCoroutines.ClassesAndStructs;

namespace TwitchChatCoroutines.Forms
{
    public partial class LoginForm : Form
    {
        public dynamic Auth;
        private Dictionary<string, string> headers = new Dictionary<string, string>()
        {
            {
                "Client-ID", ChatForm.client_id
            }
        };

        public LoginForm()
        {
            InitializeComponent();
            webBrowser1.Navigated += WebBrowser1_Navigated;
        }

        private void WebBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (webBrowser1.Url.OriginalString.Contains("http://localhost/"))
            {
                //let see
                int start = webBrowser1.Url.OriginalString.IndexOf("access_token") + "access_token".Length;
                int stop = webBrowser1.Url.OriginalString.IndexOf("&", start);
                string token = webBrowser1.Url.OriginalString.Substring(start + 1, stop - start);
                dynamic auth = new JObject();
                string[] currentHeaders = new string[] {
                        "Client-ID: " + ChatForm.client_id,
                        "Authorization: OAuth" + token
                };
                string name = HelperFunctions.jsonGet("https://api.twitch.tv/kraken/user", currentHeaders).name;
                auth.username = name;
                auth.oauth = token;
            }
        }
    }
}
