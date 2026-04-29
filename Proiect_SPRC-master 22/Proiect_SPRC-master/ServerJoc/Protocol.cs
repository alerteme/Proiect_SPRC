namespace ServerJoc
{
    public static class Protocol
    {
        public const string LOGIN = "LOGIN";
        public const string CHAT = "CHAT";
        public const string DISCONNECT = "DISCONNECT";
        public const string SERVER_MSG = "SERVER_MSG";
        public const string GAME_START = "GAME_START";
        public const string YOUR_GRID = "YOUR_GRID";
        public const string ATTACK = "ATTACK";
        public const string RESULT = "RESULT";
        public const string YOUR_TURN = "YOUR_TURN";
        public const string WAIT_TURN = "WAIT_TURN";
        public const string GAME_OVER = "GAME_OVER";

        public static string BuildLogin(string username)
        { return LOGIN + "|" + username; }

        public static string BuildChat(string username, string text)
        { return CHAT + "|" + username + "|" + text; }

        public static string BuildDisconnect(string username)
        { return DISCONNECT + "|" + username; }

        public static string BuildServerMsg(string text)
        { return SERVER_MSG + "|" + text; }

        public static string BuildGameStart(string adversar, bool estiPrimul)
        { return GAME_START + "|" + adversar + "|" + (estiPrimul ? "1" : "0"); }

        public static string BuildYourGrid(string encoded)
        { return YOUR_GRID + "|" + encoded; }

        public static string BuildAttack(int row, int col)
        { return ATTACK + "|" + row + "," + col; }

        public static string BuildResult(int row, int col, string outcome,
                                         bool yourTurn, bool gameOver)
        {
            return RESULT + "|" + row + "," + col + "|" + outcome
                         + "|" + (yourTurn ? "1" : "0")
                         + "|" + (gameOver ? "1" : "0");
        }

        public static string BuildGameOver(string winner)
        { return GAME_OVER + "|" + winner; }

        public static string[] Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return new string[] { "UNKNOWN" };
            return raw.Split('|');
        }
    }
}