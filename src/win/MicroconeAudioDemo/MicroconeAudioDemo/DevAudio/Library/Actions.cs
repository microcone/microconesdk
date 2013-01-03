using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MicroconeSDKDemo.DevAudio.Microcone;
using Windows;

namespace MicroconeSDKDemo.DevAudio.Library
{
    public class Actions
    {

        public Actions()
        {
            recordButtonFlashTimer.Tick += recordButtonFlashTimer_Tick;
        }

        internal void mainFormLoaded()
        {
            thisApp = (App)Application.Current;
            setButtonStatesForMode(Metadata.RecordingMode.Stopped);
            microconePluggedIn();    // Try and make it find a MC device on startup, but this needs to be caught.
        }

        #region Private 
        private App thisApp = (App)Application.Current;
        private MainWindow mainWindow = null;
        private MainWindow MainWindow
        {
            get
            {
                if (mainWindow == null)
                {
                    mainWindow = (Application.Current as App).MainWindow as MainWindow;
                }
                return mainWindow;
            }
        }

        private void setButtonStatesForMode(Metadata.RecordingMode mode)
        {
            if (mode == Metadata.RecordingMode.Paused)
            {
                MainWindow.RecordButtonMode = Metadata.RecordButtonModes.Record;
                MainWindow.RecordButtonEnabledState = true;
                MainWindow.StopButtonEnabledState = true;
            }
            else if (mode == Metadata.RecordingMode.Recording)
            {
                MainWindow.RecordButtonMode = Metadata.RecordButtonModes.Pause;
                MainWindow.RecordButtonEnabledState = true;
                MainWindow.StopButtonEnabledState = true;
            }
            else if (mode == Metadata.RecordingMode.Stopped)
            {
                MainWindow.RecordButtonMode = Metadata.RecordButtonModes.Record;
                MainWindow.RecordButtonEnabledState = true;
                MainWindow.StopButtonEnabledState = false;
            }
            else if (mode == Metadata.RecordingMode.Busy)
            {
                MainWindow.RecordButtonEnabledState = false;
                MainWindow.StopButtonEnabledState = false;
            }
        }
        #endregion Private 

        #region Recording Control
        internal void recordPauseButtonPressed()
        {
            if (thisApp.Recording)
            {
                pauseRecordRequested();
            }
            else
            {
                recordRequested();
            }
            mainWindow.UpdateBrowseButton();
        }
        internal void stopButtonPressed()
        {
            stopRecordRequested();
            mainWindow.UpdateBrowseButton();
        }
        private void pauseRecordRequested()
        {
            setButtonStatesForMode(Metadata.RecordingMode.Busy);
            thisApp.Device.Pause();
            setButtonStatesForMode(Metadata.RecordingMode.Paused);
        }
        private void resumeRequested()
        {
            thisApp.Device.Resume();
            setButtonStatesForMode(Metadata.RecordingMode.Recording);
        }
        private void recordRequested()
        {
            setButtonStatesForMode(Metadata.RecordingMode.Busy);
            if (thisApp.Device.Paused)
            {
                resumeRequested();
            }
            else
            {
                lastRecordingBrowsePath = "";
                if (thisApp.Device.StartRecording())
                {
                    startRequested();
                }
                else
                {
                    startRequestedFailed();
                }
            }
        }
        private void stopRecordRequested()
        {
            setButtonStatesForMode(Metadata.RecordingMode.Busy);
            try
            {
                stopRecordFlashTimer();
                setButtonStatesForMode(Metadata.RecordingMode.Stopped);
                thisApp.Device.Stop();
            }
            catch { }
            lastRecordingBrowsePath = thisApp.Device.CurrentSavePath;
            mainWindow.UpdateBrowseButton(); 
        }
        private void startRequested()
        {
            setButtonStatesForMode(Metadata.RecordingMode.Recording);
            startRecordFlashTimer();
        }
        private void startRequestedFailed()
        {
            setButtonStatesForMode(Metadata.RecordingMode.Stopped);
            stopRecordFlashTimer();
            MessageBox.Show("Recording could not start\r\n(is a Microcone device connected?)");
            // TODO: Make the above error message more informative
        }
        #endregion Recording Control

        #region Flash Buttons While Recording
        private DispatcherTimer recordButtonFlashTimer = new DispatcherTimer();
        private int ciRecordingFlashPeriodms = 750;
        private bool recordFlashState = false;
        private void startRecordFlashTimer()
        {
            recordFlashState = true;
            recordButtonFlashTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);  // ms; trip quickly the first time
            recordButtonFlashTimer.Start();
        }

        void recordButtonFlashTimer_Tick(object sender, EventArgs e)
        {
            recordButtonFlashTimer.Interval = new TimeSpan(0, 0, 0, 0, ciRecordingFlashPeriodms / 2);  // ms, 50% duty cycle
            MainWindow.SetRecordFlashState(recordFlashState);
            recordFlashState = !recordFlashState;
        }
        private void stopRecordFlashTimer()
        {
            MainWindow.SetRecordFlashState(false);
            recordButtonFlashTimer.Stop();
        }
        #endregion Flash Buttons While Recording

        internal void browseLastFolder()
        {
            if (CanBrowse)
            {
                try
                {
                    Process.Start(lastRecordingBrowsePath);
                }
                catch (Exception e)
                {
                    MessageBox.Show(String.Format("Could not browse path.\nPath: {0}\nReason: {1}", lastRecordingBrowsePath, e.Message));
                }
            }
        }

        private string lastRecordingBrowsePath = "";
        public bool CanBrowse
        {
            get 
            {
                return !String.IsNullOrEmpty(lastRecordingBrowsePath);
            }
        }

        internal void microconeUnplugged()
        {
            thisApp.Device.Close();
        }

        internal void microconePluggedIn()
        {
            thisApp.Device.Rescan();
        }
    }
}
