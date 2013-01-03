using System;
using System.Runtime.InteropServices;

namespace DevAudio.Library
{
	static class Native
	{
		/// <summary>
		/// Brings the thread that created the specified window into the foreground and activates the window
		/// </summary>
		/// <param name="wndHandle">A handle to the window</param>
		/// <returns>If the window was brought to the foreground return true, otherwise false</returns>
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetForegroundWindow(IntPtr wndHandle);

		/// <summary>
		/// Sets the show state of a window created by a different thread.
		/// </summary>
		/// <param name="wndHandle">A handle to the window</param>
		/// <param name="cmdShowNumber">int indicating how window should be shown</param>
		/// <returns>If the window was previously visible return true otherwise false</returns>
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ShowWindowAsync(IntPtr wndHandle, uint nCmdShow);

		[DllImport("user32.dll")]
		public static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);

		public const int WM_SYSCOMMAND = 0x0112;
		public const int SC_CLOSE = 0xF060;
		public const uint SW_RESTORE = 9;
	}
}
