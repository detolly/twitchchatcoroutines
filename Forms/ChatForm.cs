using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Sockets;
using System.Windows.Forms;
using CoroutineSystem;
using System.IO;
using System;
using Newtonsoft.Json.Linq;
using System.Net;

using TwitchChatCoroutines.ClassesAndStructs;
using TwitchChatCoroutines.Forms;
using TwitchChatCoroutines.Controls;
using static TwitchChatCoroutines.ClassesAndStructs.HelperFunctions;
using System.Linq;

namespace TwitchChatCoroutines
{
    public partial class ChatForm : Form
    {
        #region Declarations
        private CoroutineManager coroutineManager = new CoroutineManager();

        private string botUsername;
        private string oauth;
        private string channelToJoin = "";
        private bool initialized = false;

        private int panelBorder = 15;

        private ColorConverter cc = new ColorConverter();

        public static string client_id = "570bj9vd1lakwt3myr8mrhg05ia5u9";

        Queue<TwitchMessage> messagesToBeAdded = new Queue<TwitchMessage>();
        private Font font;
        private int pixelsToMove;
        private Color backColor;
        private Color textColor;

        //private SortedList<string, Image> cachedTwitchEmotes = new SortedList<string, Image>();
        private SortedList<string, Image> cachedBTTVEmotes = new SortedList<string, Image>();
        private SortedList<string, Image> cachedFFZEmotes = new SortedList<string, Image>();
        private SortedList<string, Badge> cachedBadges = new SortedList<string, Badge>();

        private Image splitter = Properties.Resources.splitter2;

        private int emoteSpacing = 0;

        public static Random r = new Random();
        private Size lastSize;

        private List<MessageControl> currentChatMessages = new List<MessageControl>();
        private List<TwitchMessage> allTwitchMessages = new List<TwitchMessage>();

        public bool hasClosed = false;

        private TcpClient twitchClient = new TcpClient();
        private StreamReader reader;
        private StreamWriter writer;

        private bool authenticated = false;

        private dynamic bttvEmotesJson;
        private dynamic bttvChannelEmotesJson;
        private dynamic FFZEmotesJson;
        private dynamic FFZChannelEmotesJson;

        private string[] headers;

        private dynamic badgeJson;
        private dynamic channelBadgeJson;
        private dynamic channelInformationJson;

        private bool useFFZ = true;
        private bool useBTTV = true;
        private bool useEmoji = true;
        private int border = 5;

        private bool doAnimations = false;
        private bool doSplitter = false;

        private ChatFormSettings chatFormSettings;

        private string channelId;
        private Animation enterChatAnimation;


        private WebClient client;
        #endregion

        #region Enumerators
        IEnumerator Init()
        {
            ResizeBegin += ChatForm_ResizeStart;
            ResizeEnd += ChatForm_ResizeEnd;
            Resize += ChatForm_Resize;

            int h = SystemInformation.PrimaryMonitorSize.Height - 150;
            Height = h;

            if ((ChatModes)chatFormSettings.ChatMode.currentIndex == ChatModes.ChatUser)
            {
                panel1.Visible = true;
                panel1.BackColor = chatFormSettings.BackgroundColor;
                panel1.ForeColor = chatFormSettings.ForegroundColor;

                panel1.Location = new Point(Width / 2 - panel1.Size.Width / 2, Height);
                panel1.Anchor = AnchorStyles.Left & AnchorStyles.Right & AnchorStyles.Top;
                panel1.BringToFront();

                richTextBox1.Visible = true;
                richTextBox1.EnableAutoDragDrop = true;
                richTextBox1.KeyUp += (o, e) =>
                {
                    if (e.KeyData == Keys.Enter)
                        sendCurrentMessageToChat();
                };

                comboBox1.Items.Clear();
                foreach (string s in Authentication.GetLogins())
                {
                    comboBox1.Items.Add(s);
                }
                if (comboBox1.Items.Count > 0)
                    comboBox1.SelectedIndex = 0;
                coroutineManager.StartLateCoroutine(enterLoginPanel(panel1));
            }
            else if ((ChatModes)chatFormSettings.ChatMode.currentIndex == ChatModes.Anonymous)
            {
                Controls.Remove(panel1);
            }
            //Misc Coloring Stuff
            backColor = chatFormSettings.BackgroundColor;
            textColor = chatFormSettings.ForegroundColor;
            // Login Panel Logic
            ChangeInformationalLabel("Starting...");
            label2.ForeColor = textColor;

            //Create Directories
            ChangeInformationalLabel("Creating Directories...");
            yield return new WaitForMilliseconds(1);
            Directory.CreateDirectory("./emotes/BetterTTV");
            Directory.CreateDirectory("./emotes/FFZ");
            Directory.CreateDirectory("./emotes/Twitch");
            Directory.CreateDirectory("./emotes/Badges");


            //Apply settings
            ChangeInformationalLabel("Applying settings...");
            yield return new WaitForMilliseconds(1);
            channelToJoin = chatFormSettings.Channel;

            ChangeInformationalLabel("Calculating Animations");
            yield return new WaitForMilliseconds(1);
            AddEnterLineAnimation();

            headers = new string[] {
                    "Client-ID: " + client_id
            };
            font = chatFormSettings.Font;
            doAnimations = chatFormSettings.Animations;
            emoteSpacing = chatFormSettings.EmoteSpacing;
            chatFormSettings.Changed += ChangedEvent;

            cachedBadges = new SortedList<string, Badge>();
            BackColor = backColor;
            client = new WebClient();
            //Download Badges
            ChangeInformationalLabel("Downloading Twitch Badges Information...");
            yield return new WaitForMilliseconds(1);
            badgeJson = JsonGet("https://badges.twitch.tv/v1/badges/global/display", headers).badge_sets;

            //Download BTTV Global Emotes Information
            ChangeInformationalLabel("Download BTTV Global Emotes Information...");
            yield return new WaitForMilliseconds(1);
            try
            {
                bttvEmotesJson = JsonGet("https://api.betterttv.net/2/emotes").emotes;
            }
            catch
            {
                useBTTV = false;
            }
            //Download BTTV Channel Enabled Emotes Information
            ChangeInformationalLabel("Download BTTV Channel Enabled Emotes Information...");
            yield return new WaitForMilliseconds(1);
            try
            {
                bttvChannelEmotesJson = JsonGet("https://api.betterttv.net/2/channels/" + channelToJoin).emotes;
            }
            catch
            {
                useBTTV = false;
            }
            //Download FFZ Emotes Information
            //Downloading FFZ Channel Enabled Emotes Information
            ChangeInformationalLabel("Downloading FFZ Channel Enabled Emotes Information...");
            yield return new WaitForMilliseconds(1);
            try
            {
                FFZChannelEmotesJson = JsonGet("https://api.frankerfacez.com/v1/room/" + channelToJoin);
                FFZChannelEmotesJson = FFZChannelEmotesJson.sets[(string)(FFZChannelEmotesJson.room.set)].emoticons;
            }
            catch
            {
                useFFZ = false;
            }
            //Downloading FFZ Global Emotes Information
            ChangeInformationalLabel("Downloading FFZ Global Emotes Information");
            yield return new WaitForMilliseconds(1);
            try
            {
                FFZEmotesJson = JsonGet("https://api.frankerfacez.com/v1/set/global");
            }
            catch
            {
                useFFZ = false;
            }
            //Download channel information
            channelInformationJson = JsonGet("https://api.twitch.tv/helix/users?login=" + channelToJoin, headers);
            channelId = channelInformationJson.data[0].id;
            //Download channel specific badge information
            channelBadgeJson = JsonGet("https://badges.twitch.tv/v1/badges/channels/" + channelId + "/display", headers);

            if (useBTTV)
            {
                //Downloading BTTV Global Emotes
                ChangeInformationalLabel("Downloading BTTV Global Emotes...");
                yield return new WaitForMilliseconds(1);
                var temporary = JArray.FromObject(bttvEmotesJson);
                foreach (var entry in temporary)
                {
                    string channel = entry.channel;
                    string emote = entry.id;
                    string code = entry.code;
                    string imageType = entry.imageType;
                    string path = "./emotes/BetterTTV/BTTV" + emote + "." + imageType;
                    ChangeInformationalLabel("Downloading BTTV Global Emotes: \"" + code + "\"...");
                    yield return new WaitForMilliseconds(1);
                    if (!File.Exists(path))
                        client.DownloadFile(new Uri("http://cdn.betterttv.net/emote/" + emote + "/1x"), path);
                    Image image = Image.FromFile(path);
                    try
                    {
                        cachedBTTVEmotes.Add(code, image);
                    }
                    catch { }
                }
            }
            if (useBTTV)
            {
                //Downloading BTTV Channel specific emotes
                ChangeInformationalLabel("Downloading BTTV Channel specific emotes...");
                yield return new WaitForMilliseconds(1);
                var temporary2 = JArray.FromObject(bttvChannelEmotesJson);
                foreach (var entry in temporary2)
                {
                    string channel = entry.channel;
                    string emote = entry.id;
                    string code = entry.code;
                    string imageType = entry.imageType;
                    string path = "./emotes/BetterTTV/BTTV" + emote + "." + imageType;
                    ChangeInformationalLabel("Downloading BTTV Channel specific emote: \"" + code + "\"...");
                    yield return new WaitForMilliseconds(1);
                    if (!File.Exists(path))
                        client.DownloadFile(new Uri("http://cdn.betterttv.net/emote/" + emote + "/1x"), path);
                    Image image = Image.FromFile(path);
                    try
                    {
                        cachedBTTVEmotes.Add(code, image);
                    }
                    catch { }
                }
            }
            if (useFFZ)
            {
                //Downloading FFZ Channel Enabled Emotes
                ChangeInformationalLabel("Downloading FFZ Channel Enabled Emotes...");
                yield return new WaitForMilliseconds(1);
                var temporary3 = JArray.FromObject(FFZChannelEmotesJson);
                foreach (var entry in temporary3)
                {
                    string code = entry.name;
                    string url = "http://cdn.frankerfacez.com/emoticon/" + entry.id + "/1";
                    string path = "./emotes/FFZ/FFZ" + entry.id + ".png";
                    ChangeInformationalLabel("Downloading FFZ Channel Enabled Emote: \"" + code + "\"...");
                    yield return new WaitForMilliseconds(1);
                    if (!File.Exists(path))
                        client.DownloadFile(new Uri(url), path);
                    Image image = Image.FromFile(path);
                    try
                    {
                        cachedFFZEmotes.Add(code, image);
                    }
                    catch { }
                }
                //Downloading FFZ Global Emotes
                ChangeInformationalLabel("Downloading FFZ Global Emotes");
                yield return new WaitForMilliseconds(1);
                var temporary4 = JArray.FromObject(FFZEmotesJson.sets["3"].emoticons);
                foreach (var entry in temporary4)
                {
                    string code = entry.name;
                    string url = "http://cdn.frankerfacez.com/emoticon/" + entry.id + "/1";
                    string path = "./emotes/FFZ/FFZ" + entry.id + ".png";
                    ChangeInformationalLabel("Downloading FFZ Global Emote: \"" + code + "\"...");
                    yield return new WaitForMilliseconds(1);
                    if (!File.Exists(path))
                        client.DownloadFile(new Uri(url), path);
                    Image image = Image.FromFile(path);
                    try
                    {
                        cachedFFZEmotes.Add(code, image);
                    }
                    catch { }
                }
            }
            var strings = badgeJson.ToObject<Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>>>();
            //Downloading global badges
            ChangeInformationalLabel("Downloading Twitch Global Badges...");
            yield return new WaitForMilliseconds(1);
            foreach (var entry in strings)
            {
                List<string> keys = new List<string>(entry.Value["versions"].Keys);
                for (int i = 0; i < entry.Value["versions"].Keys.Count; i++)
                {
                    string url = entry.Value["versions"][keys[i]]["image_url_1x"];
                    string path = "emotes/Badges/" + entry.Key + "_" + keys[i] + ".png";
                    Badge b;
                    if (cachedBadges.ContainsKey(entry.Key))
                        b = cachedBadges[entry.Key];
                    else
                    {
                        b = new Badge()
                        {
                            versions = new Dictionary<string, BadgeVersion>()
                        };
                        cachedBadges.Add(entry.Key, b);
                    }
                    ChangeInformationalLabel("Downloading Twitch Global Badge: " + entry.Value["versions"][keys[i]]["description"]);
                    yield return new WaitForMilliseconds(1);
                    if (!File.Exists(path))
                        client.DownloadFile(url, path);
                    Image img = default(Image);
                    try
                    {
                        img = Image.FromFile(path);
                    }
                    catch { continue; }
                    if (!b.versions.ContainsKey(keys[i]))
                    {
                        BadgeVersion v = new BadgeVersion()
                        {
                            url_1x = entry.Value["versions"][keys[i]]["image_url_1x"],
                            url_2x = entry.Value["versions"][keys[i]]["image_url_2x"],
                            url_4x = entry.Value["versions"][keys[i]]["image_url_4x"],
                            image = img,
                            description = entry.Value["versions"][keys[i]]["description"],
                            title = entry.Value["versions"][keys[i]]["description"]
                        };
                        b.versions.Add(keys[i], v);
                    }
                }
            }
            Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>> strings2 = channelBadgeJson.badge_sets.ToObject<Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>>>();
            //Downloading channel badges
            ChangeInformationalLabel("Downloading Twitch Channel Specific Badges...");
            yield return new WaitForMilliseconds(1);
            foreach (var entry in strings2)
            {
                List<string> keys = new List<string>(entry.Value["versions"].Keys);
                Dictionary<string, string> dict = new Dictionary<string, string>();
                for (int i = 0; i < entry.Value["versions"].Keys.Count; i++)
                {
                    string url = entry.Value["versions"][keys[i]]["image_url_1x"];
                    string path = "emotes/Badges/" + channelToJoin + "_" + entry.Key + "_" + keys[i] + ".png";
                    ChangeInformationalLabel("Downloading Twitch Channel Specific Badge: \"" + entry.Value["versions"][keys[i]]["description"] + "\"...");
                    yield return new WaitForMilliseconds(1);
                    if (!File.Exists(path))
                        client.DownloadFile(url, path);
                    Image img = default(Image);
                    try
                    {
                        img = Image.FromFile(path);
                    }
                    catch { continue; }
                    Badge b;
                    if (cachedBadges.ContainsKey(entry.Key))
                    {
                        b = cachedBadges[entry.Key];
                    }
                    else
                    {
                        b = new Badge()
                        {
                            versions = new Dictionary<string, BadgeVersion>()
                        };
                        cachedBadges.Add(entry.Key, b);
                    }
                    BadgeVersion v = new BadgeVersion()
                    {
                        url_1x = entry.Value["versions"][keys[i]]["image_url_1x"],
                        url_2x = entry.Value["versions"][keys[i]]["image_url_2x"],
                        url_4x = entry.Value["versions"][keys[i]]["image_url_4x"],
                        image = img,
                        description = entry.Value["versions"][keys[i]]["description"],
                        title = entry.Value["versions"][keys[i]]["title"]
                    };
                    b.versions[keys[i]] = v;
                }
            }
            ChangeInformationalLabel("Initializing Emoji... (this might take a while)");
            yield return new WaitForMilliseconds(1);
            Emojis.codeToEmoji.ToString();
            ChangeInformationalLabel("Done!");
            yield return new WaitForSeconds(1);

            //Dispose of the Update label
            label2.Dispose();
            Controls.Remove(label2);
            initialized = true;
            yield break;
        }

        IEnumerator enterLoginPanel(Panel p)
        {
            int v = 244;
            while (p.Location.Y + p.Size.Height / 2 > v)
            {
                if (p.Location.Y - (int)(1 + Math.Abs((p.Location.Y - v) * 0.1f)) > v)
                    p.Location = new Point(p.Location.X, p.Location.Y - (int)(1 + Math.Abs((p.Location.Y - v) * 0.1f)));
                else
                {
                    p.Location = new Point(p.Location.X, v);
                    break;
                }
                yield return new WaitForMilliseconds(10);
            }
            yield break;
        }

        IEnumerator removePanel(Panel p)
        {
            int v = 0 - p.Size.Height;
            while (p.Location.Y > v)
            {
                p.Location = new Point(p.Location.X, p.Location.Y - (int)(1 + Math.Abs((p.Location.Y + Height / 2) * 0.05f)));
                yield return new WaitForMilliseconds(10);
            }
            authenticated = true;
            Controls.Remove(p);
            yield break;
        }

        IEnumerator enterChatLine(TwitchUserMessage greetings)
        {
            if (doAnimations)
                for (int i = 0; i < enterChatAnimation.StepCount - 1; i++)
                {
                    greetings.Location = new Point(enterChatAnimation.GetNext(i), greetings.Location.Y);
                    yield return new WaitForMilliseconds(5);
                }
            else
                greetings.Location = new Point(0, greetings.Location.Y);
            yield break;
        }

        IEnumerator moveLabels(MessageControl messageToBeEntered)
        {
            List<MessageControl> toRemove = new List<MessageControl>();

            if (messageToBeEntered is TwitchUserMessage userMessage)
                coroutineManager.StartLateCoroutine(enterChatLine(userMessage));
            pixelsToMove = messageToBeEntered.Size.Height + panelBorder;

            for (int i = 0; i < currentChatMessages.Count; i++)
            {
                if (currentChatMessages[i] == messageToBeEntered) continue;
                int border = -currentChatMessages[i].Size.Height - 5;
                pixelsToMove = messageToBeEntered.Size.Height;
                currentChatMessages[i].Location = new Point(currentChatMessages[i].Location.X, currentChatMessages[i].Location.Y - pixelsToMove);
                if (currentChatMessages[i].Location.Y < border)
                    toRemove.Add(currentChatMessages[i]);
            }

            if (toRemove.Count > 0 && IsHandleCreated)
                toRemove[0].Invoke((MethodInvoker)(() =>
                {
                    for (int i = 0; i < toRemove.Count; i++)
                    {
                        currentChatMessages.Remove(toRemove[i]);
                        Controls.Remove(toRemove[i]);
                        toRemove[i].Dispose();
                    }
                }));
            yield break;
        }
        #endregion

        #region Init
        public ChatForm(ChatFormSettings chatFormSettings)
        {
            InitializeComponent();
            this.chatFormSettings = chatFormSettings;
            coroutineManager.Init();
            Text = chatFormSettings.Channel;
            coroutineManager.StartCoroutine(Init());
        }
        #endregion

        #region IRC
        public void CustomUpdate()
        {
            coroutineManager.Interval();
            if (!initialized) return;
            if (!twitchClient.Connected)
            {
                Connect();
            }
            else if (twitchClient.Available > 0)
            {
                string rawLine = reader.ReadLine();
                if (rawLine == "PING :tmi.twitch.tv")
                {
                    SendRawMessage("PONG :tmi.twitch.tv");
                }
                else if (rawLine.Contains("PRIVMSG"))
                {
                    TwitchMessage user = TwitchMessage.GetTwitchMessage(rawLine);
                    string username = user.display_name; //GetUsername(rawLine);
                    string extractedMessage = user.message; //GetExtractedMessage(rawLine);
                    bool isAction = false;
                    if (extractedMessage.StartsWith("\u0001"))
                    {
                        extractedMessage = extractedMessage.Replace("\u0001", "");
                        extractedMessage = extractedMessage.ReplaceFirst("ACTION ", "");
                        isAction = true;
                    }
                    user.message = extractedMessage;
                    messagesToBeAdded.Enqueue(user);
                }
                else if (rawLine.Contains("CLEARCHAT"))
                {
                    int start = rawLine.IndexOf(":", rawLine.IndexOf("CLEARCHAT")) + 1;
                    int stop = rawLine.IndexOf(" ", start);
                    string user = rawLine.Substring(start, rawLine.Length - start);
                    Font f = font;
                    f = new Font(f, FontStyle.Strikeout);
                    foreach (TwitchUserMessage m in currentChatMessages)
                    {
                        if (m.twitchMessage.display_name.ToLower() == user.ToLower())
                            m.Font = f;
                    }
                }
            }
            if (messagesToBeAdded.Count > 0)
            {
                ProcessMessage(messagesToBeAdded.Dequeue());
            }
        }

        private void SendRawMessage(string inp)
        {
            writer.WriteLine(inp);
        }

        private void SendMessage(string message)
        {
            writer.WriteLine($":{botUsername}!{botUsername}@{botUsername}.tmi.twitch.tv PRIVMSG #{channelToJoin} :{message}");
        }

        void Connect()
        {
            if (authenticated || (ChatModes)chatFormSettings.ChatMode.currentIndex == ChatModes.Anonymous)
            {
                twitchClient.Connect("irc.chat.twitch.tv", 6667);
                reader = new StreamReader(twitchClient.GetStream());
                writer = new StreamWriter(twitchClient.GetStream())
                {
                    AutoFlush = true
                };
                var chatMod = (ChatModes)chatFormSettings.ChatMode.currentIndex;
                writer.WriteLine("CAP REQ :twitch.tv/tags");
                writer.WriteLine("CAP REQ :twitch.tv/commands");
                if (chatMod == ChatModes.Anonymous)
                {
                    botUsername = "justinfan1";
                    writer.WriteLine("NICK " + botUsername.ToLower());
                }
                else if (chatMod == ChatModes.ChatUser)
                {
                    //Todo: Move to constructor
                    writer.WriteLine("PASS oauth:" + oauth);
                    writer.WriteLine("NICK " + botUsername.ToLower());
                }

                writer.WriteLine("JOIN #" + channelToJoin.ToLower());
            }
        }
        #endregion

        #region Events
        public void ChangedEvent(object o, EventArgs e)
        {
            backColor = chatFormSettings.BackgroundColor;
            BackColor = backColor;
            textColor = chatFormSettings.ForegroundColor;
            font = chatFormSettings.Font;
            doAnimations = chatFormSettings.Animations;
            panelBorder = chatFormSettings.PanelBorder;
            emoteSpacing = chatFormSettings.EmoteSpacing;
            
            if (FormBorderStyle != chatFormSettings.BorderStyle)
                if (IsHandleCreated)
                    Invoke((MethodInvoker)(() =>
                    {
                        FormBorderStyle = chatFormSettings.BorderStyle;
                    }));
            if (IsHandleCreated)
                Invoke((MethodInvoker)(() =>
                {
                    if (doSplitter != chatFormSettings.Splitter)
                    {
                        doSplitter = chatFormSettings.Splitter;
                    }
                    foreach (TwitchUserMessage m in currentChatMessages)
                    {
                        m.DoSplitter = doSplitter;
                        m.ForeColor = textColor;
                        m.BackColor = backColor;
                        m.PanelBorder = panelBorder;
                        if (m.EmoteSpacing != emoteSpacing)
                            m.Invalidate();
                        if (m.Font != font)
                            m.Invalidate();
                        m.Font = font;
                        m.EmoteSpacing = emoteSpacing;
                        m.DrawContent(m.CreateGraphics());
                    }
                }));
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            hasClosed = true;
        }

        private void ChatForm_ResizeStart(object sender, EventArgs e)
        {
            lastSize = Size;
        }

        private void ChatForm_ResizeEnd(object sender, EventArgs e)
        {
            doChangeLines();
        }

        private void ChatForm_Resize(object s, EventArgs e)
        {
            for (int i = currentChatMessages.Count - 1; i >= 0; i--)
            {
                var m = currentChatMessages[i];
                if (m is TwitchUserMessage me)
                {
                    me.DesiredWidth = Width - 2 * border;
                    me.CalculateTextAndEmotes();
                }
            }
        }

        void doChangeLines()
        {
            if (currentChatMessages.Count == 0) return;
            int totalDiff = 0;
            Size difference = new Size(lastSize.Width - Size.Width, lastSize.Height - Size.Height);
            var differences = new Size[currentChatMessages.Count];
            for (int i = currentChatMessages.Count - 1; i >= 0; i--)
            {
                var m = currentChatMessages[i];
                Size oldSize = m.Size;
                differences[i] = oldSize;
            }
            Application.DoEvents();
            for (int i = differences.Length - 1; i >= 0; i--)
            {
                var m = currentChatMessages[i];
                int heightDifference = m.Size.Height - differences[i].Height;
                totalDiff += heightDifference;
                for (int x = i; x >= 0; x--)
                {
                    currentChatMessages[x].Location = new Point(currentChatMessages[x].Location.X, currentChatMessages[x].Location.Y - heightDifference);
                }
                m.Location = new Point(m.Location.X, m.Location.Y - difference.Height);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WebForm form = new WebForm();
            form.Authenticated += (o, auth) =>
            {
                Authentication.Add(auth);
                var g = Authentication.GetLogins();
                comboBox1.Items.Clear();
                foreach (string item in g)
                    comboBox1.Items.Add(item);
                comboBox1.SelectedIndex = 0;
                form.Dispose();
            };
            form.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            botUsername = comboBox1.SelectedItem.ToString();
            oauth = Authentication.GetOauth(botUsername);
            coroutineManager.StartCoroutine(removePanel(panel1));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var username = (string)comboBox1.SelectedItem;
            Authentication.Remove(username);
            comboBox1.Items.Remove(comboBox1.SelectedItem);
            comboBox1.SelectedItem = "";
        }
        #endregion

        #region Visual
        private void sendCurrentMessageToChat()
        {
            string textToSend = richTextBox1.Text;
            richTextBox1.Clear();
            writer.WriteLine("PRIVMSG #" + channelToJoin + " :" + textToSend);
            TwitchMessage m = new TwitchMessage();
            m.username = botUsername;
            m.message = textToSend;
            m.color = "#FFFFFF";
            SentUserMessage m2 = new SentUserMessage(m, panelBorder, border, Width);
            Controls.Add(m2);
            currentChatMessages.Add(m2);
            Application.DoEvents();
            m2.Location = new Point(-m2.Width, Height - m2.Size.Height - 50 - (richTextBox1.Visible ? richTextBox1.Size.Height : 0));
            coroutineManager.StartCoroutine(moveLabels(m2));

        }

        private void ChangeInformationalLabel(string newText)
        {
            label2.Text = newText;
            label2.Location = new Point(Size.Width / 2 - label2.Size.Width / 2, label2.Location.Y);
        }

        private void ProcessMessage(TwitchMessage twitchMessage)
        {
            // http://static-cdn.jtvnw.net/emoticons/v1/:<emote ID>/1.0
            //<emote ID>:<first index>-<last index>,<another first index>-<another last index>/<another emote ID>:<first index>-<last index>...

            SortedList<int, ImageAndInts> emoteBoxes = new SortedList<int, ImageAndInts>();
            var badges = new List<Image>();

            string[] array = twitchMessage.message.Split(' ');
            int lastLoc = 0;
            if (useFFZ || useBTTV || useEmoji)
                foreach (string a in array)
                {
                    if (cachedBTTVEmotes.ContainsKey(a))
                    {
                        int start = twitchMessage.message.IndexOf(a, lastLoc);
                        int stop = start + a.Length - 1;
                        Tuple<int, int> ints = new Tuple<int, int>(start, stop);
                        lastLoc = stop;

                        ImageAndInts iss = new ImageAndInts
                        {
                            img = cachedBTTVEmotes[a],
                            ints = ints
                        };
                        emoteBoxes.Add(start, iss);
                    }
                    else if (cachedFFZEmotes.ContainsKey(a))
                    {
                        int start = twitchMessage.message.IndexOf(a, lastLoc);
                        int stop = start + a.Length - 1;
                        Tuple<int, int> ints = new Tuple<int, int>(start, stop);
                        lastLoc = stop;

                        ImageAndInts iss = new ImageAndInts
                        {
                            img = cachedFFZEmotes[a],
                            ints = ints
                        };
                        emoteBoxes.Add(start, iss);
                    }
                    else if (useEmoji)
                    {
                        List<string> emojis = new List<string>();
                        string current = "";
                        for (int i = 0; i < a.Length; i++)
                        {
                            char c = a[i];
                            if (!Emojis.codeToEmoji.ContainsKey(current))
                                if (c > 255)
                                {
                                    current += c;
                                    continue;
                                }
                            if (current.Length > 0)
                            {
                                i--;
                                emojis.Add(current);
                            }
                            current = "";
                        }
                        if (current.Length > 0)
                            emojis.Add(current);
                        foreach (string s in emojis)
                            if (Emojis.codeToEmoji.ContainsKey(s))
                            {
                                int start = twitchMessage.message.IndexOf(s, lastLoc);
                                int stop = start + s.Length - 1;
                                Tuple<int, int> ints = new Tuple<int, int>(start, stop);
                                lastLoc = stop;
                                ImageAndInts iss = new ImageAndInts
                                {
                                    img = Emojis.codeToEmoji[s],
                                    ints = ints,
                                    preferredSize = new Size(18, 18)
                                };
                                try
                                {
                                    emoteBoxes.Add(start, iss);
                                }
                                catch { }
                            }
                    }
                }
            string[] tBadges = twitchMessage.badges.Split(',');
            if (tBadges[0] != null)
            {
                foreach (string s in tBadges)
                {
                    string[] parts = s.Split('/');
                    if (cachedBadges.ContainsKey(parts[0]))
                    {
                        badges.Add(cachedBadges[parts[0]].versions[parts[1]].image);
                    }
                }
            }
            string[] emotes = twitchMessage.emotes.Split('/');
            if (emotes[0] != "")
            {
                foreach (string s in emotes)
                {
                    int start = s.IndexOf(":");
                    string onePart = s.Substring(start + 1);
                    List<Tuple<int, int>> ints = new List<Tuple<int, int>>();
                    string[] indexes = onePart.Split(',');
                    foreach (string a in indexes)
                    {
                        string[] e = a.Split('-');
                        Tuple<int, int> t = new Tuple<int, int>(int.Parse(e[0]), int.Parse(e[1]));
                        ints.Add(t);
                    }

                    for (int i = 0; i < ints.Count; i++)
                    {
                        int firstIndex = ints[i].Item1;
                        int secondIndex = ints[i].Item2;
                        int length = secondIndex - firstIndex + 1;
                        string code = twitchMessage.message.Substring(firstIndex, firstIndex + length > twitchMessage.message.Length - 1 ? twitchMessage.message.Length - firstIndex : length);
                        string theId = s.Substring(0, start);
                        string theUrl = "http://static-cdn.jtvnw.net/emoticons/v1/" + theId + "/1.0";
                        string path = "./emotes/Twitch/Twitch" + theId + ".png";
                        if (!File.Exists(path))
                        {
                            client.DownloadFile(theUrl, path);
                        }
                        Image image = default(Image);
                        try
                        {
                            image = Image.FromFile(path);
                        }
                        catch { }
                        ImageAndInts iss = new ImageAndInts
                        {
                            img = image,
                            ints = ints[i],
                            //tooltip = new Controls.ToolTip("Twitch Emote: " + code, image)
                        };
                        try
                        {
                            emoteBoxes.Add(firstIndex, iss);
                        }
                        catch { }
                    }
                }
            }
            TwitchUserMessage m = new TwitchUserMessage(twitchMessage, badges, emoteBoxes, font, doSplitter, textColor, backColor, Width - 2 * border - (vScrollBar1.Visible ? vScrollBar1.Width : 0), panelBorder / 2, emoteSpacing);
            Controls.Add(m);
            currentChatMessages.Add(m);
            m.Location = new Point(-m.Width, Height - m.Size.Height - 50 - (richTextBox1.Visible ? richTextBox1.Size.Height : 0));
            Application.DoEvents();
            coroutineManager.StartCoroutine(moveLabels(m));
        }

        public static string getRandomColor()
        {
            string[] colors = new string[]
            {
                "#00FDFD",
                "#ADEFFF",
                "#66FFB3",
                "#7FFF7F",
                "#91FF00",
                "#C8F500",
            };
            return colors[r.Next(colors.Length)];
        }
        #endregion

        #region Misc
        void AddEnterLineAnimation()
        {
            int comparison = -Width;
            Animation animation = new Animation();
            enterChatAnimation = animation;
            while (comparison < 0)
            {
                if ((comparison + (1 + Math.Abs(comparison * 0.1f))) > 0)
                {
                    comparison = 0;
                    continue;
                }
                else
                    comparison = (int)(comparison + (1 + Math.Abs(comparison * 0.1f)));
                animation.AddStep(comparison);
            }
            animation.Finish();
        }

        public bool IsUnicodeCharacter(char c)
        {
            const int MaxAnsiCode = 255;

            return c > MaxAnsiCode;
        }

        int getOffset(int index, Dictionary<int, int> start)
        {
            for (int i = start.Count - 1; i >= 0; i--)
            {
                var k = new KeyValuePair<int, int>(start.Keys.ElementAt(i), start.Values.ElementAt(i));
                if (index > k.Key)
                    return k.Value;

            }
            return 0;
        }
        #endregion
    }
}
