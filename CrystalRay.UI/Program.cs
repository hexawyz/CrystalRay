using System;
using System.Windows.Forms;

namespace CrystalRay.UI
{
	internal static class Program
	{
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
			Application.Run(new MainForm());
		}
	}
}
