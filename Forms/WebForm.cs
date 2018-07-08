using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TwitchChatCoroutines.ClassesAndStructs;

namespace TwitchChatCoroutines.Forms
{
    public partial class WebForm : Form
    {
        public dynamic Auth = null;
        public event EventHandler<Auth> authenticated;

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);

        private const int INTERNET_OPTION_SUPPRESS_BEHAVIOR = 3; //INTERNET_SUPPRESS_COOKIE_PERSIST - Suppresses the persistence of cookies, even if the server has specified them as persistent.

        private const int INTERNET_OPTION_END_BROWSER_SESSION = 42;

        private const int INTERNET_SUPPRESS_COOKIE_PERSIST = 81;

        public WebForm()
        {
            InitializeComponent();
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_END_BROWSER_SESSION, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SUPPRESS_BEHAVIOR, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, INTERNET_SUPPRESS_COOKIE_PERSIST, IntPtr.Zero, 0);
            string url = "https://id.twitch.tv/oauth2/authorize?client_id=570bj9vd1lakwt3myr8mrhg05ia5u9&redirect_uri=http://localhost/&response_type=token&scope=chat_login%20user_read";
            url += "&random=" + Guid.NewGuid();
            webBrowser1.Navigated += WebBrowser1_Navigated;
            webBrowser1.Url = new Uri(url);
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
