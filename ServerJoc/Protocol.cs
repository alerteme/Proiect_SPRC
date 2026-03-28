namespace ServerJoc
{
    public static class Protocol
    {
        public const string LOGIN = "LOGIN";
        public const string CHAT = "CHAT";
        public const string DISCONNECT = "DISCONNECT";
        public const string SERVER_MSG = "SERVER_MSG";

        /// <summary>Construiește un mesaj LOGIN|username</summary>
        public static string BuildLogin(string username)
            => $"{LOGIN}|{username}";

        /// <summary>Construiește un mesaj CHAT|username|text</summary>
        public static string BuildChat(string username, string text)
            => $"{CHAT}|{username}|{text}";

        /// <summary>Construiește un mesaj DISCONNECT|username</summary>
        public static string BuildDisconnect(string username)
            => $"{DISCONNECT}|{username}";

        /// <summary>Construiește un mesaj SERVER_MSG|text</summary>
        public static string BuildServerMsg(string text)
            => $"{SERVER_MSG}|{text}";

        /// <summary>Parsează un mesaj și returnează tipul și câmpurile</summary>
        public static (string type, string[] fields) Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return ("UNKNOWN", new string[0]);

            var parts = raw.Split('|');
            string type = parts[0].Trim().ToUpper();
            return (type, parts);
        }
    }
}