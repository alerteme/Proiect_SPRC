using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Windows.Forms;

namespace ServerJoc
{
    static class Program
    {
        public static List<TcpClient> clienti = new List<TcpClient>();

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}