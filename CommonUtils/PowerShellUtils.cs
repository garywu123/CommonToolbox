#region License
// author:         Gary Wu
// created:        9:14 PM
// description:
#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CommonUtils
{
	public class PowerShellUtils
	{
		public static void ExecuteScript(string scriptFilePath)
		{

			if (!File.Exists(scriptFilePath))
			{
				throw new FileNotFoundException($"Unable to find the script: {scriptFilePath}");
			}

			var startInfo = new ProcessStartInfo()
			{
				FileName = "powershell.exe",
				Arguments = $"-NoProfile -ExecutionPolicy ByPass -File \"{scriptFilePath}\"",
				CreateNoWindow = false,
			};

			var process =Process.Start(startInfo);

			// process.WaitForExit();
			// var errorLevel = process.ExitCode;
			// process.Close();
		}
	}
}
