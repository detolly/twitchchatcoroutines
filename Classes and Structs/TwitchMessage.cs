
namespace TwitchChatCoroutines
{
    struct TwitchMessage
    {
        //@badges=premium/1;color=;display-name=wangstar1;emotes=;id=b0167ef4-58a8-4591-b094-cd6d9da90c55;mod=0;room-id=192805101;subscriber=0;tmi-sent-ts=1529454707822;turbo=0;user-id=184083783;user-type= :wangstar1!wangstar1@wangstar1.tmi.twitch.tv PRIVMSG #nhl :message here
        public string badges;
        public string color;
        public string display_name;
        public string emotes;
        public string id;
        public byte mod;
        public long room_id;
        public byte subscriber;
        public long tmi_sent_ts;
        public byte turbo;
        public long user_id;
        //public string user_type;
        public string message;

        public static TwitchMessage GetTwitchMessage(string raw)
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
    }
}