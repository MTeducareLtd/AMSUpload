using System;
using System.Windows.Forms;

namespace AMSUpload
{

	internal static class Program
	{
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(defaultValue: false);
			Application.Run(new AzureUpload());
		}
	}
}
