using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Sockets;
using System.Windows.Forms;
using CoroutineSystem;
using System.IO;
using static CoroutineSystem.CoroutineManager;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace TwitchChatCoroutines
{
    public partial class Form1 : Form
    {
        private string botUsername = "tSparkles".ToLower();
        private string oauth = "oauth:3rr9k6dz0iyd96km5ymhizad60xf0o";
        private string channelToJoin = "";

        private string client_id = "m4rybj39stievswbum8069zxhxl5y4";

        Queue<MessageControl> stringsToBeAdded = new Queue<MessageControl>();
        private Font font = new Font("sans-serif", 16f);
        private int pixelsToMove = 16;
        private Color outlineColor = Color.Black;
        private Color textColor = Color.White;

        private SortedList<string, Image> cachedBTTVEmotes = new SortedList<string, Image>();
        private SortedList<string, Image> cachedFFZEmotes = new SortedList<string, Image>();
        private SortedList<string, Image> cachedTwitchEmotes = new SortedList<string, Image>();

        bool finished = true;
        int quickness = 1;


        Random r = new Random();

        private ColorConverter cc = new ColorConverter();

        private List<MessageControl> currentChatMessages = new List<MessageControl>();
        private int temporaryThing = 0;

        public bool hasClosed = false;

        private TcpClient twitchClient = new TcpClient();
        private StreamReader reader;
        private StreamWriter writer;

        private dynamic bttvEmotesJson;
        private dynamic bttvChannelEmotesJson;
        private dynamic badgeJson;
        private dynamic channelBadgeJson;
        private dynamic channelInformationJson;

        private bool useFFZ = true;
        private bool useBTTV = true;

        private string channelId;

        //private dynamic FFZEmotesJson;
        private dynamic FFZChannelEmotesJson;

        private SortedList<string, Dictionary<string, string>> badges;

        private WebClient client;

        // Useful variables to change
        int pixelsToMovee(int pixelsToMove)
        {
            //return pixelsToMove/10;
            return pixelsToMove;
        }

        dynamic jsonGet(string url)
        {
            return JsonConvert.DeserializeObject<dynamic>(client.DownloadString(url));
        }

        public Form1()
        {
            InitializeComponent();
            Directory.CreateDirectory("./emotes/BetterTTV");
            Directory.CreateDirectory("./emotes/FFZ");
            Directory.CreateDirectory("./emotes/Twitch");
            var d = new askChannel();
            d.ShowDialog();
            channelToJoin = d.theChannel;
            if (channelToJoin == "" || channelToJoin == null)
            {
                Close();
                hasClosed = true;
                return;
            }
            badges = new SortedList<string, Dictionary<string, string>>();
            BackColor = outlineColor;
            //TransparencyKey = BackColor;
            Height = SystemInformation.PrimaryMonitorSize.Height - 150;
            client = new WebClient();
            client.Headers.Add("Client-ID", client_id);
            badgeJson = jsonGet("https://badges.twitch.tv/v1/badges/global/display").badge_sets;
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
            }
            catch
            {
                useFFZ = false;
            }
            channelInformationJson = jsonGet("https://api.twitch.tv/helix/users?login=" + channelToJoin);
            channelId = channelInformationJson.data[0].id;
            channelBadgeJson = jsonGet("https://badges.twitch.tv/v1/badges/channels/" + channelId + "/display");

            if (useBTTV)
            {
                var temporary = JArray.FromObject(bttvEmotesJson);
                foreach (var entry in temporary)
                {
                    string channel = entry.channel;
                    string emote = entry.id;
                    string code = entry.code;
                    string imageType = entry.imageType;
                    string path = "./emotes/BetterTTV/BTTV" + code.Replace(":", "") + "." + imageType;
                    if (!File.Exists(path))
                        client.DownloadFile(new Uri("http://cdn.betterttv.net/emote/" + emote + "/1x"), path);
                    Image image = Image.FromFile(path);
                    cachedBTTVEmotes.Add(code, image);
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
                    string path = "./emotes/BetterTTV/BTTV" + code.Replace(":", "") + "." + imageType;
                    if (!File.Exists(path))
                        client.DownloadFile(new Uri("http://cdn.betterttv.net/emote/" + emote + "/1x"), path);
                    Image image = Image.FromFile(path);
                    cachedBTTVEmotes.Add(code, image);
                }
            }
            if (useFFZ)
            {
                var temporary3 = JArray.FromObject(FFZChannelEmotesJson);
                foreach (var entry in temporary3)
                {
                    string code = entry.name;
                    string url = "http://cdn.frankerfacez.com/emoticon/" + entry.id + "/1";
                    string path = "./emotes/FFZ/FFZ" + code.Replace(":", "") + ".png";
                    if (!File.Exists(path))
                        client.DownloadFile(new Uri(url), path);
                    Image image = Image.FromFile(path);
                    cachedFFZEmotes.Add(code, image);
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
                } catch { }
            }
        }

        IEnumerator enterChatLine(MessageControl greetings)
        {
            while (greetings.panel.Location.X < 2)
            {
                //move
                greetings.panel.Location = new Point(2, greetings.panel.Location.Y);
                //greetings.panel.Location = new Point((int)(greetings.panel.Location.X + (1 + Math.Abs(greetings.panel.Location.X * 0.1f))), greetings.panel.Location.Y);
                yield return new WaitForMilliseconds(5);
            }
            yield break;
        }

        IEnumerator removeChatLine(Label greetings)
        {
            throw new NotImplementedException();
            while (greetings.Location.X > -greetings.Size.Width - 20)
            {
                //move 
                greetings.Location = new Point((int)(greetings.Location.X - (1 + Math.Abs(greetings.Location.X * 1.1f))), greetings.Location.Y);
                yield return new WaitForMilliseconds(5);
            }
            //currentChatMessages.Remove(greetings);
            Controls.Remove(greetings);
            greetings.Dispose();
            yield break;
        }

        IEnumerator moveLabels(MessageControl exclude)
        {
            List<MessageControl> toRemove = new List<MessageControl>();

            StartLateCoroutine(enterChatLine(exclude));
            pixelsToMove = exclude.panel.Size.Height;

            for (int k = 0; k < pixelsToMove; k += quickness)
            {
                if (k < 0) break;
                quickness = Math.Min(pixelsToMove, pixelsToMovee(pixelsToMove) + temporaryThing);
                for (int i = 0; i < currentChatMessages.Count; i++)
                {
                    if (currentChatMessages[i].messages == exclude.messages) continue;
                    int border = -currentChatMessages[i].panel.Size.Height - 5;
                    pixelsToMove = exclude.panel.Size.Height;
                    currentChatMessages[i].panel.Location = new Point(currentChatMessages[i].panel.Location.X, currentChatMessages[i].panel.Location.Y - (k > pixelsToMove ? k - pixelsToMove : quickness));
                    if (currentChatMessages[i].panel.Location.Y < border)
                        toRemove.Add(currentChatMessages[i]);
                    yield return new WaitForMilliseconds(1);
                }
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                currentChatMessages.Remove(toRemove[i]);
                Controls.Remove(toRemove[i].panel);
                toRemove[i].panel.Dispose();
                //StartLateCoroutine(removeChatLine(toRemove[i])); // Totally doesn't work btw unless your cpu is like insane
            }
            finished = true;
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
                if (rawLine.Contains("PRIVMSG"))
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
                    if (!finished)
                    {
                        temporaryThing = 2 * (temporaryThing + 1);
                    }
                    MessageControl m = new MessageControl();
                    m.twitchMessage = user;
                    m.isAction = isAction;
                    stringsToBeAdded.Enqueue(m);
                }
                else
                {
                    MessageControl m = new MessageControl();
                    m.oneMessage = rawLine;
                    //MakeAndInsertLabel(m).ForeColor = Color.Green;
                }
            }
            if (finished && stringsToBeAdded.Count > 0)
            {
                MakeAndInsertLabel(stringsToBeAdded.Dequeue());
            }
            else if (stringsToBeAdded.Count == 0)
            {
                temporaryThing = 0;
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
            if (m.oneMessage != null)
            {
                TwitchLabel l = new TwitchLabel();
                l.Text = m.oneMessage;
                l.MaximumSize = new Size(Width - 2 * 20, 0);
                l.AutoSize = true;
                l.Font = font;
                l.ForeColor = textColor;
                Controls.Add(l);
                l.Width = Width;
                l.Location = new Point(-Width, Height - 40 - 10 - l.Size.Height);
                currentChatMessages.Add(m);
                finished = false;
                StartLateCoroutine(moveLabels(m));
                return l;
            }
            else
            {
                // http://static-cdn.jtvnw.net/emoticons/v1/:<emote ID>/1.0
                //<emote ID>:<first index>-<last index>,<another first index>-<another last index>/<another emote ID>:<first index>-<last index>...
                SortedList<int, PictureBoxAndInts> emoteBoxes = new SortedList<int, PictureBoxAndInts>();
                List<Label> labelsToAdd = new List<Label>();
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
                            PictureBox pb = new PictureBox();
                            pb.Image = cachedBTTVEmotes[a];
                            pb.SizeMode = PictureBoxSizeMode.AutoSize;
                            PictureBoxAndInts iss = new PictureBoxAndInts();
                            iss.pb = pb;
                            iss.ints = ints;
                            emoteBoxes.Add(start, iss);
                        }
                        else if (cachedFFZEmotes.ContainsKey(a))
                        {
                            int start = m.twitchMessage.message.IndexOf(a, lastLoc);
                            int stop = start + a.Length - 1;
                            Tuple<int, int> ints = new Tuple<int, int>(start, stop);
                            lastLoc = stop;
                            PictureBox pb = new PictureBox();
                            pb.Image = cachedFFZEmotes[a];
                            pb.SizeMode = PictureBoxSizeMode.AutoSize;
                            PictureBoxAndInts iss = new PictureBoxAndInts();
                            iss.pb = pb;
                            iss.ints = ints;
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
                            string path = "./emotes/Twitch/Twitch" + code.Replace(":", "coalon").Replace("<", "lesssthan").Replace(">", "greatterthan").Replace("/", "slassh").Replace("\"", "backslassh") + ".png";
                            if (!File.Exists(path))
                            {
                                client.DownloadFile(new Uri("http://static-cdn.jtvnw.net/emoticons/v1/" + theId + "/1.0"), path);
                            }
                            Image image = Image.FromFile(path);
                            if (!cachedTwitchEmotes.ContainsKey(code))
                                cachedTwitchEmotes.Add(code, image);
                            PictureBox b = new PictureBox();
                            PictureBoxAndInts iss = new PictureBoxAndInts();
                            b.SizeMode = PictureBoxSizeMode.AutoSize;
                            b.Image = image;
                            iss.pb = b;
                            iss.ints = ints[i];
                            try
                            {
                                emoteBoxes.Add(firstIndex, iss);
                            }
                            catch { }
                        }
                    }
                }
                int border = 5;
                int tStart = 0;
                bool exists = false;
                foreach (var s in badgges)
                {
                    exists = true;
                    s.Location = new Point(tStart+border, 10);
                    tStart += s.Size.Width + border;
                }
                TwitchLabel userNameLabel = new TwitchLabel();
                userNameLabel.MaximumSize = new Size(Width - 20 - userNameLabel.Size.Width, 0);
                userNameLabel.Font = font;
                p.Controls.Add(userNameLabel);
                userNameLabel.Text = m.twitchMessage.display_name;
                userNameLabel.Location = new Point(tStart + border, 10 + (exists ? badgges[0].Size.Height / 2 - userNameLabel.Size.Height / 2 : 0));
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
                    TwitchLabel thel = new TwitchLabel();
                    if (first)
                    {
                        thel.Text = ": ";
                        first = false;
                    }
                    thel.Font = font;
                    thel.ForeColor = m.isAction ? (Color)cc.ConvertFromString(m.twitchMessage.color == "" ? "#FFFFFF" : m.twitchMessage.color) : textColor;
                    thel.Text += text.Substring(nextStart, (ints.Item1 - nextStart) < 0 ? 0 : ints.Item1 - nextStart);
                    TwitchLabel comparison = thel;
                    if (thel.Text != "" || thel.Text != " ")
                    {
                        bool f = false;
                        p.Controls.Add(thel);
                        thel.Location = new Point(lastLocation, userNameLabel.Location.Y + yoffset);
                        while (comparison.Right > Width - 5 - pb.Size.Width)
                        {
                            string[] args = thel.Text.Split(' ');
                            string upTillNow = "";
                            for (int i = 0; i < args.Length; i++)
                            {
                                string old = upTillNow;
                                upTillNow += args[i];
                                if (TextRenderer.MeasureText(upTillNow + " ", font).Width + comparison.Location.X + border + pb.Size.Width > Width)
                                {
                                    var a = TextRenderer.MeasureText(args[i], font).Width;
                                    if (a + comparison.Location.X + border + pb.Size.Width > Width)
                                    {
                                        f = true;
                                        break;
                                    }
                                    comparison.Text = old;
                                    yoffset += Math.Max(pb.Height, comparison.Height);
                                    TwitchLabel newLabel = new TwitchLabel();
                                    newLabel.Font = font;
                                    newLabel.ForeColor = m.isAction ? (Color)cc.ConvertFromString(m.twitchMessage.color == "" ? "#FFFFFF" : m.twitchMessage.color) : textColor;
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
                                if (i == args.Length - 1)
                                {
                                    comparison.Text = upTillNow.Substring(0, upTillNow.Length-1);
                                }
                            }
                            if (f)
                                break;
                        }
                        int rightborder = comparison.Right + pb.Size.Width + border;
                        lastLocation = rightborder > Width ? border : comparison.Right;
                        yoffset += rightborder > Width ? pb.Size.Height : 0;
                        labelsToAdd.Add(thel);
                    }
                    nextStart = ints.Item2 + 1;
                    pb.Location = new Point(lastLocation, userNameLabel.Location.Y + Math.Max(userNameLabel.Size.Height / 2, pb.Size.Height / 2) - Math.Min(userNameLabel.Size.Height / 2, pb.Size.Height / 2) + yoffset);
                    lastLocation = pb.Right;
                    p.Controls.Add(pb);
                }
                TwitchLabel lastLabel = new TwitchLabel();
                lastLabel.ForeColor = m.isAction ? (Color)cc.ConvertFromString(m.twitchMessage.color == "" ? "#FFFFFF" : m.twitchMessage.color) : textColor;
                lastLabel.Font = font;
                lastLabel.MaximumSize = new Size(Width - 20 - lastLabel.Size.Width - userNameLabel.Size.Width, 0);
                string theT = text.Substring(nextStart, text.Length - nextStart);
                lastLabel.Text = first ? ": " + theT : theT;
                if (first || theT.Length > 0)
                {
                    p.Controls.Add(lastLabel);
                    lastLabel.Location = new Point(lastLocation, userNameLabel.Location.Y);
                    labelsToAdd.Add(lastLabel);
                    TwitchLabel labelToCompare = lastLabel;
                    var args = lastLabel.Text.Split(' ');
                    string stringCompare = "";
                    for (int i = 0; i < args.Length; i++)
                    {
                        string old = stringCompare;
                        stringCompare += args[i];
                        if (TextRenderer.MeasureText(stringCompare, font).Width > Width - labelToCompare.Location.X - border)
                        {
                            if (TextRenderer.MeasureText(args[i], font).Width > Width - labelToCompare.Location.X - border)
                                break;
                            yoffset += labelToCompare.Height;
                            TwitchLabel l = new TwitchLabel();
                            labelToCompare.Text = old;
                            p.Controls.Add(l);
                            l.Location = new Point(border, lastLabel.Location.Y + yoffset);
                            l.Font = font;
                            l.ForeColor = textColor;
                            i--;
                            stringCompare = "";
                            labelsToAdd.Add(l);
                            labelToCompare = l;
                        }
                        else
                        {
                            stringCompare += " ";
                        }
                        if (i == args.Length - 1)
                            labelToCompare.Text = stringCompare;
                    }
                }
                int highest = 0;
                int lowest = 0;
                for (int i = 0; i < p.Controls.Count; i++)
                {
                    if (p.Controls[i].Size.Height + p.Controls[i].Location.Y > highest)
                        highest = p.Controls[i].Size.Height + p.Controls[i].Location.Y;
                    if (p.Controls[i].Location.Y < lowest)
                        lowest = p.Controls[i].Location.Y;
                }
                for (int i = 0; i < p.Controls.Count; i++)
                {
                    p.Controls[i].Location = new Point(p.Controls[i].Location.X, p.Controls[i].Location.Y - lowest);
                }
                p.Size = new Size(Width, highest - lowest);
                m.panel = p;
                m.emotes = emoteBoxes;
                m.messages = labelsToAdd;
                m.username = userNameLabel;
                p.Location = new Point(-Width, Height - 40 - 10 - p.Size.Height);
                currentChatMessages.Add(m);
            }

            finished = false;
            StartLateCoroutine(moveLabels(m));
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
            twitchClient.Connect("irc.chat.twitch.tv", 6667);
            reader = new StreamReader(twitchClient.GetStream());
            writer = new StreamWriter(twitchClient.GetStream());
            writer.AutoFlush = true;
            writer.WriteLine("PASS " + oauth);
            writer.WriteLine("NICK " + botUsername.ToLower());

            writer.WriteLine("JOIN #" + channelToJoin.ToLower());
            writer.WriteLine("CAP REQ :twitch.tv/tags");
            //writer.WriteLine("CAP REQ :twitch.tv/commands");
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            hasClosed = true;
        }
    }
}
