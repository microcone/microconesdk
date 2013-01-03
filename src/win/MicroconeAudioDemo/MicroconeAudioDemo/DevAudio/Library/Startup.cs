using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using MicroconeSDKDemo;

namespace DevAudio.Library
{
	public partial class Startup
	{		
		[STAThread]
		public static void Main()
		{
			try
			{
				if (System.Environment.GetCommandLineArgs().Contains("-exitall"))
				{
					// close running instances
					var current = Process.GetCurrentProcess();
					foreach (var process in Process.GetProcessesByName(current.ProcessName).Where(_ => _.Id != current.Id))
					{
						Native.SendMessage(process.MainWindowHandle.ToInt32(), Native.WM_SYSCOMMAND, Native.SC_CLOSE, 0);
					}
				}
				else
				{
					var createdNew = true;
					using (var mutex = new Mutex(true, "Local\\DevAudio.Microcone.Demo", out createdNew))
					{
						if (createdNew)
						{
							var app = new App();
							app.InitializeComponent();
                            app.Run();
						}
						else
						{
							var current = Process.GetCurrentProcess();
							foreach (var process in Process.GetProcessesByName(current.ProcessName).Where(_ => _.Id != current.Id))
							{
								Native.SetForegroundWindow(process.MainWindowHandle);
								Native.ShowWindowAsync(process.MainWindowHandle, Native.SW_RESTORE);
								break;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				App.NotifyDevelopers(ex);
			}
		}
	}
}
