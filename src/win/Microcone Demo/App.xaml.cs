using System;
using System.Windows;

namespace DevAudio.Microcone.Settings
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		internal static void NotifyDevelopers(Exception ex)
		{
			MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}
}
