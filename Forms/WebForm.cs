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
    public partial class WebForm : Form
    {
        public dynamic Auth = null;
        public WebForm()
        {
            InitializeComponent();
            webBrowser1.Navigated += WebBrowser1_Navigated;
        }

        public void WebBrowser1_Navigated(object o, WebBrowserNavigatedEventArgs e)
        {
            if (webBrowser1.Url.OriginalString.Contains("http://localhost/"))
            {
                //let see
                int start = webBrowser1.Url.OriginalString.IndexOf("access_token") + "access_token".Length + 1;
                int stop = webBrowser1.Url.OriginalString.IndexOf("&", start);
                string token = webBrowser1.Url.OriginalString.Substring(start, stop - start);
                dynamic auth = new JObject();
                string name = HelperFunctions.jsonGet("https://api.twitch.tv/kraken/user?authorization=oauth+" + token + "&client_id=" + ChatForm.client_id).name;
                auth.username = name;
                auth.oauth = token;
                Auth = auth;
                Close();
            }
        }
    }
}
