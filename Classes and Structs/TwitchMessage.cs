
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
    }
}