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
using static TwitchChatCoroutines.ClassesAndStructs.HelperFunctions;

namespace TwitchChatCoroutines
{
    public partial class ChatForm : Form
    {
        private CoroutineManager coroutineManager = new CoroutineManager();

        private string botUsername;
        private string oauth;
        private string channelToJoin = "";

        private int panelBorder = 15;

        private ColorConverter cc = new ColorConverter();

        public static string client_id = "570bj9vd1lakwt3myr8mrhg05ia5u9";

        Queue<MessageControl> stringsToBeAdded = new Queue<MessageControl>();
        private Font font;
        private int pixelsToMove;
        private Color outlineColor;
        private Color textColor;

        private SortedList<string, Image> cachedBTTVEmotes = new SortedList<string, Image>();
        private SortedList<string, Image> cachedFFZEmotes = new SortedList<string, Image>();
        private SortedList<string, Image> cachedTwitchEmotes = new SortedList<string, Image>();

        private Image splitter = Properties.Resources.splitter;

        private int emoteSpacing = 0;

        Random r = new Random();

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
        private int border = 5;

        private bool doAnimations = false;
        private bool doSplitter = false;

        private ChatFormSettings chatFormSettings;

        private string channelId;

        private SortedList<string, Dictionary<string, string>> badges;

        private WebClient client;

        // Useful variables to change
        int pixelsToMovee(int pixelsToMove)
        {
            //return pixelsToMove/10;
            return pixelsToMove;
        }

        void ChangedEvent(object o, EventArgs e)
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
            emoteSpacing = chatFormSettings.EmoteSpacing;
        }

        IEnumerator enterLoginPanel(Panel p)
        {
            int v = Height / 2 - p.Size.Height;
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
                p.Location = new Point(p.Location.X, p.Location.Y - (int)(1 + Math.Abs((p.Location.Y + Height/2) * 0.05f)));
                yield return new WaitForMilliseconds(10);
            }
            authenticated = true;
            Controls.Remove(p);
            yield break;
        }

        public ChatForm(ChatFormSettings chatFormSettings)
        {
            InitializeComponent();

            if ((ChatModes)chatFormSettings.ChatMode.currentIndex == ChatModes.ChatUser)
            {

                panel1.BackColor = chatFormSettings.BackgroundColor;
                panel1.ForeColor = chatFormSettings.ForegroundColor;

                int h = SystemInformation.PrimaryMonitorSize.Height - 150;
                Height = h;

                coroutineManager.Init();
                Text = chatFormSettings.Channel;

                this.chatFormSettings = chatFormSettings;
                panel1.Location = new Point(Width / 2 - panel1.Size.Width / 2, Height);
                panel1.Anchor = AnchorStyles.Left & AnchorStyles.Right & AnchorStyles.Top & AnchorStyles.Bottom;

                comboBox1.Items.Clear();
                foreach (string s in Authentication.GetLogins())
                {
                    comboBox1.Items.Add(s);
                }
                comboBox1.SelectedIndex = 0;
                coroutineManager.StartCoroutine(enterLoginPanel(panel1));

            }

            Directory.CreateDirectory("./emotes/BetterTTV");
            Directory.CreateDirectory("./emotes/FFZ");
            Directory.CreateDirectory("./emotes/Twitch");

            outlineColor = chatFormSettings.BackgroundColor;
            textColor = chatFormSettings.ForegroundColor;

            channelToJoin = chatFormSettings.Channel;

            headers = new string[] {
                    "Client-ID: " + client_id
            };
            font = chatFormSettings.Font;
            doAnimations = chatFormSettings.Animations;
            emoteSpacing = chatFormSettings.EmoteSpacing;
            chatFormSettings.Changed += ChangedEvent;

            badges = new SortedList<string, Dictionary<string, string>>();
            BackColor = outlineColor;
            //TransparencyKey = BackColor;
            client = new WebClient();
            badgeJson = jsonGet("https://badges.twitch.tv/v1/badges/global/display", headers).badge_sets;
            try
            {
                bttvEmotesJson = jsonGet("https://api.betterttv.net/2/emotes").emotes;
                bttvChannelEmotesJson = jsonGet("https://api.betterttv.net/2/channels/" + channelToJoin).emotes;
            }
            catch
            {
                useBTTV = false;
            }
            //FFZEmotesJson = jsonGet("https://api.betterttv.net/2/emotes").emotes;
            try
            {
                FFZChannelEmotesJson = jsonGet("https://api.frankerfacez.com/v1/room/" + channelToJoin);
                FFZChannelEmotesJson = FFZChannelEmotesJson.sets[(string)(FFZChannelEmotesJson.room.set)].emoticons;
                FFZEmotesJson = jsonGet("https://api.frankerfacez.com/v1/set/global");
            }
            catch
            {
                useFFZ = false;
            }
            channelInformationJson = jsonGet("https://api.twitch.tv/helix/users?login=" + channelToJoin, headers);
            channelId = channelInformationJson.data[0].id;
            channelBadgeJson = jsonGet("https://badges.twitch.tv/v1/badges/channels/" + channelId + "/display", headers);

            if (useBTTV)
            {
                var temporary = JArray.FromObject(bttvEmotesJson);
                foreach (var entry in temporary)
                {
                    string channel = entry.channel;
                    string emote = entry.id;
                    string code = entry.code;
                    string imageType = entry.imageType;
                    string path = "./emotes/BetterTTV/BTTV" + emote + "." + imageType;
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
                var temporary2 = JArray.FromObject(bttvChannelEmotesJson);
                foreach (var entry in temporary2)
                {
                    string channel = entry.channel;
                    string emote = entry.id;
                    string code = entry.code;
                    string imageType = entry.imageType;
                    string path = "./emotes/BetterTTV/BTTV" + emote + "." + imageType;
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
                var temporary3 = JArray.FromObject(FFZChannelEmotesJson);
                foreach (var entry in temporary3)
                {
                    string code = entry.name;
                    string url = "http://cdn.frankerfacez.com/emoticon/" + entry.id + "/1";
                    string path = "./emotes/FFZ/FFZ" + entry.id + ".png";
                    if (!File.Exists(path))
                        client.DownloadFile(new Uri(url), path);
                    Image image = Image.FromFile(path);
                    try
                    {
                        cachedFFZEmotes.Add(code, image);
                    }
                    catch { }
                }
                var temporary4 = JArray.FromObject(FFZEmotesJson.sets["3"].emoticons);
                foreach (var entry in temporary4)
                {
                    string code = entry.name;
                    string url = "http://cdn.frankerfacez.com/emoticon/" + entry.id + "/1";
                    string path = "./emotes/FFZ/FFZ" + entry.id + ".png";
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
            Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>> strings = badgeJson.ToObject<Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>>>();
            foreach (var entry in strings)
            {
                List<string> keys = new List<string>(entry.Value["versions"].Keys);
                Dictionary<string, string> dict = new Dictionary<string, string>();
                for (int i = 0; i < entry.Value["versions"].Keys.Count; i++)
                {
                    dict.Add(keys[i], entry.Value["versions"][keys[i]]["image_url_1x"]);
                }
                badges.Add(entry.Key, dict);
            }
            Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>> strings2 = channelBadgeJson.badge_sets.ToObject<Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>>>();
            foreach (var entry in strings2)
            {
                List<string> keys = new List<string>(entry.Value["versions"].Keys);
                Dictionary<string, string> dict = new Dictionary<string, string>();
                for (int i = 0; i < entry.Value["versions"].Keys.Count; i++)
                {
                    dict.Add(keys[i], entry.Value["versions"][keys[i]]["image_url_1x"]);
                    try
                    {
                        badges[entry.Key].Remove(keys[i]);
                        badges[entry.Key].Add(keys[i], entry.Value["versions"][keys[i]]["image_url_1x"]);
                    }
                    catch { }
                }
                try
                {
                    badges.Add(entry.Key, dict);
                }
                catch { }
            }
        }

        IEnumerator enterChatLine(MessageControl greetings)
        {
            while (greetings.panel.Location.X < 0)
            {
                if (!doAnimations)
                    greetings.panel.Location = new Point(0, greetings.panel.Location.Y);
                else if (doAnimations)
                {
                    if ((greetings.panel.Location.X + (1 + Math.Abs(greetings.panel.Location.X * 0.1f))) > 0)
                    {
                        greetings.panel.Location = new Point(0, greetings.panel.Location.Y);
                        continue;
                    }
                    greetings.panel.Location = new Point((int)(greetings.panel.Location.X + (1 + Math.Abs(greetings.panel.Location.X * 0.1f))), greetings.panel.Location.Y);
                }
                yield return new WaitForMilliseconds(5);
            }
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
                        Controls.Remove(toRemove[i].panel);
                        toRemove[i].panel.Dispose();
                        //StartLateCoroutine(removeChatLine(toRemove[i])); // Totally doesn't work btw unless your cpu is like insane
                    }
                }));
            yield break;
        }

        public string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        public void CustomUpdate()
        {
            coroutineManager.Interval();
            if (!twitchClient.Connected)
            {
                Connect();
                //MessageControl m = new MessageControl();
                //m.oneMessage = "Disconnected from chat. Reconnecting.";
                //Label l = MakeAndInsertLabel(m);
                //l.ForeColor = Color.Red;
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
                    TwitchMessage user = GetTwitchMessage(rawLine);
                    string username = user.display_name; //GetUsername(rawLine);
                    string extractedMessage = user.message; //GetExtractedMessage(rawLine);
                    bool isAction = false;
                    if (extractedMessage.StartsWith("\u0001"))
                    {
                        extractedMessage = extractedMessage.Replace("\u0001", "");
                        extractedMessage = ReplaceFirst(extractedMessage, "ACTION ", "");
                        isAction = true;
                    }
                    user.message = extractedMessage;
                    MessageControl m = new MessageControl
                    {
                        twitchMessage = user,
                        isAction = isAction
                    };
                    stringsToBeAdded.Enqueue(m);
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
            if (stringsToBeAdded.Count > 0)
            {
                MakeAndInsertLabel(stringsToBeAdded.Dequeue());
            }
        }

        TwitchMessage GetTwitchMessage(string raw)
        {
            TwitchMessage returnMessage = new TwitchMessage();

            int iStartRaw = raw.IndexOf("@");
            string current = "badges=";

            int iStartBadges = raw.IndexOf(current, iStartRaw) + current.Length;
            int iStopBadges = raw.IndexOf(";", iStartBadges);
            returnMessage.badges = raw.Substring(iStartBadges, iStopBadges - iStartBadges);

            current = "color=";
            int iStartColor = raw.IndexOf(current, iStopBadges) + current.Length;
            int iStopColor = raw.IndexOf(";", iStartColor);
            returnMessage.color = raw.Substring(iStartColor, iStopColor - iStartColor);

            current = "display-name=";
            int iStartDisplayName = raw.IndexOf(current, iStopColor) + current.Length;
            int iStopDisplayName = raw.IndexOf(";", iStartDisplayName);
            returnMessage.display_name = raw.Substring(iStartDisplayName, iStopDisplayName - iStartDisplayName);

            current = "emotes=";
            int iStartEmotes = raw.IndexOf(current, iStopDisplayName) + current.Length;
            int iStopEmotes = raw.IndexOf(";", iStartEmotes);
            returnMessage.emotes = raw.Substring(iStartEmotes, iStopEmotes - iStartEmotes);

            current = "id=";
            int iStartId = raw.IndexOf(current, iStopEmotes) + current.Length;
            int iStopId = raw.IndexOf(";", iStartId);
            returnMessage.id = raw.Substring(iStartId, iStopId - iStartId);

            current = "mod=";
            int iStartMod = raw.IndexOf(current, iStopId) + current.Length;
            int iStopMod = raw.IndexOf(";", iStartMod);
            returnMessage.mod = byte.Parse(raw.Substring(iStartMod, iStopMod - iStartMod));

            current = "room-id=";
            int iStartRoomId = raw.IndexOf(current, iStopMod) + current.Length;
            int iStopRoomId = raw.IndexOf(";", iStartRoomId);
            returnMessage.room_id = long.Parse(raw.Substring(iStartRoomId, iStopRoomId - iStartRoomId));

            current = "subscriber=";
            int iStartSubscriber = raw.IndexOf(current, iStopRoomId) + current.Length;
            int iStopSubscriber = raw.IndexOf(";", iStartSubscriber);
            returnMessage.subscriber = byte.Parse(raw.Substring(iStartSubscriber, iStopSubscriber - iStartSubscriber));

            current = "tmi-sent-ts=";
            int iStartTMI = raw.IndexOf(current, iStopSubscriber) + current.Length;
            int iStopTMI = raw.IndexOf(";", iStartTMI);
            returnMessage.tmi_sent_ts = long.Parse(raw.Substring(iStartTMI, iStopTMI - iStartTMI));

            current = "turbo=";
            int iStartTurbo = raw.IndexOf(current, iStopTMI) + current.Length;
            int iStopTurbo = raw.IndexOf(";", iStartTurbo);
            returnMessage.turbo = byte.Parse(raw.Substring(iStartTurbo, iStopTurbo - iStartTurbo));

            current = "user-id=";
            int iStartUserID = raw.IndexOf(current, iStopTurbo) + current.Length;
            int iStopUserID = raw.IndexOf(";", iStartUserID);
            returnMessage.user_id = long.Parse(raw.Substring(iStartUserID, iStopUserID - iStartUserID));

            //current = "user-type=";
            //int iStartUserType = raw.IndexOf(current, iStopUserID) + current.Length;
            //int iStopUserType = raw.IndexOf(";", iStartUserType);
            //returnMessage.id = raw.Substring(iStartUserType, iStopUserType - iStartUserType);

            returnMessage.message = raw.Substring(raw.IndexOf(':', raw.IndexOf(':', iStopUserID) + 1) + 1);

            return returnMessage;
        }

        private TwitchLabel MakeAndInsertLabel(MessageControl m)
        {
            // http://static-cdn.jtvnw.net/emoticons/v1/:<emote ID>/1.0
            //<emote ID>:<first index>-<last index>,<another first index>-<another last index>/<another emote ID>:<first index>-<last index>...
            SortedList<int, PictureBoxAndInts> emoteBoxes = new SortedList<int, PictureBoxAndInts>();
            List<TwitchLabel> labelsToAdd = new List<TwitchLabel>();
            Panel p = new Panel();
            Controls.Add(p);
            string[] array = m.twitchMessage.message.Split(' ');
            int lastLoc = 0;
            if (useFFZ || useBTTV)
                foreach (string a in array)
                {
                    if (cachedBTTVEmotes.ContainsKey(a))
                    {
                        int start = m.twitchMessage.message.IndexOf(a, lastLoc);
                        int stop = start + a.Length - 1;
                        Tuple<int, int> ints = new Tuple<int, int>(start, stop);
                        lastLoc = stop;
                        PictureBox pb = new PictureBox
                        {
                            Image = cachedBTTVEmotes[a],
                            SizeMode = PictureBoxSizeMode.AutoSize
                        };
                        PictureBoxAndInts iss = new PictureBoxAndInts
                        {
                            pb = pb,
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
                        PictureBox pb = new PictureBox
                        {
                            Image = cachedFFZEmotes[a],
                            SizeMode = PictureBoxSizeMode.AutoSize
                        };
                        PictureBoxAndInts iss = new PictureBoxAndInts
                        {
                            pb = pb,
                            ints = ints
                        };
                        emoteBoxes.Add(start, iss);
                    }
                }
            List<PictureBox> badgges = new List<PictureBox>();
            string[] tBadges = m.twitchMessage.badges.Split(',');
            if (tBadges[0] != null)
            {
                foreach (string s in tBadges)
                {
                    string[] parts = s.Split('/');
                    if (badges.ContainsKey(parts[0]))
                    {
                        PictureBox box = new PictureBox();
                        p.Controls.Add(box);
                        box.ImageLocation = badges[parts[0]][parts[1]];
                        box.Size = new Size(18, 18);
                        badgges.Add(box);
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
                        string code = m.twitchMessage.message.Substring(firstIndex, secondIndex - firstIndex + 1);
                        string theId = s.Substring(0, start);
                        string theUrl = "http://static-cdn.jtvnw.net/emoticons/v1/" + theId + "/1.0";
                        PictureBox b = new PictureBox();
                        if (Forms.MainForm.generalSettings.twitchEmoteCaching)
                        {
                            string path = "./emotes/Twitch/Twitch" + theId + ".png";
                            if (!File.Exists(path))
                            {
                                client.DownloadFile(theUrl, path);
                            }
                            Image image = Image.FromFile(path);
                            if (!cachedTwitchEmotes.ContainsKey(code))
                                cachedTwitchEmotes.Add(code, image);
                            b.Image = image;
                            b.SizeMode = PictureBoxSizeMode.AutoSize;
                        }
                        if (!Forms.MainForm.generalSettings.twitchEmoteCaching)
                        {
                            b.ImageLocation = theUrl;
                            b.Size = new Size(28, 28);
                            b.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                        PictureBoxAndInts iss = new PictureBoxAndInts
                        {
                            pb = b,
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
            int tStart = 0;
            bool exists = false;
            foreach (var s in badgges)
            {
                exists = true;
                s.Location = new Point(tStart + border, 100);
                tStart += s.Size.Width + border;
            }
            TwitchLabel userNameLabel = new TwitchLabel(this);
            userNameLabel.MaximumSize = new Size(Width - 20 - userNameLabel.Size.Width, 0);
            userNameLabel.Font = font;
            p.Controls.Add(userNameLabel);
            userNameLabel.Text = m.twitchMessage.display_name;
            userNameLabel.Location = new Point(tStart + border, 100 + (exists ? badgges[0].Size.Height / 2 - userNameLabel.Size.Height / 2 : 0));
            userNameLabel.ForeColor = (Color)cc.ConvertFromString(m.twitchMessage.color == "" ? getRandomColor() : m.twitchMessage.color);
            string text = m.twitchMessage.message;

            int nextStart = 0;
            int lastLocation = userNameLabel.Right;
            bool first = true;
            int yoffset = 0;

            foreach (var pbandInt in emoteBoxes)
            {
                Tuple<int, int> ints = pbandInt.Value.ints;
                PictureBox pb = pbandInt.Value.pb;
                TwitchLabel thel = new TwitchLabel(this);
                if (first)
                {
                    thel.Text = ": ";
                    first = false;
                }
                thel.Font = font;
                thel.ForeColor = m.isAction ? (Color)cc.ConvertFromString(m.twitchMessage.color == "" ? "#FFFFFF" : m.twitchMessage.color) : textColor;
                thel.Text += text.Substring(nextStart, (ints.Item1 - nextStart) < 0 ? 0 : ints.Item1 - nextStart);
                TwitchLabel comparison = thel;
                if (thel.Text != "" && thel.Text != " ")
                {
                    if (thel.Text[0] == ' ')
                        thel.Text = thel.Text.Substring(1);
                    if (thel.Text[thel.Text.Length - 1] == ' ')
                        thel.Text = thel.Text.Substring(0, thel.Text.Length - 1);
                    p.Controls.Add(thel);
                    thel.Location = new Point(lastLocation, userNameLabel.Location.Y + yoffset);
                    int startingLoc = comparison.Location.X + border + TextRenderer.MeasureText(".", font).Width * 2;
                    if (TextRenderer.MeasureText(comparison.Text, font).Width + startingLoc > Width - (vScrollBar1.Visible ? vScrollBar1.Width : 0))
                    {
                        var args = new List<string>(thel.Text.Split(' '));
                        string upTillNow = "";
                        for (int i = 0; i < args.Count; i++)
                        {
                            startingLoc = comparison.Location.X + border + TextRenderer.MeasureText(".", font).Width * 2;
                            string old = upTillNow;
                            upTillNow += args[i];
                            if (TextRenderer.MeasureText(upTillNow, font).Width + startingLoc > Width - (vScrollBar1.Visible ? vScrollBar1.Width : 0))
                            {
                                var a = TextRenderer.MeasureText(args[i], font).Width;
                                if (a + 2 * border > Width - vScrollBar1.Width)
                                {
                                    string current = "";
                                    for (int x = 0; x < args[i].Length; x++)
                                    {
                                        string anotherold = current;
                                        current += args[i][x];
                                        if (TextRenderer.MeasureText(current + old, font).Width + startingLoc > Width - (vScrollBar1.Visible ? vScrollBar1.Width : 0))
                                        {
                                            x = x < 0 ? 0 : x;
                                            args.Insert(i + 1, args[i].Substring(x));
                                            args[i] = args[i].Substring(0, x);
                                            old += anotherold;
                                            i++;
                                            break;
                                        }
                                    }
                                }
                                comparison.Text = old;
                                yoffset += Math.Max(pb.Height, comparison.Height);
                                TwitchLabel newLabel = new TwitchLabel(this)
                                {
                                    Font = font,
                                    ForeColor = m.isAction ? (Color)cc.ConvertFromString(m.twitchMessage.color == "" ? "#FFFFFF" : m.twitchMessage.color) : textColor
                                };
                                p.Controls.Add(newLabel);
                                newLabel.Location = new Point(border, userNameLabel.Location.Y + yoffset);
                                comparison = newLabel;
                                labelsToAdd.Add(newLabel);
                                lastLocation = newLabel.Right;
                                upTillNow = "";
                                i--;
                            }
                            else
                            {
                                upTillNow += " ";
                            }
                            if (i == args.Count - 1)
                            {
                                comparison.Text = upTillNow.Substring(0, upTillNow.Length - 1);
                            }
                        }
                    }
                    int rightborder = comparison.Right + pb.Width;
                    lastLocation = rightborder > Width ? border : rightborder - pb.Width + emoteSpacing;
                    yoffset += rightborder > Width ? Math.Max(28, comparison.Height) : 0;
                    labelsToAdd.Add(thel);
                }
                nextStart = ints.Item2 + 1;
                int theOr = lastLocation + (int)(pb.Size.Width * 2) + border;
                yoffset += theOr > Width ? Math.Max(28, comparison.Height) : 0;
                pb.Location = new Point(theOr > Width ? border : lastLocation, userNameLabel.Location.Y + userNameLabel.Size.Height / 2 - pb.Size.Height / 2 + yoffset);
                lastLocation = pb.Right + emoteSpacing;
                p.Controls.Add(pb);
            }
            TwitchLabel lastLabel = new TwitchLabel(this)
            {
                ForeColor = m.isAction ? (Color)cc.ConvertFromString(m.twitchMessage.color == "" ? "#FFFFFF" : m.twitchMessage.color) : textColor,
                Font = font
            };
            lastLabel.MaximumSize = new Size(Width - 20 - lastLabel.Size.Width - userNameLabel.Size.Width, 0);
            string theT = text.Substring(nextStart > text.Length ? text.Length - 1 : nextStart, (text.Length - nextStart < 0) ? 0 : text.Length - nextStart);
            lastLabel.Text = first ? ": " + theT : theT;
            if (first || theT.Length > 0)
            {
                if (lastLabel.Text[0] == ' ')
                    lastLabel.Text = lastLabel.Text.Substring(1);
                p.Controls.Add(lastLabel);
                lastLabel.Location = new Point(lastLocation, userNameLabel.Location.Y + yoffset);
                labelsToAdd.Add(lastLabel);
                TwitchLabel labelToCompare = lastLabel;
                var args = new List<string>(lastLabel.Text.Split(' '));
                string stringCompare = "";
                for (int i = 0; i < args.Count; i++)
                {
                    int startingLoc = labelToCompare.Location.X + border + TextRenderer.MeasureText(".", font).Width * 2;
                    string old = stringCompare;
                    stringCompare += args[i];
                    if (TextRenderer.MeasureText(stringCompare, font).Width + startingLoc > Width - (vScrollBar1.Visible ? vScrollBar1.Width : 0))
                    {
                        var a = TextRenderer.MeasureText(args[i], font).Width;
                        if (a + 2 * border + startingLoc > Width - vScrollBar1.Width)
                        {
                            string current = "";
                            for (int x = 0; x < args[i].Length; x++)
                            {
                                string anotherold = current;
                                current += args[i][x];
                                if (TextRenderer.MeasureText(current + old, font).Width + startingLoc > Width - (vScrollBar1.Visible ? vScrollBar1.Width : 0))
                                {
                                    x = x < 0 ? 0 : x;
                                    args.Insert(i + 1, args[i].Substring(x));
                                    args[i] = args[i].Substring(0, x);
                                    old += anotherold;
                                    i++;
                                    break;
                                }
                            }
                        }
                        yoffset += labelToCompare.Height;
                        TwitchLabel l = new TwitchLabel(this);
                        labelToCompare.Text = old;
                        p.Controls.Add(l);
                        l.Location = new Point(border, userNameLabel.Location.Y + yoffset);
                        l.Font = font;
                        l.ForeColor = m.isAction ? (Color)cc.ConvertFromString(m.twitchMessage.color == "" ? "#FFFFFF" : m.twitchMessage.color) : textColor;
                        i--;
                        stringCompare = "";
                        labelsToAdd.Add(l);
                        labelToCompare = l;
                    }
                    else
                    {
                        stringCompare += " ";
                    }
                    if (i == args.Count - 1)
                        labelToCompare.Text = stringCompare;
                }
            }
            int highest = 0;
            int lowest = 1000;
            PictureBox splitterbox = null;
            splitterbox = new PictureBox
            {
                Image = splitter,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = new Size(Width, 1)
            };
            Control lowestC = null;
            if (!doSplitter)
                splitterbox.Visible = false;
            for (int i = 0; i < p.Controls.Count; i++)
            {
                if (p.Controls[i].Size.Height + p.Controls[i].Location.Y > highest)
                    highest = p.Controls[i].Bottom;
                if (p.Controls[i].Location.Y < lowest)
                {
                    lowestC = p.Controls[i];
                    lowest = p.Controls[i].Location.Y;
                }
            }
            p.Size = new Size(Width, highest - lowest + panelBorder);
            splitterbox.Location = new Point(0, lowestC.Location.Y - panelBorder);
            p.Controls.Add(splitterbox);
            lowest = splitterbox.Top;
            for (int i = 0; i < p.Controls.Count; i++)
            {
                p.Controls[i].Location = new Point(p.Controls[i].Location.X, p.Controls[i].Location.Y - lowest);
            }
            if (panelBorder != 0)
                foreach (Control c in p.Controls)
                {
                    if (c == splitterbox) continue;
                    c.Location = new Point(c.Location.X, c.Location.Y - panelBorder / 2);
                }
            m.panel = p;
            m.splitter = splitterbox;
            m.emotes = emoteBoxes;
            m.messages = labelsToAdd;
            m.username = userNameLabel;
            p.Location = new Point(-Width, Height - p.Size.Height - 50);
            //allTwitchMessages.Add(m.twitchMessage);
            currentChatMessages.Add(m);
            coroutineManager.StartCoroutine(moveLabels(m));
            return null;
        }

        private string getRandomColor()
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

        private string GetUsername(string raw)
        {
            return raw.Substring(raw.IndexOf(':') + 1, raw.IndexOf('!') - raw.IndexOf(':') - 1);
        }

        private string GetExtractedMessage(string raw)
        {
            return raw.Substring(raw.IndexOf(':', 1) + 1);
        }

        private void SendRawMessage(string inp)
        {
            writer.WriteLine(inp);
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

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            hasClosed = true;
        }

        private void ChatForm_SizeChanged(object sender, EventArgs e)
        {
            foreach (MessageControl m in currentChatMessages)
                m.splitter.Size = new Size(Width, m.splitter.Size.Height);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WebForm form = new WebForm();
            form.authenticated += (o, auth) =>
            {
                Authentication.Add(auth);
                var g = Authentication.GetLogins();
                comboBox1.Items.Clear();
                foreach (string item in g)
                    comboBox1.Items.Add(item);
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
    }
}
