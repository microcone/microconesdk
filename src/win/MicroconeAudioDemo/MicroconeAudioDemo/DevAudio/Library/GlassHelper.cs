using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace DevAudio.Library
{
    /// <summary>
    /// Helper class to extend the glass region at the top of a window into the client area
    /// </summary>
    /// <remarks>
    /// Thanks largely to http://wpftutorial.net/ExtendGlass.html
    /// </remarks>
	class GlassHelper
	{
		[StructLayout(LayoutKind.Sequential)]
		struct MARGINS
		{
			public int cxLeftWidth;
			public int cxRightWidth;
			public int cyTopHeight;
			public int cyBottomHeight;
		}
		
		public const int WM_DWMCOMPOSITIONCHANGED = 0x031E;

		[DllImport("dwmapi.dll")]
		static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

		[DllImport("dwmapi.dll")]
		extern static int DwmIsCompositionEnabled(ref int en);

		public static void ExtendGlass(Window window, Thickness thikness)
		{
			try
			{
				var isGlassEnabled = 0;
				DwmIsCompositionEnabled(ref isGlassEnabled);
				if (Environment.OSVersion.Version.Major > 5 && isGlassEnabled > 0)
				{
					// Get the window handle
					var helper = new WindowInteropHelper(window);
					var source = HwndSource.FromHwnd(helper.Handle);
					source.AddHook(new HwndSourceHook(
						delegate(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
						{
							if (msg == WM_DWMCOMPOSITIONCHANGED)
							{
								ExtendGlass(window, thikness);
								handled = true;
							}
							return IntPtr.Zero;
						}));
					source.CompositionTarget.BackgroundColor = Colors.Transparent;
					window.Background = Brushes.Transparent;

					// Get the dpi of the screen
					var desktop = System.Drawing.Graphics.FromHwnd(source.Handle);
					var dpiX = desktop.DpiX / 96;
					var dpiY = desktop.DpiY / 96;

					// Set Margins
					var margins = new MARGINS()
					{
						cxLeftWidth = (int)(thikness.Left * dpiX),
						cxRightWidth = (int)(thikness.Right * dpiX),
						cyBottomHeight = (int)(thikness.Bottom * dpiY),
						cyTopHeight = (int)(thikness.Top * dpiY),
					};
					//var hr = 
					DwmExtendFrameIntoClientArea(source.Handle, ref margins);
				}
				else
				{
					window.Background = SystemColors.WindowBrush;
				}
			}
			catch (DllNotFoundException) { }
		}
	}
}
