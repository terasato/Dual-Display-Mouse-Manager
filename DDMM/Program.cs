using System;
using System.Windows.Forms;

namespace DDMM
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Application.Run(new DDMM_Form());
            var args = Environment.GetCommandLineArgs();
            var controller = new SingleInstanceController();
            controller.Run(args);
        }
    }
}