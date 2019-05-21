using System;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;

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

    public class SingleInstanceController : WindowsFormsApplicationBase
    {
        public SingleInstanceController()
        {
            IsSingleInstance = true;
            StartupNextInstance += this_StartupNextInstance;
        }

        private void this_StartupNextInstance(object sender, StartupNextInstanceEventArgs e)
        {
            var form = MainForm as DdmmForm;
            form?.NotifyNewInstance();
        }

        protected override void OnCreateMainForm()
        {
            MainForm = new DdmmForm();
        }
    }
}