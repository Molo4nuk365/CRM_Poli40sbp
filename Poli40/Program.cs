using System;
using System.Windows.Forms;

namespace Poli40
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Database.Initialize();
            Application.Run(new LoginForm());
        }
    }
}