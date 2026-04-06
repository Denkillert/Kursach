using System;
using System.Windows.Forms;
using PolesSU_Sports.Management;

namespace PolesSU_Sports
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
    }
}