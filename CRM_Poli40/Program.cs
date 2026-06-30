using System;
using System.Windows.Forms;

namespace CRM_Poli40
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Обработчики непойманных ошибок
            Application.ThreadException += (s, e) =>
                MessageBox.Show(e.Exception.ToString(), "Ошибка в потоке");
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                MessageBox.Show(e.ExceptionObject.ToString(), "Необработанная ошибка");

            Database.Initialize();
            Application.Run(new LoginForm());
        }
    }
}