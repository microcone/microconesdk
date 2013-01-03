using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DevAudio.Library;
using DevAudio.Microcone;
using MicroconeSDKDemo;
using MicroconeSDKDemo.DevAudio.Library;
using MicroconeSDKDemo.DevAudio.Microcone;

namespace Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Properties
        private App thisApp = (App)Application.Current;
        Actions actions = null;
        MicroconeCallbackFunction _callback;
        int _clientId = -1;
        float masterGain = 1f;
        bool recording = false;
        #endregion Properties

        #region Window Events
        public MainWindow()
        {
            InitializeComponent();
            browseButton.Visibility = Visibility.Hidden;
            actions = thisApp.Actions;
            _callback = new MicroconeCallbackFunction(ActivityCallback);
            InitClient();
            UpdateSettings();
            Loaded += MainWindow_Loaded;

            thisApp.AfterMainFormInitialised();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            actions.mainFormLoaded();
        }

        void Refresh()
        {
            InvalidateVisual();
            Dispatcher.Invoke(new Action(() => { }), System.Windows.Threading.DispatcherPriority.Render);
        }
        protected override void OnActivated(EventArgs e)
        {
            Refresh();
            base.OnActivated(e);
        }
        #endregion Window Events

        #region Microcone Setup
        private void InitClient()
        {
            try
            {
                _clientId = MicroconeAPI.InitClientConnection(_callback);
                if (_clientId > 0)
                {
                    MicroconeAPI.SetDspEnabled(_clientId, enabled: 1);
                    MicroconeAPI.SetDoStereo(_clientId, doStereo: 0);
                    MicroconeAPI.SetEnabled(_clientId, sectorEnabled: new[] { 1, 1, 1, 1, 1, 1 });
                    MicroconeAPI.SetGain(_clientId, sectorGain: new[] { 1f, 1f, 1f, 1f, 1f, 1f });
                }
            }
            catch { }
        }
        void UpdateSettings()
        {
            var dspEnabled = 0;
            var stereoEnabled = 0;
            var sectorEnabled = new int[6];
            var sectorGain = new[] { 1f, 1f, 1f, 1f, 1f, 1f };
            if (_clientId > 0)
            {
                MicroconeAPI.GetDspEnabled(_clientId, ref dspEnabled);
                MicroconeAPI.GetDoStereo(_clientId, ref stereoEnabled);
                MicroconeAPI.GetEnabled(_clientId, sectorEnabled);
                MicroconeAPI.GetGain(_clientId, sectorGain);
            }

            this.Dispatcher.Invoke(new Action(() =>
            {
                var plugged = !_unplugged && _clientId > 0;
                sliderMaster.IsEnabled = Reset.IsEnabled = DSPEnabled.IsEnabled = StereoEnabled.IsEnabled = plugged;
                Enabled1.IsEnabled = Enabled2.IsEnabled = Enabled3.IsEnabled =
                    Enabled4.IsEnabled = Enabled5.IsEnabled = Enabled6.IsEnabled = plugged;

                string appTitle = (string)Resources["AppTitle"];
                Title = plugged ? appTitle : appTitle + " [Unplugged]";

                DSPEnabled.IsChecked = (dspEnabled == 1);
                StereoEnabled.IsChecked = (stereoEnabled == 1);
                UpdateSectorEnabled(sectorEnabled);
                recordButton.IsEnabled = plugged;
                stopButton.IsEnabled = recording;
                UpdateSliders(masterGain, sectorGain);
            }));
        }
        void ActivityCallback(IntPtr sectorActivityPtr, IntPtr sectorLocationPtr)
        {
            var sectorActivity = MicroconeAPI.ConvertSectorActivity(sectorActivityPtr);
            var sectorLocation = MicroconeAPI.ConvertSectorLocation(sectorLocationPtr);
            this.Dispatcher.Invoke(new Action(() =>
            {
                SectorView1.SetLocation(sectorActivity, sectorLocation);
            }));
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            GlassHelper.ExtendGlass(this, this.Grid.Margin);
            Refresh();
            var hwndSource = PresentationSource.FromVisual(this) as System.Windows.Interop.HwndSource;
            if (hwndSource != null)
                hwndSource.AddHook(HwndSourceHook);
            MicroconeAPI.RegisterPnpNotification(hwndSource.Handle);
        }
        public IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            try
            {
                switch (msg)
                {
                    case MicroconeAPI.WM_PNP_NOTIFICATION:
                        _unplugged = (wParam.ToInt32() == 2);
                        if (_unplugged)
                        {
                            MicroconeAPI.CloseClientConnection(_clientId);
                            _clientId = -1;
                            actions.microconeUnplugged();
                        }
                        else
                        {
                            InitClient();
                            actions.microconePluggedIn();
                        }
                        UpdateSettings();
                        break;
                }
            }
            catch (Exception e)
            {
                App.NotifyDevelopers(e);
            }
            return IntPtr.Zero;
        }
        bool _unplugged;

        #endregion Microcone Setup

        #region Microcone Control
        protected override void OnClosed(EventArgs e)
        {
            _callback = null;
            if (_clientId > 0)
            {
                MicroconeAPI.CloseClientConnection(_clientId);
            }
            MicroconeAPI.UnregisterPnpNotification();
            base.OnClosed(e);
            System.Environment.Exit(0);
        }

        private void Button_Click_DSP(object sender, RoutedEventArgs e)
        {
            if (!DSPEnabled.IsChecked.Value) // is checked has already been updated so check for false
            {
                var result = MessageBox.Show("This will turn off the Microcone signal processing. Only do this if you want to access the raw microphone signals.\n\nDo you wish to proceed?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                if (result == MessageBoxResult.No)
                {
                    DSPEnabled.IsChecked = true;
                    return;
                }
            }
            try
            {
                if (_clientId > 0)
                {
                    MicroconeAPI.SetDspEnabled(_clientId, DSPEnabled.IsChecked.Value ? 1 : 0);
                }
                UpdateSettings();
            }
            catch { }
        }
        private void Button_Click_Stereo(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_clientId > 0)
                {
                    MicroconeAPI.SetDoStereo(_clientId, StereoEnabled.IsChecked.Value ? 1 : 0);
                }
                UpdateSettings();
            }
            catch { }
        }
        private void Button_Click_Reset(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_clientId > 0)
                {
                    var sectorEnabled = new[] { 1, 1, 1, 1, 1, 1, };
                    MicroconeAPI.SetEnabled(_clientId, sectorEnabled);

                    var sectorGain = new[] { 1f, 1f, 1f, 1f, 1f, 1f };
                    MicroconeAPI.SetGain(_clientId, sectorGain);
                    sliderMaster.Value = sliderMaster.Maximum / 2;
                }
                UpdateSettings();
            }
            catch { }
        }
        private void Button_Click_Enabled(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_clientId > 0)
                {
                    var sectorEnabled = new[]{
						Enabled1.IsChecked.Value ? 1 : 0,
						Enabled2.IsChecked.Value ? 1 : 0,
						Enabled3.IsChecked.Value ? 1 : 0,
						Enabled4.IsChecked.Value ? 1 : 0,
						Enabled5.IsChecked.Value ? 1 : 0,
						Enabled6.IsChecked.Value ? 1 : 0,
					};
                    MicroconeAPI.SetEnabled(_clientId, sectorEnabled);
                }
                UpdateSettings();
            }
            catch { }
        }
        #region Volume Sliders
        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SliderChange();
        }
        private void sliderMaster_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SliderChange();
        }

        private void SliderChange()
        {
            try
            {
                if (_clientId > 0)
                {
                    var sliderValues = new[]{
						slider1.Value,
						slider2.Value,
						slider3.Value,
						slider4.Value,
						slider5.Value,
						slider6.Value,
					};
                    masterGain = Scale.SliderToGain(sliderMaster.Value);
                    var sectorGain = sliderValues.Select(value => Scale.SliderToGain(value) * masterGain).ToArray();
                    MicroconeAPI.SetGain(_clientId, sectorGain);
                }
                UpdateSettings();
            }
            catch { }
        }

        private void UpdateSliders(float masterGain, float[] sectorGain)
        {
            sliderMaster.Value = Scale.GainToSlider(masterGain);
            slider1.Value = Scale.GainToSlider(sectorGain[0] / masterGain);
            slider2.Value = Scale.GainToSlider(sectorGain[1] / masterGain);
            slider3.Value = Scale.GainToSlider(sectorGain[2] / masterGain);
            slider4.Value = Scale.GainToSlider(sectorGain[3] / masterGain);
            slider5.Value = Scale.GainToSlider(sectorGain[4] / masterGain);
            slider6.Value = Scale.GainToSlider(sectorGain[5] / masterGain);
        }
        #endregion Volume Sliders

        private void UpdateSectorEnabled(int[] sectorEnabled)
        {
            Enabled1.IsChecked = (sectorEnabled[0] == 1);
            Enabled2.IsChecked = (sectorEnabled[1] == 1);
            Enabled3.IsChecked = (sectorEnabled[2] == 1);
            Enabled4.IsChecked = (sectorEnabled[3] == 1);
            Enabled5.IsChecked = (sectorEnabled[4] == 1);
            Enabled6.IsChecked = (sectorEnabled[5] == 1);
            UpdateSliderEnabled();
            SectorView1.SetEnabled(sectorEnabled);
        }
        private void UpdateSliderEnabled()
        {
            slider1.IsEnabled = Enabled1.IsChecked.Value;
            slider2.IsEnabled = Enabled2.IsChecked.Value;
            slider3.IsEnabled = Enabled3.IsChecked.Value;
            slider4.IsEnabled = Enabled4.IsChecked.Value;
            slider5.IsEnabled = Enabled5.IsChecked.Value;
            slider6.IsEnabled = Enabled6.IsChecked.Value;
        }

        private void Button_Click_Record(object sender, RoutedEventArgs e)
        {
            actions.recordPauseButtonPressed();
        }

        internal void UpdateBrowseButton()
        {
            browseButton.Visibility = actions.CanBrowse ? Visibility.Visible : Visibility.Hidden;
        }
        private void Button_Click_Stop(object sender, RoutedEventArgs e)
        {
            actions.stopButtonPressed();
        }
        private void browseButton_Click(object sender, RoutedEventArgs e)
        {
            actions.browseLastFolder();
        }
        #endregion Microcone Control

        #region WindowStates
        private bool recordButtonEnabledState = true;
        public bool RecordButtonEnabledState
        {
            get { return recordButtonEnabledState; }
            set
            {
                recordButtonEnabledState = value;
                recordButton.IsEnabled = value;
            }
        }
        private bool stopButtonEnabledState = true;
        public bool StopButtonEnabledState
        {
            get { return stopButtonEnabledState; }
            set
            {
                stopButtonEnabledState = value;
                stopButton.IsEnabled = value;
            }
        }

        private Metadata.RecordButtonModes recordButtonMode;
        public Metadata.RecordButtonModes RecordButtonMode
        {
            get { return recordButtonMode; }
            set
            {
                recordButtonMode = value;
                String imageName;
                if (value == Metadata.RecordButtonModes.Record)
                {
                    imageName = recordFlashState ? "RecordBlue48.png" : "Record48.png";
                }
                else
                {
                    imageName = recordFlashState ? "PauseBlue48.png" : "Pause48.png";
                }
                Uri path = new Uri(Path.Combine(Metadata.ImagesBasePath, imageName), UriKind.Absolute);
                BitmapImage bitmap = new BitmapImage(path);
                ImageBrush brush = new ImageBrush(bitmap);
                brush.Stretch = Stretch.None;
                brush.TileMode = TileMode.None;
                this.recordButton.Background = brush;
            }
        }
        #endregion WindowStates

        #region Flashing While Recording
        private bool recordFlashState = false;
        public void SetRecordFlashState(bool flashState)
        {
            this.recordFlashState = flashState;
            RecordButtonMode = recordButtonMode;    // Force a redraw
        }
        #endregion // Flashing While Recording


    }
}
