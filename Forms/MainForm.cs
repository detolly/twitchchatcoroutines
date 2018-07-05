using Newtonsoft.Json;
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
        private string version = "v0.3-alpha-d";

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
                } catch
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
                chatFormSettings[i] = Default();
            }
        }

        private void CheckGeneralSettings()
        {
            if (!System.IO.File.Exists("settings.json"))
            {
                var a = JsonConvert.DeserializeObject<dynamic>(@"
{general: {
twitchEmoteCaching: true,
bttvEmoteCaching: true,
ffzEmoteCaching: true,
emotesCaching: true
}}");
                var o = JsonConvert.SerializeObject(a);
                System.IO.File.WriteAllText("settings.json", o);
                Thread.Sleep(50);
            }
        }

        ChatFormSettings Default()
        {
            ChatFormSettings settings = new ChatFormSettings();
            settings.ForegroundColor = (Color)cc.ConvertFromString("#FFFFFF");
            settings.BackgroundColor = (Color)cc.ConvertFromString("#111111");
            settings.Animations = false;
            settings.Font = defaultFont;
            settings.EmoteSpacing = 3;
            settings.PanelBorder = 15;
            settings.Channel = "forsen";
            settings.Animations = false;
            settings.Splitter = true;
            return settings;
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
                Stopwatch w = new Stopwatch();
                while (true)
                {
                    if (a.hasClosed)
                        break;
                    w.Start();
                    Application.DoEvents();
                    a.CustomUpdate();
                    w.Stop();
                    if (w.ElapsedMilliseconds > 2500)
                        a.Controls.Clear();
                    w.Reset();
                    Thread.Sleep(1);
                }
                a.Dispose();
                GC.Collect();
                chatFormSettings[index] = Default();
                radios[index].Invoke((MethodInvoker)(() => radios[index].Text = "Empty"));
                if (radios[index].Checked)
                    button1.Invoke((MethodInvoker)(() => button1.Enabled = true));
            });
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
                generalSettings = TwitchSettings.Interpret(JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText("settings.json")));
            };
            a.Show();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            chatFormSettings[selectedIndex].PanelBorder = (int)numericUpDown1.Value;
        }
    }
}
