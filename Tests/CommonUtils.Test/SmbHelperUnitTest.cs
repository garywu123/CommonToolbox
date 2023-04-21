using EzSmb;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace CommonUtils.Test;

public class SmbHelperUnitTest
{
	#region Static Fields

	private static readonly string         ThinkpadHost = "192.168.1.16";
	private static readonly string         UserName     = "vehicleuser";
	private static readonly string         Password     = "jbtadmin789";
	private static readonly string         ThinkpadHostMovieFolder     = @"\Movies\";
	private static readonly SmbAccountInfo AccountInfo  = new(UserName, Password);

	#endregion

	#region Fields

	private readonly ITestOutputHelper _testOutputHelper;

	#endregion

	#region Constructors

	public SmbHelperUnitTest(ITestOutputHelper testOutputHelper) { _testOutputHelper = testOutputHelper; }

	#endregion

	#region Methods

	[Theory]
	[InlineData("192.168.1.16", 3)] // 3 items in this host
	public async void Test_ListSharedRootFolders(string host, int sharedRoot)
	{
		var files = Smb2ClientFileUtils.ListShares(
			await Smb2ClientFileUtils.ConnectAsync(host, AccountInfo), out var status
		);

		_testOutputHelper.WriteLine($@"{status}");

		if (host == ThinkpadHost) Assert.Equal(sharedRoot, files.Count());
	}

	[Fact]
	public async void Test_PrintFilesInFolder()
	{
		var nodes = await Smb2ClientFileUtils.GetAllItemsAsync(
			ThinkpadHost, AccountInfo, ThinkpadHostMovieFolder
		);

		foreach (var subNode in nodes)
		{
			_testOutputHelper.WriteLine(
				$"Name: {
					subNode.Name
				}, Type: {
					subNode.Type
				}, LastAccessed: {
					subNode.LastAccessed
					:yyyy-MM-dd HH:mm:ss}"
			);
		}
	}

	[Fact]
	public async void Test_DownloadFile()
	{
		var dest = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Movies.mkv";

		await Smb2ClientFileUtils.DownloadFileAsync(
			ThinkpadHost, AccountInfo,
			@"\Movies\Heat.1995.2160p.BluRay.x265\Sample\Sample-HBSM10.mkv", dest
		);
		
	}
	#endregion
}
