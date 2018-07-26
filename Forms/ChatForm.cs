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

        Queue<MessageControl> messagesToBeAdded = new Queue<MessageControl>();
        private Font font;
        private int pixelsToMove;
        private Color outlineColor;
        private Color textColor;

        //private SortedList<string, Image> cachedTwitchEmotes = new SortedList<string, Image>();
        private SortedList<string, Image> cachedBTTVEmotes = new SortedList<string, Image>();
        private SortedList<string, Image> cachedFFZEmotes = new SortedList<string, Image>();
        private SortedList<string, Badge> cachedBadges = new SortedList<string, Badge>();

        private Image splitter = Properties.Resources.splitter2;

        private int emoteSpacing = 0;

        Random r = new Random();
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
            if ((ChatModes)chatFormSettings.ChatMode.currentIndex == ChatModes.ChatUser)
            {
                panel1.BackColor = chatFormSettings.BackgroundColor;
                panel1.ForeColor = chatFormSettings.ForegroundColor;

                int h = SystemInformation.PrimaryMonitorSize.Height - 150;
                Height = h;

                panel1.Location = new Point(Width / 2 - panel1.Size.Width / 2, Height);
                panel1.Anchor = AnchorStyles.Left & AnchorStyles.Right & AnchorStyles.Top;
                panel1.BringToFront();

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
            outlineColor = chatFormSettings.BackgroundColor;
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
            BackColor = outlineColor;
            client = new WebClient();
            //Download Badges
            ChangeInformationalLabel("Downloading Twitch Badges Information...");
            yield return new WaitForMilliseconds(1);
            badgeJson = JsonGet("https://badges.twitch.tv/v1/badges/global/display", headers).badge_sets;

            //Download BTTV Emotes Information
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
                ChangeInformationalLabel("Downloading BTT Channel specific emotes...");
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

        IEnumerator enterChatLine(MessageControl greetings)
        {
            //while (greetings.panel.Location.X < 0)
            //{
            //    if (!doAnimations)
            //        greetings.panel.Location = new Point(0, greetings.panel.Location.Y);
            //    else if (doAnimations)
            //    {
            //        if ((greetings.panel.Location.X + (1 + Math.Abs(greetings.panel.Location.X * 0.1f))) > 0)
            //        {
            //            greetings.panel.Location = new Point(0, greetings.panel.Location.Y);
            //            continue;
            //        }
            //        greetings.panel.Location = new Point((int)(greetings.panel.Location.X + (1 + Math.Abs(greetings.panel.Location.X * 0.1f))), greetings.panel.Location.Y);
            //    }
            //    yield return new WaitForMilliseconds(5);
            //}
            if (doAnimations)
                for (int i = 0; i < enterChatAnimation.StepCount - 1; i++)
                {
                    greetings.panel.Location = new Point(enterChatAnimation.GetNext(i), greetings.panel.Location.Y);
                    yield return new WaitForMilliseconds(5);
                }
            else
                greetings.panel.Location = new Point(0, greetings.panel.Location.Y);
            yield break;
        }

        IEnumerator moveLabels(MessageControl exclude)
        {
            List<MessageControl> toRemove = new List<MessageControl>();

            coroutineManager.StartLateCoroutine(enterChatLine(exclude));
            pixelsToMove = exclude.panel.Size.Height + panelBorder;

            for (int i = 0; i < currentChatMessages.Count; i++)
            {
                if (currentChatMessages[i].messages == exclude.messages) continue;
                int border = -currentChatMessages[i].panel.Size.Height - 5;
                pixelsToMove = exclude.panel.Size.Height;
                currentChatMessages[i].panel.Location = new Point(currentChatMessages[i].panel.Location.X, currentChatMessages[i].panel.Location.Y - pixelsToMove);
                if (currentChatMessages[i].panel.Location.Y < border)
                    toRemove.Add(currentChatMessages[i]);
            }

            if (toRemove.Count > 0 && IsHandleCreated)
                toRemove[0].panel.Invoke((MethodInvoker)(() =>
                {
                    for (int i = 0; i < toRemove.Count; i++)
                    {
                        currentChatMessages.Remove(toRemove[i]);
                        for (int x = 0; x < toRemove[i].panel.Controls.Count; x++)
                        {
                            toRemove[i].panel.Controls[x].Dispose();
                        }
                        for (int x = 0; x < toRemove[i].badges.Count; x++)
                        {
                            toRemove[i].badges[x].Dispose();
                        }
                        for (int x = 0; x < toRemove[i].tooltips.Count; x++)
                        {
                            toRemove[i].tooltips[x].Dispose();
                        }
                        Controls.Remove(toRemove[i].panel);
                        toRemove[i].panel.Dispose();
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
                    MessageControl m = new MessageControl
                    {
                        twitchMessage = user,
                        isAction = isAction,
                        badges = new List<PictureBox>()
                    };
                    messagesToBeAdded.Enqueue(m);

                    //#if DEBUG
                    //  if (channelToJoin.ToLower() == "kingkalus" && user.mod == 0)
                    //    SendMessage(".timeout " + user.display_name + " 1");
                    //#endif
                }
                else if (rawLine.Contains("CLEARCHAT"))
                {
                    int start = rawLine.IndexOf(":", rawLine.IndexOf("CLEARCHAT")) + 1;
                    int stop = rawLine.IndexOf(" ", start);
                    string user = rawLine.Substring(start, rawLine.Length - start);
                    Font f = font;
                    f = new Font(f, FontStyle.Strikeout);
                    foreach (MessageControl m in currentChatMessages)
                    {
                        if (m.twitchMessage.display_name.ToLower() == user.ToLower())
                            foreach (TwitchLabel l in m.messages)
                            {
                                l.Font = f;
                                l.ForeColor = Color.Gray;
                            }
                    }
                }
            }
            if (messagesToBeAdded.Count > 0)
            {
                MakeAndInsertLabel(messagesToBeAdded.Dequeue());
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
                writer.WriteLine("CAP REQ :twitch.tv/tags");
                writer.WriteLine("CAP REQ :twitch.tv/commands");
            }
        }
        #endregion

        #region Events
        public void ChangedEvent(object o, EventArgs e)
        {
            outlineColor = chatFormSettings.BackgroundColor;
            BackColor = outlineColor;
            textColor = chatFormSettings.ForegroundColor;
            font = chatFormSettings.Font;
            doAnimations = chatFormSettings.Animations;
            panelBorder = chatFormSettings.PanelBorder;
            if (doSplitter != chatFormSettings.Splitter)
            {
                doSplitter = chatFormSettings.Splitter;
                if (IsHandleCreated)
                    Invoke((MethodInvoker)(() =>
                    {
                        foreach (MessageControl m in currentChatMessages)
                            m.splitter.Invoke((MethodInvoker)(() =>
                            {
                                m.splitter.Visible = doSplitter;
                            }));
                    }));
            }
            if (IsHandleCreated)
                Invoke((MethodInvoker)(() =>
                {
                    FormBorderStyle = chatFormSettings.BorderStyle;
                }));
            emoteSpacing = chatFormSettings.EmoteSpacing;
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
            Size difference = new Size(lastSize.Width - Size.Width, lastSize.Height - Size.Height);
            foreach (MessageControl m in currentChatMessages)
            {
                m.panel.Location = new Point(m.panel.Location.X, m.panel.Location.Y - difference.Height);
            }
            foreach (MessageControl m in currentChatMessages)
            {
                m.panel.Size = new Size(Width, m.panel.Size.Height);
                m.splitter.Size = new Size(Width, m.splitter.Size.Height);
            }
            AddEnterLineAnimation();
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
        private void ChangeInformationalLabel(string newText)
        {
            label2.Text = newText;
            label2.Location = new Point(Size.Width / 2 - label2.Size.Width / 2, label2.Location.Y);
        }

        public void AddTooltip(Control box, Controls.ToolTip tip, Panel p, List<Controls.ToolTip> tips)
        {
            box.MouseEnter += (o, e) =>
            {
                if (!tip.IsCreated)
                {
                    Controls.Add(tip);
                    tip.BringToFront();
                    tip.IsCreated = true;
                }
                int spacing = TwitchChatCoroutines.Controls.ToolTip.spacing;
                tip.Location = new Point(Math.Max(tip.CustomParent.Location.X + tip.CustomParent.Size.Width / 2 - tip.Size.Width / 2 + spacing, spacing), p.Location.Y + box.Location.Y - tip.Size.Height);
                tip.Visible = true;
            };
            box.MouseLeave += (o, e) =>
            {
                tip.Visible = false;
            };
            tips.Add(tip);
        }

        private TwitchLabel MakeAndInsertLabel(MessageControl m)
        {
            // http://static-cdn.jtvnw.net/emoticons/v1/:<emote ID>/1.0
            //<emote ID>:<first index>-<last index>,<another first index>-<another last index>/<another emote ID>:<first index>-<last index>...
            SortedList<int, ImageAndInts> emoteBoxes = new SortedList<int, ImageAndInts>();
            List<TwitchLabel> labelsToAdd = new List<TwitchLabel>();
            List<Controls.ToolTip> tooltips = new List<Controls.ToolTip>();
            Panel p = new Panel();
            Controls.Add(p);
            string[] array = m.twitchMessage.message.Split(' ');
            int lastLoc = 0;
            if (useFFZ || useBTTV || useEmoji)
                foreach (string a in array)
                {
                    if (cachedBTTVEmotes.ContainsKey(a))
                    {
                        int start = m.twitchMessage.message.IndexOf(a, lastLoc);
                        int stop = start + a.Length - 1;
                        Tuple<int, int> ints = new Tuple<int, int>(start, stop);
                        lastLoc = stop;
                        PictureBox box = new PictureBox
                        {
                            Image = cachedBTTVEmotes[a],
                            SizeMode = PictureBoxSizeMode.AutoSize
                        };
                        Controls.ToolTip tip = new Controls.ToolTip(box)
                        {
                            BackColor = Color.Black,
                            ForeColor = Color.White,
                            Image = box.Image,
                            Text = "BetterTTV Emote: " + a,
                        };
                        AddTooltip(box, tip, p, tooltips);
                        ImageAndInts iss = new ImageAndInts
                        {
                            pb = box,
                            ints = ints
                        };
                        emoteBoxes.Add(start, iss);
                    }
                    else if (cachedFFZEmotes.ContainsKey(a))
                    {
                        int start = m.twitchMessage.message.IndexOf(a, lastLoc);
                        int stop = start + a.Length - 1;
                        Tuple<int, int> ints = new Tuple<int, int>(start, stop);
                        lastLoc = stop;
                        PictureBox box = new PictureBox
                        {
                            Image = cachedFFZEmotes[a],
                            SizeMode = PictureBoxSizeMode.AutoSize
                        };
                        Controls.ToolTip tip = new Controls.ToolTip(box)
                        {
                            BackColor = Color.Black,
                            ForeColor = Color.White,
                            Image = box.Image,
                            Text = "FrankerFaceZ Emote: " + a,
                        };
                        AddTooltip(box, tip, p, tooltips);
                        ImageAndInts iss = new ImageAndInts
                        {
                            pb = box,
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
                                int start = m.twitchMessage.message.IndexOf(s, lastLoc);
                                int stop = start + s.Length - 1;
                                Tuple<int, int> ints = new Tuple<int, int>(start, stop);
                                lastLoc = stop;
                                PictureBoxWithInterpolation box = new PictureBoxWithInterpolation
                                {
                                    InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High,
                                    Image = Emojis.codeToEmoji[s],
                                    SizeMode = PictureBoxSizeMode.StretchImage,
                                    Size = new Size(18, 18)
                                };
                                Controls.ToolTip tip = new Controls.ToolTip(box)
                                {
                                    BackColor = Color.Black,
                                    ForeColor = Color.White,
                                    Image = box.Image,
                                    Text = "Emoji: " + s,
                                };
                                AddTooltip(box, tip, p, tooltips);
                                ImageAndInts iss = new ImageAndInts
                                {
                                    pb = box,
                                    ints = ints
                                };
                                try
                                {
                                    emoteBoxes.Add(start, iss);
                                }
                                catch { }
                            }
                    }
                }
            string[] tBadges = m.twitchMessage.badges.Split(',');
            if (tBadges[0] != null)
            {
                foreach (string s in tBadges)
                {
                    string[] parts = s.Split('/');
                    if (cachedBadges.ContainsKey(parts[0]))
                    {
                        PictureBoxWithInterpolation box = new PictureBoxWithInterpolation
                        {
                            Image = cachedBadges[parts[0]].versions[parts[1]].image,
                            SizeMode = PictureBoxSizeMode.AutoSize
                        };
                        p.Controls.Add(box);
                        m.badges.Add(box);
                        Controls.ToolTip tip = new Controls.ToolTip(box)
                        {
                            BackColor = Color.Black,
                            ForeColor = Color.White,
                            Image = box.Image,
                            Text = cachedBadges[parts[0]].versions[parts[1]].title,
                        };
                        AddTooltip(box, tip, p, tooltips);
                    }
                }
            }
            string[] emotes = m.twitchMessage.emotes.Split('/');
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
                        string code = m.twitchMessage.message.Substring(firstIndex, firstIndex + length > m.twitchMessage.message.Length - 1 ? m.twitchMessage.message.Length - firstIndex : length);
                        string theId = s.Substring(0, start);
                        string theUrl = "http://static-cdn.jtvnw.net/emoticons/v1/" + theId + "/1.0";
                        PictureBox box = new PictureBox();
                        if (MainForm.generalSettings.twitchEmoteCaching)
                        {
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
                            //if (!cachedTwitchEmotes.ContainsKey(code))
                            //    cachedTwitchEmotes.Add(code, image);
                            box.Image = image;
                            box.SizeMode = PictureBoxSizeMode.AutoSize;
                        }
                        if (!MainForm.generalSettings.twitchEmoteCaching)
                        {
                            box.ImageLocation = theUrl;
                            box.Size = new Size(28, 28);
                            box.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                        Controls.ToolTip tip = new Controls.ToolTip(box)
                        {
                            BackColor = Color.Black,
                            ForeColor = Color.White,
                            Image = box.Image,
                            Text = "Twitch Emote: " + code,
                        };
                        AddTooltip(box, tip, p, tooltips);
                        ImageAndInts iss = new ImageAndInts
                        {
                            pb = box,
                            ints = ints[i]
                        };
                        try
                        {
                            emoteBoxes.Add(firstIndex, iss);
                        }
                        catch { }
                    }
                }
            }
            
            m.panel = p;
            m.tooltips = tooltips;
            m.emotes = emoteBoxes;
            m.messages = labelsToAdd;
            p.Location = new Point(-Width, Height - p.Size.Height - 50);
            //allTwitchMessages.Add(m.twitchMessage);
            currentChatMessages.Add(m);
            coroutineManager.StartCoroutine(moveLabels(m));
            return null;
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
