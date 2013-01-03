using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.ComponentModel;
using System.Threading;
using NAudio.Wave;
using NAudio.Wave.Asio;
using MicroconeSDKDemo.DevAudio.Library;
using MicroconeSDKDemo.DevAudio.Microcone;

namespace DevAudio.Microcone
{
	public class Device
	{
       
		#region Private
		private const string csSubFolderBase = "DevAudio\\SDKDemo";
        private AsioOut asioOut = null;
		private const int ciSampleRateDefault = 48000;
        private const int ciBitDepthDefault = 16;
		private const int ciUnitFramesPerBuffer = 0x800;
		private const int ciNumberOfMicroconeSectors = 6;
		private const int ciNumberOfMicroconeAudioChannels = 8;    // Legacy devices have only 7 channels.
        private string saveBasePath;
        private string[] saveFiles;             // File names for soundFiles
        private WaveFileWriter[] soundFiles;    
		private int iNumberOfChannelsActual = 0;
        private int sampleCount = 0;
        private string csMicroconeDeviceNameKey1 = "Microcone"; // The USB device must contain this.
		private string csMicroconeDeviceNameKey2 = "ASIO";      // And this
		#endregion Private

        public Device()
		{
			this.RecordEnabled = false;
            this.Paused = false;
		}
        ~Device()
        {
            if (this.asioOut != null)
            {
                this.asioOut.Dispose();
                this.asioOut = null;
            }
        }

        /// <summary>
        /// Starts a new recording using the Microcone.
        /// Outputs 2 mix files an 6 (or 5) sector files, in wav format.
        /// </summary>
        /// <returns>True if recording started successfully.</returns>
		public bool StartRecording()
		{
			Trace.WriteLine("Start recording");
            Paused = false;
			if (this.RecordEnabled)
			{
				Debug.Print("Illegal: Record request made while recording");
				return false;
			}
			this.RecordEnabled = true;
			if (!SetupAudio())
			{
				this.RecordEnabled = false;
				return false;
			}
			createOutputFileObjects();
            sampleCount = 0;
            if (this.asioOut == null)
            {
                return false;
            }
            try
            {
                asioOut.Play();
                return true;
            }
            catch { }
            return false;
		}

		/// <summary>
		/// Stops the Microcone recording and saving to file, and closes the files.
		/// </summary>
		/// <returns>The total length of the recorded data in seconds.</returns>
		public TimeSpan Stop()
		{
			Trace.WriteLine("Stop recording");
            Paused = false;
            if (asioOut != null)
            {
                asioOut.Stop();
            }
			this.RecordEnabled = false;
			destroyOutputFileObjects();
            return outputFileTime();
		}

        /// <summary>
        /// Stops recording but doesn't destroy output files so they can be continued later.
        /// </summary>
		public void Pause()
		{   
			if (this.RecordEnabled)
			{
                Trace.WriteLine("Pausing recording");
                Paused = true;
                if (asioOut != null)
                    asioOut.Stop();
			}
			else
			{
                Paused = false;
				Debug.Print("Error: Pause requested when recording is not enabled");
			}
		}

        /// <summary>
        /// Resumes recording after a pause event.
        /// </summary>
		public void Resume()
		{
            Paused = false;
			if (this.RecordEnabled)
			{
                Trace.WriteLine("Resuming recording");
				asioOut.Play();
			}
			else
			{
				Debug.Print("Error: Resume requested when recording is not enabled");
			}
		}

        /// <summary>
        /// Makes a series of WaveFileWriter objects
        /// </summary>
		private void createOutputFileObjects()
		{   
			createSaveFileNames(true);
			// Make a new set (Use GC to tidy up any previous objects)
			soundFiles = new WaveFileWriter[ciNumberOfMicroconeAudioChannels];
			for (int i = 0; i < soundFiles.Length; i++)
			{
                soundFiles[i] = new WaveFileWriter(saveFiles[i], new WaveFormat(ciSampleRateDefault, ciBitDepthDefault, 1)); // Single channel per file
			}
		}

        /// <summary>
        /// Destroys output file objects after closing them off.
        /// </summary>
		private void destroyOutputFileObjects()
		{
            if (soundFiles != null && soundFiles.Length > 0)
            {
                // Finalise each one
                for (int i = 0; i < soundFiles.Length; i++)
                {
                    soundFiles[i].Dispose();
                    soundFiles[i] = null;
                }
                // And destroy objects
                Array.Clear(soundFiles, 0, soundFiles.Length);
            }
		}

		/// <summary>
		/// Returns the total play time of the recorded file.
		/// </summary>
		/// <returns>File play time</returns>
		/// <remarks>This should only be called when recording is stopped and during tidy-up.</remarks>
		private TimeSpan outputFileTime()
		{
            string mixFileName = saveFiles[0];
            try
            {
                WaveFileReader fileWaveStream = new WaveFileReader(mixFileName);
                return fileWaveStream.TotalTime;
            }
            catch (Exception e)
            {
                Debug.Print("Could not get total time for file {0} : {1}", mixFileName, e.Message);
            }
            return TimeSpan.Zero;
		}

		private bool SetupAudio()
		{
			if (asioOut == null)  // Lazy load
			{
				if (!Rescan())  // Look for audio devices
				{
					return false;
				}
			}
			
			return true;
		}

        public bool RecordEnabled { get; private set; }
        public bool Paused { get; private set; }

		private string currentSavePath = "";
		public string CurrentSavePath { get { return currentSavePath; } }
        /// <summary>
        /// Generates a file save path using the user's Documents directory, an app sub-directory, and the current time/date.
        /// </summary>
        /// <returns>An output path to save files into</returns>
		private string savePath()
		{
			saveBasePath = string.Format("{0}\\{1}\\{2}\\",
					Helpers.GetDocumentsDir(),
					csSubFolderBase,
					DateTime.Now.ToString("yyyyMMddHHmmss"));
			return saveBasePath;
		}

        /// <summary>
        /// Creates an array of file names used for saving audio data to.
        /// </summary>
        /// <param name="makeOutputPath">If set to <c>true</c> create folders to make theoutput path.</param>
		private void createSaveFileNames(bool makeOutputPath)
		{   
			if (saveFiles == null)
			{
				saveFiles = new string[ciNumberOfMicroconeAudioChannels];
			}
			currentSavePath = savePath();
			for (int i = 0; i < saveFiles.Length; i++)
			{
				if (i < 2)
				{
                    string mixFile = String.Format(Metadata.csMixFileFormat, i + 1, Metadata.csSaveFileExtension);
					saveFiles[i] = string.Format("{0}{1}", currentSavePath, mixFile);
				}
				else
				{
                    string channelFile = String.Format(Metadata.csChannelFileFormat, i - 1, Metadata.csSaveFileExtension);
					saveFiles[i] = string.Format("{0}{1}", currentSavePath, channelFile);
				}
			}
			if (makeOutputPath)
			{
				if (!System.IO.Directory.Exists(saveBasePath))
				{
					System.IO.Directory.CreateDirectory(saveBasePath);
				}
			}
		}

        /// <summary>
        /// Closes this device by destroying the asioOut member of this device
        /// </summary>
        /// <returns>True if successful</returns>
        public bool Close()
        {
            try
            {
                if (asioOut != null)
                {
                    asioOut.Dispose();
                    asioOut = null;
                }
            }
            catch (Exception e) 
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Rescans all compatible audio devices and creates an asioOut object when it finds one.
        /// </summary>
        /// <remarks>
        /// If more than one device is found, the first one is used.
        /// </remarks>
        /// <returns>True if a device was successfully opened.</returns>
		public bool Rescan()
		{
            String sSummary = "NAudio version: " + Assembly.GetAssembly(typeof(AsioOut)).GetName().Version.ToString();
            Debug.WriteLine(sSummary);

            // Find a list of compatible devices.  If more than 1, choose the first.
            var asioDriverNames = AsioOut.GetDriverNames().ToList().FindAll(d => d.Contains(csMicroconeDeviceNameKey1) 
                && d.Contains(csMicroconeDeviceNameKey2));
            if (asioDriverNames.Count == 0)
            {
                Debug.Print("No compatible Microcone drivers found");
                return false;
            }

            // Create device if necessary
            if (this.asioOut == null)
            {
                try
                {
                    this.asioOut = new AsioOut(asioDriverNames[0]) { InputChannelOffset = 0 };
                }
                catch (Exception e)
                {
                    Debug.Print("No Microcone devices found: {0}", e.Message);
                    return false;
                }
                int inputChannelCount = Math.Min(this.asioOut.DriverInputChannelCount, ciNumberOfMicroconeAudioChannels);
                this.asioOut.InitRecordAndPlayback(null, inputChannelCount, ciSampleRateDefault);
                this.asioOut.AudioAvailable += asioOut_AudioAvailable;
            }

			iNumberOfChannelsActual = asioOut.NumberOfInputChannels;
			return true;
		}

        public int InputChannelCount { get { return iNumberOfChannelsActual; } }

        private float[] audioBuff = new float[ciUnitFramesPerBuffer];   // Unsure about the size of this buffer. Microcone seems to use 0x800 samples.

        /// <summary>
        /// Handles the AudioAvailable event of the asioOut control.
        /// Saves audio data (stored in buffers) into WaveFileWriter objects.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="AsioAudioAvailableEventArgs" /> instance containing the event data.</param>
        private void asioOut_AudioAvailable(object sender, AsioAudioAvailableEventArgs e)
        {
            sampleCount += e.SamplesPerBuffer;
            var samples = e.InputBuffers;
            for (int iSoundFileIndex = 0; iSoundFileIndex < ciNumberOfMicroconeAudioChannels; iSoundFileIndex++)
			{
				int iInputBufferPtrIndex = iSoundFileIndex;
				if (iNumberOfChannelsActual == 7 && iSoundFileIndex >= 1)
				{   // For 7-channel (legacy) devices, duplicate buffer [0] so it exists in Sound Files [0] & [1]
					iInputBufferPtrIndex--;
				}
                Marshal.Copy((IntPtr)samples[iInputBufferPtrIndex], (float[])audioBuff, (int)0, e.SamplesPerBuffer);
				soundFiles[iSoundFileIndex].WriteSamples(audioBuff, 0, e.SamplesPerBuffer);
			}
        }
	}
}
