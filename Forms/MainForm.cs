using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using TwitchChatCoroutines.ClassesAndStructs;

namespace TwitchChatCoroutines.Forms
{
    public partial class MainForm : Form
    {
        private string version = "v0.3-alpha-e";

        static List<ChatForm> chatforms = new List<ChatForm>();
        static List<ChatForm> toRemove = new List<ChatForm>();

        public SortedList<string, Image> cachedBTTVEmotes = new SortedList<string, Image>();
        public SortedList<string, Image> cachedFFZEmotes = new SortedList<string, Image>();
        public SortedList<string, Image> cachedTwitchEmotes = new SortedList<string, Image>();

        public static TwitchSettings generalSettings;

        int amountOfThings = 0;
        public RadioButton[] radios;

        private ColorConverter cc = new ColorConverter();

        public static ChatFormSettings[] chatFormSettings;

        public static Font defaultFont = new Font("Segoe UI", 9.75f);

        public string channel;
        int selectedIndex = 0;

        public bool hasClosed
        {
            get; private set;
        }

        public MainForm()
        {
            InitializeComponent();
            CheckGeneralSettings();
            generalSettings = TwitchSettings.Interpret(JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText("settings.json")));
            if (Directory.Exists("./.AutoUpdater"))
            {
                string fileName = "./.AutoUpdater/AutoUpdater.exe";
                File.Delete("./AutoUpdater.exe");
                File.Copy(fileName, "./AutoUpdater.exe");
                File.Delete(fileName);
                Directory.Delete("./.AutoUpdater");
                MessageBox.Show("Update Complete!");
            }
            using (WebClient client = new WebClient())
            {
                string v = "";
                try
                {
                    v = client.DownloadString("http://blog.detolly.no/version.txt");
                }
                catch
                {
                    v = version;
                    MessageBox.Show("Internet connection not present. Please connect to the internet to use this application.");
                }
                if (v != version)
                {
                    DialogResult result = MessageBox.Show("New update available. \nNew update version: " + v + "\n Want to update?", "Update", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        Process.Start("AutoUpdater.exe", "--url=\"http://blog.detolly.no/TwitchChat-" + v + ".zip");
                        Process.GetCurrentProcess().Kill();
                    }
                }
            }
            Fontlabel.Text = defaultFont.Name + ", " + defaultFont.Size;
            radios = new RadioButton[] {
                radioButton1,
                radioButton2,
                radioButton3,
                radioButton4,
                radioButton5,
                radioButton6
            };
            amountOfThings = radios.Length;
            chatFormSettings = new ChatFormSettings[amountOfThings];
            for (int i = 0; i < amountOfThings; i++)
            {
                chatFormSettings[i] = ChatFormSettings.Default();
            }
            foreach (var s in (ChatModes[])Enum.GetValues(typeof(ChatModes)))
            {
                comboBox1.Items.Add(s);
            }
            radioButton1.Checked = true;
        }

        private void CheckGeneralSettings()
        {
            string text = File.ReadAllText("settings.json");
            dynamic json = JsonConvert.DeserializeObject<dynamic>(text);
            if (json.authentication == null)
                json.authentication = new JObject();
            if (json.general == null)
                json.general = new JObject();
            if (json.general.bttvEmoteCaching == null)
                json.general.bttvEmoteCaching = true;
            if (json.general.twitchEmoteCaching == null)
                json.general.twitchEmoteCaching = true;
            if (json.general.ffzEmoteCaching == null)
                json.general.ffzEmoteCaching = true;
            if (json.general.emotesCaching == null)
                json.general.emotesCaching = true;

            if (json.users == null)
                json.users = new JObject();
            text = JsonConvert.SerializeObject(json);
            File.WriteAllText("settings.json", text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int index = selectedIndex;
            radios[index].Text = textBox1.Text;
            button1.Enabled = false;
            chatFormSettings[index].Animations = AnimationsCheckBox.Checked;
            chatFormSettings[index].EmoteSpacing = (int)Emotespacing.Value;
            chatFormSettings[index].Channel = textBox1.Text;
            chatFormSettings[index].ForegroundColor = (Color)cc.ConvertFromString(ForegroundColorBox.Text);
            chatFormSettings[index].BackgroundColor = (Color)cc.ConvertFromString(BackgroundColorBox.Text);

            Thread t = new Thread(() =>
            {
                var settings = chatFormSettings[index];
                var a = new ChatForm(settings);
                chatFormSettings[index].Current = a;
                a.Show();
                while (true)
                {
                    if (a.hasClosed)
                        break;
                    Application.DoEvents();
                    a.CustomUpdate();
                    Thread.Sleep(1);
                }
                a.Dispose();
                GC.Collect();
                chatFormSettings[index] = ChatFormSettings.Default();
                radios[index].Invoke((MethodInvoker)(() => radios[index].Text = "Empty"));
                if (radios[index].Checked)
                    button1.Invoke((MethodInvoker)(() => button1.Enabled = true));
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            textBox1.Text = "";
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            hasClosed = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            fontDialog1.ShowDialog();
            chatFormSettings[selectedIndex].Font = fontDialog1.Font;
            Fontlabel.Text = fontDialog1.Font.Name + ", " + fontDialog1.Font.Size;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            chatFormSettings[selectedIndex].BackgroundColor = colorDialog1.Color;
            BackgroundColorBox.Text = cc.ConvertToString(colorDialog1.Color);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            colorDialog2.ShowDialog();
            chatFormSettings[selectedIndex].ForegroundColor = colorDialog2.Color;
            ForegroundColorBox.Text = cc.ConvertToString(colorDialog2.Color);
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            int tIndex = int.Parse(((RadioButton)sender).Name.Substring(11));
            selectedIndex = tIndex - 1;
            if (chatFormSettings[selectedIndex].Current != null)
                button1.Enabled = false;
            else
                button1.Enabled = true;
            ForegroundColorBox.Text = cc.ConvertToString(chatFormSettings[selectedIndex].ForegroundColor);
            BackgroundColorBox.Text = cc.ConvertToString(chatFormSettings[selectedIndex].BackgroundColor);
            Emotespacing.Value = chatFormSettings[selectedIndex].EmoteSpacing;
            AnimationsCheckBox.Checked = chatFormSettings[selectedIndex].Animations;
            Fontlabel.Text = chatFormSettings[selectedIndex].Font.Name + ", " + chatFormSettings[selectedIndex].Font.Size;
            checkBox1.Checked = chatFormSettings[selectedIndex].Splitter;
            comboBox1.SelectedIndex = chatFormSettings[selectedIndex].ChatMode.currentIndex;
            numericUpDown1.Value = chatFormSettings[selectedIndex].PanelBorder;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            chatFormSettings[selectedIndex].Animations = AnimationsCheckBox.Checked;
        }

        private void Emotespacing_ValueChanged(object sender, EventArgs e)
        {
            chatFormSettings[selectedIndex].EmoteSpacing = (int)Emotespacing.Value;
        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {
            chatFormSettings[selectedIndex].Splitter = checkBox1.Checked;
        }

        private void generalToolStripMenuItem_Click(object sender, EventArgs e2)
        {
            SettingsForm a = new SettingsForm();
            a.saved += (o, e) =>
            {
                generalSettings = TwitchSettings.Interpret(JsonConvert.DeserializeObject<dynamic>(File.ReadAllText("settings.json")));
            };
            a.Show();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            chatFormSettings[selectedIndex].PanelBorder = (int)numericUpDown1.Value;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            chatFormSettings[selectedIndex].ChatMode.currentIndex = comboBox1.SelectedIndex;
        }
    }
}
