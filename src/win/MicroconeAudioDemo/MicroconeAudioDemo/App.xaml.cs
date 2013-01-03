using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MicroconeSDKDemo.DevAudio.Library;
using DevAudio.Microcone;

namespace MicroconeSDKDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Actions actions = new Actions();
        public Actions Actions
        {
            get
            {
                if (actions == null)
                {
                    actions = new Actions();
                }
                return actions;
            }
        }

        public bool Recording
        {
            get
            {
                return this.Device.RecordEnabled && !this.Device.Paused;
            }
        }

        private Device microconeDevice;
        public Device Device
        {
            get { return microconeDevice; }
        }

        internal static void NotifyDevelopers(Exception ex)
        {
            MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void AfterMainFormInitialised()
        {
            microconeDevice = new Device();
        }

        private void Application_LoadCompleted_1(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
        }

        private void Application_Exit_1(object sender, ExitEventArgs e)
        {
        }
    }
}
