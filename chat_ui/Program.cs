using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace chat_ui
{
    static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try { Application.Run(new Form1()); } catch { }
        }
    }
}
