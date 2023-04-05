#region License
// author:         Wu, Gary
// created:        12:09 PM
// description:
#endregion

using Xunit;
using Xunit.Abstractions;

namespace CommonUtils.Test;

public class SshHelperUnitTest
{
	private readonly ITestOutputHelper _testOutputHelper;
	private static readonly string host = "192.168.0.10";
	private static readonly string user = "root";
	private static readonly string password = "navitrol";

	public SshHelperUnitTest(ITestOutputHelper testOutputHelper)
	{
		_testOutputHelper = testOutputHelper;
	}

	[Fact]
	public async void Test_ExecuteCommand_PrintCommandResult()
	{
		var command = "find /var/cache/apt/archives/. -type f ! -name 'lock' -delete";

		var client = SshUtils.InitialSshClient(host, 22, user, password);
		var result = SshUtils.ExecuteCommand(client, command);

		_testOutputHelper.WriteLine($@"{result.Result}");
	}

	[Fact]
	public async void Test_GetConnectInfo()
	{
		var client = SshUtils.InitialSshClient(host, 22, user, password);
		_testOutputHelper.WriteLine($@"{SshUtils.GetHostKey()}");
		_testOutputHelper.WriteLine($@"{SshUtils.GetHostKeyName()}");
		_testOutputHelper.WriteLine($@"{SshUtils.GetFingerprint()}");
	}

}
