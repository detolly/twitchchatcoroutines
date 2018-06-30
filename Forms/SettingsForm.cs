using System;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace TwitchChatCoroutines.Forms
{
    public partial class SettingsForm : Form
    {
        public event EventHandler saved;

        public SettingsForm()
        {
            InitializeComponent();
            var json = JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText("settings.json"));
            twitchBox.Checked = json.general.twitchEmoteCaching;
            bttvBox.Checked = json.general.bttvEmoteCaching;
            ffzBox.Checked = json.general.ffzEmoteCaching;
            emotesBox.Checked = json.general.emotesCaching;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var json = JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText("settings.json"));
            json.general.twitchEmoteCaching = twitchBox.Checked;
            json.general.bttvEmoteCaching = bttvBox.Checked;
            json.general.ffzEmoteCaching = ffzBox.Checked;
            json.general.emotesCaching = emotesBox.Checked;
            var theString = JsonConvert.SerializeObject(json);
            System.IO.File.WriteAllText("settings.json", theString);
            saved?.Invoke(this, new EventArgs());
            Close();
            Dispose();
        }
    }
}
