using System;
using System.Runtime.InteropServices;

namespace DevAudio.Microcone
{
	//typedef void (MicroconeCallbackFunction)(int* sectorActivity, float* sectorLocation);
	public delegate void MicroconeCallbackFunction(System.IntPtr sectorActivity, System.IntPtr sectorLocation);
	public static class MicroconeAPI
	{
		const string __microconeApiDll = "MicroconeAPI.dll";
		const CallingConvention __call = CallingConvention.Cdecl;

		public static int[] ConvertSectorActivity(System.IntPtr sectorActivityPtr)
		{
			var array = new int[6];
			System.Runtime.InteropServices.Marshal.Copy(sectorActivityPtr, array, 0, 6);
			return array;
		}

		public static float[] ConvertSectorLocation(System.IntPtr sectorLocationPtr)
		{
			var array = new float[6];
			System.Runtime.InteropServices.Marshal.Copy(sectorLocationPtr, array, 0, 6);
			return array;
		}

		//SInt32 InitClientConnection(MicroconeCallbackFunction* callback);
		[DllImport(__microconeApiDll, CallingConvention = __call)]
		public static extern int InitClientConnection(MicroconeCallbackFunction callback);

		//void CloseClientConnection(SInt32 clientId);
		[DllImport(__microconeApiDll, CallingConvention = __call)]
		public static extern void CloseClientConnection(int clientId);

		//SInt32 SetDoStereo(SInt32 clientId, int doStereo);
		[DllImport(__microconeApiDll, CallingConvention = __call)]
		public static extern int SetDoStereo(int clientId, [In]int doStereo);

		//SInt32 GetDoStereo(SInt32 clientId, int* doStereo);
		[DllImport(__microconeApiDll, CallingConvention = __call)]
		public static extern int GetDoStereo(int clientId, [In][Out]ref int doStereo);

		//SInt32 SetGain(SInt32 clientId, float* sectorGain);
		[DllImport(__microconeApiDll, CallingConvention = __call)]
		public static extern int SetGain(int clientId, [In]float[] sectorGain);

		//SInt32 GetGain(SInt32 clientId, float* sectorGain);
		[DllImport(__microconeApiDll, CallingConvention = __call)]
		public static extern int GetGain(int clientId, [In][Out]float[] sectorGain);

		//SInt32 SetEnabled(SInt32 clientId, int* sectorEnabled);
		[DllImport(__microconeApiDll, CallingConvention = __call)]
		public static extern int SetEnabled(int clientId, [In]int[] sectorEnabled);

		//SInt32 GetEnabled(SInt32 clientId, int* sectorEnabled);
		[DllImport(__microconeApiDll, CallingConvention = __call)]
		public static extern int GetEnabled(int clientId, [In][Out]int[] sectorEnabled);

		//SInt32 SetDspEnabled(SInt32 clientId, int enabled);
		[DllImport(__microconeApiDll, CallingConvention = __call)]
		public static extern int SetDspEnabled(int clientId, [In]int enabled);

		//SInt32 GetDspEnabled(SInt32 clientId, int* enabled);
		[DllImport(__microconeApiDll, CallingConvention = __call)]
		public static extern int GetDspEnabled(int clientId, [In][Out]ref int enabled);

		public const int WM_USER = 0x400;
		public const int WM_PNP_NOTIFICATION = (WM_USER + 101);
		//SInt32 RegisterPnpNotification(HANDLE hWnd);
		[DllImport(__microconeApiDll, CallingConvention = __call)]
		public static extern int RegisterPnpNotification([In]IntPtr hWnd);

		[DllImport(__microconeApiDll, CallingConvention = __call)]
		public static extern void UnregisterPnpNotification();
	}
}
