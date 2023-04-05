#region License
// author:         Wu, Gary
// created:        2:34 PM
// description:
#endregion

using Xunit;
using Xunit.Abstractions;

namespace CommonUtils.Test;

public class ScpHelperUnitTest
{
	private readonly        ITestOutputHelper _testOutputHelper;
	private static readonly string            host     = "192.168.0.10";
	private static readonly int            port     = 22;
	private static readonly string            user     = "root";
	private static readonly string            password = "navitrol";

	public ScpHelperUnitTest() { }

	[Theory]
	[InlineData("/etc/network/interfaces", "ToolboxTest/ScpHelperTest/DownloadFile")]
	public async void Test_DownloadFile(string remoteFile, string localFolder)
	{
		var runningFolder = Path.Combine(FileUtils.GetRunningAssemblyFolder(), localFolder);
		FileUtils.DeleteAll(runningFolder);

		var fileName = Path.GetFileName(remoteFile);
		var local = Path.Combine(runningFolder, fileName);
		
		await ScpUtils.DownloadFileAsync(
			host, port, user, password, remoteFile,
			local, true
		);
	}

}

