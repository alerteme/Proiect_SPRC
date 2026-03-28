using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Windows.Forms;

namespace ServerJoc
{
    // Programul principal al serverului
    static class Program
    {
        // Lista de clienți accesibilă din Form1 (static/shared)
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