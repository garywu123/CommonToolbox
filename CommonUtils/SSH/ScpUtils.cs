#region License

// author:         Wu, Gary
// created:        1:43 PM
// description:

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Renci.SshNet;

namespace CommonUtils;

public static class ScpUtils
{
    #region Static Methods

    /// <summary>
    ///     Download the file.
    /// </summary>
    /// <param name="host">host address</param>
    /// <param name="port">port</param>
    /// <param name="user">user</param>
    /// <param name="password">password</param>
    /// <param name="remoteFilePath">remote file path</param>
    /// <param name="localFilePath">local file file path</param>
    /// <param name="createDir">
    ///     Create dir for the local file path if the directory of the local file
    ///     doesn't exist.
    /// </param>
    /// <returns>return true for download file success.</returns>
    public static async Task<bool> DownloadFileAsync(string host, int port, string user,
                                                     string password,
                                                     string remoteFilePath, string localFilePath,
                                                     bool   createDir = false)
    {
        FileUtils.TryParseFilePath(localFilePath, createDir);

        using var client =
            await Task.Run(() => SshUtils.InitialSshClient(host, port, user, password));

        if (!client.IsConnected) client.Connect();

        using var scp = new ScpClient(client.ConnectionInfo);
        scp.Connect();
        await Task.Run(() => scp.Download(remoteFilePath, new FileInfo(localFilePath)));
        return true;
    }

    /// <summary>
    ///     Download the file.
    /// </summary>
    /// <param name="host">host address</param>
    /// <param name="port">port</param>
    /// <param name="user">user</param>
    /// <param name="password">password</param>
    /// <param name="remoteFiles">a list of remote file path</param>
    /// <param name="localDir">local file file path</param>
    /// <returns>return true for download file success.</returns>
    public static async Task DownloadFilesAsync(string   host, int port, string user,
                                                string   password,
                                                string[] remoteFiles,
                                                string   localDir)
    {
        if (!Directory.Exists(localDir)) Directory.CreateDirectory(localDir);

        using var client =
            await Task.Run(() => SshUtils.InitialSshClient(host, port, user, password));

        if (!client.IsConnected) client.Connect();
        using var scp = new ScpClient(client.ConnectionInfo);

        foreach (var remoteFile in remoteFiles)
        {
            var fileName      = Path.GetFileName(remoteFile);
            var localFilePath = Path.Combine(localDir, fileName);
            if (!scp.IsConnected)
            {
                scp.Connect();
            }
            await Task.Run(() => scp.Download(remoteFile, new FileInfo(localFilePath)));
        }
    }

    /// <summary>
    ///     Download the files.
    /// </summary>
    /// <param name="host">host address</param>
    /// <param name="port">port</param>
    /// <param name="user">user</param>
    /// <param name="password">password</param>
    /// <param name="remoteFiles">a list of remote file path</param>
    /// <param name="localDir">local file file path</param>
    /// <param name="progress">report progress handler</param>
    /// <returns>return true for download file success.</returns>
    public static async Task<(int, List<(string, string)>)> DownloadFilesAsync(
        string         host,        int    port, string user, string password,
        string[]       remoteFiles, string localDir,
        IProgress<int> progress = null)
    {
        var filesDownloaded = 0;

        if (!Directory.Exists(localDir)) Directory.CreateDirectory(localDir);

        using var client =
            await Task.Run(() => SshUtils.InitialSshClient(host, port, user, password));

        if (!client.IsConnected) client.Connect();

        using var scp         = new ScpClient(client.ConnectionInfo);
        var       failedFiles = new List<(string, string)>();

        foreach (var remoteFile in remoteFiles)
        {
            var fileName      = Path.GetFileName(remoteFile);
            var localFilePath = Path.Combine(localDir, fileName);
            try
            {
                if (!scp.IsConnected) scp.Connect();
                var file = remoteFile;
                await Task.Run(() => scp.Download(file, new FileInfo(localFilePath)));
                filesDownloaded++;
                progress?.Report(filesDownloaded);
            }
            catch (Exception ex) { failedFiles?.Add((remoteFile, ex.Message)); }
        }

        return (filesDownloaded, failedFiles);
    }


    public static async Task DownloadFolderAsync(string host, int port, string user,
                                                 string password,
                                                 string remoteFolderPath,
                                                 string localFolderPath)
    {
        using var client =
            await Task.Run(() => SshUtils.InitialSshClient(host, port, user, password));

        if (!client.IsConnected) client.Connect();
        using var scp = new ScpClient(client.ConnectionInfo);
        scp.Connect();
        await Task.Run(
            () => scp.Download(remoteFolderPath, new DirectoryInfo(localFolderPath))
        );
    }


    public static async Task<string[]> FindRemoteFilesByExtensionAsync(
        string host,      int    port, string user, string password,
        string remoteDir, string extension)
    {
        var cmd = $"find {remoteDir} -name '*.{extension}'";

        var output = await SshUtils.ExecuteCommand(host, port, user, password, cmd);

        var fileNames = output.Split(
            new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries
        );

        return fileNames.Select(fileName => fileName.Trim()).ToArray();
    }


    public static async Task UploadFileAsync(string host, int port, string user,
                                             string password,
                                             string localFilePath, string remoteFilePath)
    {
        using var client =
            await Task.Run(() => SshUtils.InitialSshClient(host, port, user, password));

        if (!client.IsConnected) client.Connect();

        using var scp = new ScpClient(client.ConnectionInfo);
        scp.Connect();

        await Task.Run(() => scp.Upload(new FileInfo(localFilePath), remoteFilePath));
    }


    public static async Task<(int, List<(string, string)>)> UploadFilesAsync(
        string   host,       int    port,      string         user, string password,
        string[] localFiles, string remoteDir, IProgress<int> progress = null)
    {
        var filesUploaded = 0;

        using var client =
            await Task.Run(() => SshUtils.InitialSshClient(host, port, user, password));

        if (!client.IsConnected) client.Connect();

        using var scp         = new ScpClient(client.ConnectionInfo);
        var       failedFiles = new List<(string, string)>();

        foreach (var localFile in localFiles)
        {
            var fileName       = Path.GetFileName(localFile);
            var remoteFilePath = $"{remoteDir}/{fileName}";
            try
            {
                if (!scp.IsConnected) scp.Connect();
                await Task.Run(() => scp.Upload(new FileInfo(localFile), remoteFilePath));
                filesUploaded++;
                progress?.Report(filesUploaded);
            }
            catch (Exception ex) { failedFiles?.Add((localFile, ex.Message)); }
        }

        return (filesUploaded, failedFiles);
    }

    #endregion
}
