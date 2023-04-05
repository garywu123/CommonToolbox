#region License

// author:         Wu, Gary
// created:        1:43 PM
// description:

#endregion

using System.IO;
using System.Threading.Tasks;
using Renci.SshNet;

namespace CommonUtils
{
	public class ScpUtils
	{
		#region Static Methods

		public static async Task DownloadFileAsync(string host, int port, string user,
												   string password,
												   string remoteFilePath, string localFilePath,
												   bool createDir = false)
		{
			if (createDir)
			{
				FileUtils.TryParseFilePath(localFilePath, true);
			}

			using var client =
				await Task.Run(() => SshUtils.InitialSshClient(host, port, user, password));

			if (!client.IsConnected) client.Connect();

			using var scp = new ScpClient(client.ConnectionInfo);
			scp.Connect();
			scp.Download(remoteFilePath, new FileInfo(localFilePath));
		}

		public static async Task DownloadFilesAsync(string   host, int port, string user,
													string   password,
													string[] remoteFiles,
													string   localDir)
		{
			if (!Directory.Exists(localDir))
			{
				Directory.CreateDirectory(localDir);
			}

			using (var client =
				await Task.Run(() => SshUtils.InitialSshClient(host, port, user, password)))
			{
				if (!client.IsConnected) client.Connect();
				using (var scp = new ScpClient(client.ConnectionInfo))
				{
					foreach (var remoteFile in remoteFiles)
					{
						var fileName      = Path.GetFileName(remoteFile);
						var localFilePath = Path.Combine(localDir, fileName);
						scp.Connect();
						await Task.Run(() => scp.Download(remoteFile, new FileInfo(localFilePath)));
					}
				}
			}
		}

		public static async Task DownloadFolderAsync(string host, int port, string user,
													 string password,
													 string remoteFolderPath,
													 string localFolderPath)
		{
			using (var client =
				await Task.Run(() => SshUtils.InitialSshClient(host, port, user, password)))
			{
				if (!client.IsConnected) client.Connect();
				using (var scp = new ScpClient(client.ConnectionInfo))
				{
					scp.Connect();
					await Task.Run(
						() => scp.Download(remoteFolderPath, new DirectoryInfo(localFolderPath))
					);
				}
			}
		}

		public static async Task UploadFileAsync(string host, int port, string user,
												 string password,
												 string localFilePath, string remoteFilePath)
		{
			using (var client =
				await Task.Run(() => SshUtils.InitialSshClient(host, port, user, password)))
			{
				if (!client.IsConnected) client.Connect();

				using (var scp = new ScpClient(client.ConnectionInfo))
				{
					scp.Connect();

					await Task.Run(() => scp.Upload(new FileInfo(localFilePath), remoteFilePath));
				}
			}
		}

		#endregion
	}
}
