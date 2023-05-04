#region License
// author:         Wu, Gary
// created:        4:55 PM
// description:
#endregion

using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace CommonUtils.Test;

public class FileUtilsUnitTests
{
    private readonly ITestOutputHelper _testOutput;
    private static readonly string BeckhoffLicenseExt = "tclrs";

    private static  string? _jbtHomeFolder = null;
    private static  string? _twinCatFolder = null;

    private static  string? _routePad;
    private static  string? _vhb ;
    private static  string? _commandapo ;
    private static  string? _vehicleIdFile ;

    private static string? BeckhoffLicenseFolder;
    private static string? BeckhoffConfigXml;

    private static string[] VehicleFiles;
    private static readonly string DownloadFolder = @"ToolboxTest\FileUtilsTest\dest";

    private static readonly string VehicleFilesFolder = @"ToolboxTest\FileUtilsTest\src\Program Files\VehicleFiles\";
    private static readonly string TwinCATFolder = @"ToolboxTest\FileUtilsTest\src\TwinCAT\";

    private readonly string _runningAssemblyDir;


    public FileUtilsUnitTests(ITestOutputHelper testOutput)
    {
        _testOutput = testOutput;

        _runningAssemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        _jbtHomeFolder = Path.Combine(_runningAssemblyDir,VehicleFilesFolder);
        _twinCatFolder = Path.Combine(_runningAssemblyDir, TwinCATFolder);

        _routePad = Path.Combine(_jbtHomeFolder, "ROUTE.PAD");
        _vhb = Path.Combine(_jbtHomeFolder, "VEHICLE.VHB");
        _commandapo = Path.Combine(_jbtHomeFolder, "COMMAND.APO");
        _vehicleIdFile = Path.Combine(_jbtHomeFolder, "Vehicle.id");
        VehicleFiles = new[] { _routePad, _vhb, _commandapo, _vehicleIdFile};

        BeckhoffLicenseFolder = Path.Combine(
            _twinCatFolder ?? throw new InvalidOperationException(), "3.1\\Target\\License\\");

        BeckhoffConfigXml = Path.Combine(
            _twinCatFolder ?? throw new InvalidOperationException(), "3.1\\Boot\\CurrentConfig.xml"
        );

    }

    [Fact]
    public async Task CopyVehicleFilesAsync_SuccessfulTest()
    {
        // Arrange
        string[] srcFilePaths      = { _routePad, _vhb, _commandapo, _vehicleIdFile };
        string   destDirectoryPath = Path.Combine(_runningAssemblyDir, DownloadFolder);

        if (!Directory.Exists(destDirectoryPath) )
        {
            Directory.CreateDirectory(destDirectoryPath);
        }

        FileUtils.DeleteFilesInDirectory(destDirectoryPath);

        var progress = new Progress<int>(value => _testOutput.WriteLine($"Progress: {value}"));

        // Act
        var (filesCopied, failedFiles) = await FileUtils.CopyFilesAsync(srcFilePaths, destDirectoryPath, progress);

        // Assert
        Assert.Equal(srcFilePaths.Length, filesCopied);
        Assert.Empty(failedFiles);

        // Cleanup
        foreach (var destFilePath in Directory.GetFiles(destDirectoryPath))
        {
            File.Delete(destFilePath);
        }
        Directory.Delete(destDirectoryPath);
    }


    [Fact]
    public async Task CopyBeckhoffFilesAsync_SuccessfulTest()
    {
        // Arrange
        string   destDirectoryPath = Path.Combine(_runningAssemblyDir, DownloadFolder);

        if (!Directory.Exists(destDirectoryPath))
        {
            Directory.CreateDirectory(destDirectoryPath);
        }

        FileUtils.DeleteFilesInDirectory(destDirectoryPath);

        var progress = new Progress<int>(value => _testOutput.WriteLine($"Progress: {value}"));

        // Act
        var licenses = FileUtils.FindFilesByExtension(BeckhoffLicenseFolder, BeckhoffLicenseExt);
        var files = new List<string>(licenses)
        {
            BeckhoffConfigXml!,
        };

        files.AddRange(VehicleFiles);


        var (filesCopied, failedFiles) 
            = await FileUtils.CopyFilesAsync(files, destDirectoryPath, progress);

        // Assert
        Assert.Equal(files.Count(), filesCopied);
        Assert.Empty(failedFiles);
    }

    [Fact]
    public async Task CopyFilesAsync_NestedDirectory_Created()
    {
        // Create a temporary source directory
        string sourceFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(sourceFolder);

        // Create a temporary destination directory
        string destFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            // Create a test file in the source directory
            string testFile = Path.Combine(sourceFolder, "test.txt");
            File.WriteAllText(testFile, "This is a test file.");

            // Create a subdirectory in the destination directory
            string subDirectory = Path.Combine(destFolder, "subdirectory");

            // Try to copy the test file with the nested directory
            var (numFilesCopied, failedFiles) = await FileUtils.CopyFilesAsync(new[] { testFile }, subDirectory);

            // Check that the method created the nested directory and copied the file
            string destFile = Path.Combine(subDirectory, "test.txt");
            Assert.Equal(1, numFilesCopied);
            Assert.Empty(failedFiles);
            Assert.True(File.Exists(destFile));
        }
        finally
        {
            // Clean up the temporary directories
            Directory.Delete(sourceFolder, true);
            Directory.Delete(destFolder, true);
        }
    }


}
