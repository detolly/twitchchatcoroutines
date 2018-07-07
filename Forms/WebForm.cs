using System;
using System.Windows.Forms;
using TwitchChatCoroutines.ClassesAndStructs;

namespace TwitchChatCoroutines.Forms
{
    public partial class WebForm : Form
    {
        public dynamic Auth = null;
        public event EventHandler<Auth> authenticated;

        public WebForm()
        {
            InitializeComponent();
            webBrowser1.Navigated += WebBrowser1_Navigated;
        }

        public void WebBrowser1_Navigated(object o, WebBrowserNavigatedEventArgs e)
        {
            if (webBrowser1.Url.OriginalString.StartsWith("http://localhost/"))
            {
                //let see
                int start = webBrowser1.Url.OriginalString.IndexOf("access_token") + "access_token".Length + 1;
                int stop = webBrowser1.Url.OriginalString.IndexOf("&", start);
                string token = webBrowser1.Url.OriginalString.Substring(start, stop - start);
                string[] headers = new string[]
                {
                    "Authorization: OAuth " + token,
                    "Client-ID: " + ChatForm.client_id
                };
                string name = HelperFunctions.jsonGet("https://api.twitch.tv/kraken/user?authorization=oauth+" + token + "&client_id=" + ChatForm.client_id, headers).name;
                dynamic auth = new Auth();
                auth.username = name;
                auth.oauth = token;
                authenticated?.Invoke(this, auth);
                Close();
            }
        }
    }
}
