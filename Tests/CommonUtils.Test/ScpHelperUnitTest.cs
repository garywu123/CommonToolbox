﻿#region License

// author:         Wu, Gary
// created:        2:34 PM
// description:

#endregion

using CommonUtils.Test.TestData;
using Renci.SshNet.Common;
using Xunit;
using Xunit.Abstractions;

namespace CommonUtils.Test;

[Collection("NavitecTest")]
public class ScpHelperUnitTest
{
    #region Fields

    private readonly ITestOutputHelper        _testOutputHelper;
    private readonly NavitecTestSharedFixture _navitecTestCollection;

    #endregion

    #region Constructors

    public ScpHelperUnitTest(ITestOutputHelper        testOutputHelper,
                             NavitecTestSharedFixture navitecTestCollection)
    {
        _testOutputHelper = testOutputHelper;
        _navitecTestCollection = navitecTestCollection;
    }

    #endregion

    #region Methods

    [Theory]
    [InlineData(
        "192.168.0.10", 22, "root", "navitrol", "/home/navitec",
        "nte"
    )]
    public async Task FindRemoteFilesByExtensionAsync_Should_Return_File_Names(
        string host, int port, string user, string password, string remoteDir,
        string extension)
    {
        // Arrange

        // Act
        var fileNames = await ScpUtils.FindRemoteFilesByExtensionAsync(
            host, port, user, password, remoteDir,
            extension
        );

        // Assert
        Assert.NotNull(fileNames);
        Assert.NotEmpty(fileNames);
        Assert.All(fileNames, fileName => Assert.EndsWith($".{extension}", fileName));
    }

    [Theory]
    [InlineData("/etc/network/interfaces", "ToolboxTest/ScpHelperTest/DownloadFile/", true)]
    public async void Test_DownloadFile(string remoteFile, string localFolder, bool expectResult)
    {
        var runningFolder = Path.Combine(FileUtils.GetRunningAssemblyFolder(), localFolder);
        FileUtils.DeleteFilesInDirectory(runningFolder);

        var fileName = Path.GetFileName(remoteFile);
        var local    = Path.Combine(runningFolder, fileName);

        var result = await ScpUtils.DownloadFileAsync(
            _navitecTestCollection.Host, _navitecTestCollection.Port, _navitecTestCollection.User,
            _navitecTestCollection.Password, remoteFile,
            local, true
        );

        Assert.Equal(expectResult, result);
    }


    [Theory]
    [InlineData("/etc/network/interface", "ToolboxTest/ScpHelperTest/DownloadFile/", false)]
    public async void Test_DownloadFile_NoSuchFileException(string remoteFile, string localFolder,
                                                            bool   expectResult)
    {
        var runningFolder = Path.Combine(FileUtils.GetRunningAssemblyFolder(), localFolder);
        FileUtils.DeleteFilesInDirectory(runningFolder);

        var fileName = Path.GetFileName(remoteFile);
        var local    = Path.Combine(runningFolder, fileName);

        try
        {
            await ScpUtils.DownloadFileAsync(
                _navitecTestCollection.Host, _navitecTestCollection.Port,
                _navitecTestCollection.User,
                _navitecTestCollection.Password, remoteFile,
                local, true
            );

            // If the previous line didn't throw an exception, the test failed
            Assert.True(false, "Expected ScpException was not thrown.");
        }
        catch (ScpException ex)
        {
            // Handle the exception here if needed
            // You can also assert on the exception message or other properties
            Assert.Contains("No such file or directory", ex.Message);
        }
    }


    [Theory]
    [InlineData(
        new[]
        {
            "/etc/network/interfaces",
            "/home/navitec/license.txt",
            "/home/navitec/licensekey.txt"
        }, "ToolboxTest/ScpHelperTest/DownloadFile/"
    )]
    public async Task Test_DownloadFilesAsync(string[] remoteFiles, string localFolder)
    {
        // Arrange
        var localDir = Path.Combine(FileUtils.GetRunningAssemblyFolder(), localFolder);
        FileUtils.DeleteFilesInDirectory(localDir);

        // Act
        var result = await ScpUtils.DownloadFilesAsync(
            _navitecTestCollection.Host,
            _navitecTestCollection.Port,
            _navitecTestCollection.User,
            _navitecTestCollection.Password,
            remoteFiles,
            localDir, new Progress<int>(ReportFilesCopied)
        );

        // Assert
        Assert.Equal(remoteFiles.Length, result.Item1);
        Assert.Empty(result.Item2);
    }


    [Fact]
    public async Task Test_UploadFilesAsync_Success()
    {
        var localFiles = new[]
        {
            "ToolboxTest/ScpHelperTest/UploadFiles/license.txt",
            "ToolboxTest/ScpHelperTest/UploadFiles/licensekey.txt",
            "ToolboxTest/ScpHelperTest/UploadFiles/env_Richwood UPS_generic_floor0_ver4.nte"
        };

        var remoteDir = _navitecTestCollection.NavitecHomePath;

        var progress = new Progress<int>(x => _testOutputHelper.WriteLine($"Uploaded {x} files."));

        var result = await ScpUtils.UploadFilesAsync(
            _navitecTestCollection.Host, _navitecTestCollection.Port, _navitecTestCollection.User,
            _navitecTestCollection.Password, localFiles, remoteDir, progress
        );

        Assert.Equal(localFiles.Length, result.Item1);
        Assert.Empty(result.Item2);
    }

    private void ReportFilesCopied(int obj)
    {
        _testOutputHelper.WriteLine($@"File downloaded: {obj}");
    }

    #endregion
}
