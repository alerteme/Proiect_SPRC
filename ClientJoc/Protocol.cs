namespace ClientJoc
{
    public static class Protocol
    {
        public const string LOGIN      = "LOGIN";
        public const string CHAT       = "CHAT";
        public const string DISCONNECT = "DISCONNECT";
        public const string SERVER_MSG = "SERVER_MSG";

        public static string BuildLogin(string username)
            => $"{LOGIN}|{username}";

        public static string BuildChat(string username, string text)
            => $"{CHAT}|{username}|{text}";

        public static string BuildDisconnect(string username)
            => $"{DISCONNECT}|{username}";

        public static string BuildServerMsg(string text)
            => $"{SERVER_MSG}|{text}";

        public static (string type, string[] fields) Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return ("UNKNOWN", new string[0]);
            var parts = raw.Split('|');
            return (parts[0].Trim().ToUpper(), parts);
        }
    }
}