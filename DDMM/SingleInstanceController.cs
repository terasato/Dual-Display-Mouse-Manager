using Microsoft.VisualBasic.ApplicationServices;

namespace DDMM
{
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