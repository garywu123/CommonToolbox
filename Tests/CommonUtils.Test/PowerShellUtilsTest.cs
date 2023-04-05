#region License
// author:         Wu, Gary
// created:        4:09 PM
// description:
#endregion

using Xunit;

namespace CommonUtils.Test;

public class PowerShellUtilsTest
{
	[Fact]
	public void Test_ExecutePowerShellScript()
	{
		string scriptPath = FileUtils.GetRunningAssemblyFolder() + "\\Resources\\TestScript.ps1";

		PowerShellUtils.ExecuteScript(scriptPath);
	}
}
