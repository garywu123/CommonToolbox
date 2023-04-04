using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EzSmb;
using SMBLibrary;
using SMBLibrary.Client;
using FileAttributes = SMBLibrary.FileAttributes;

namespace CommonUtils
{
	public class Smb2ClientFileUtils
	{
		#region Static Methods

		public static async Task<SMB2Client>  ConnectAsync(string hostAddress, SmbAccountInfo accountInfo)
		{
			var client = new SMB2Client(); // SMB2Client can be used as well
			var isConnected = client.Connect(
				IPAddress.Parse(hostAddress), SMBTransportType.DirectTCPTransport
			);

			if (!isConnected) throw new InvalidOperationException("Unable to connect server.");

			var status = await Task.Run(
				() => client.Login(string.Empty, accountInfo.Username, accountInfo.Password));
			
			if (status == NTStatus.STATUS_SUCCESS) return client;

			client.Disconnect();
			throw new InvalidOperationException(
				"Unable to login server. Verify your protocol is SMB2 and user login information."
			);
		}


		public static async Task<IEnumerable<Node>> GetAllItemsAsync(
			string hostAddress, SmbAccountInfo accountInfo, string targetFolderPath)
		{
			var client = await Task.Run(() => ConnectAsync(hostAddress, accountInfo));
			var isConnected = client.Connect(
				IPAddress.Parse(hostAddress), SMBTransportType.DirectTCPTransport
			);

			if (!isConnected)
				throw new InvalidOperationException("Unable to connect remote server");

			var target = hostAddress + @"\" + targetFolderPath;

			var node = await Node.GetNode(target, accountInfo.Username, accountInfo.Password, true);
			var itemsInNode = await node.GetList();

			if (itemsInNode.Length > 0)
			{
				client.Disconnect();
				return itemsInNode;
			}

			client.Disconnect();
			throw new InvalidOperationException("Unable to get items in the folder.");
		}


		public static IEnumerable<string> ListShares(SMB2Client client, out NTStatus status)
		{
			var shares = client.ListShares(out status);
			
			if (status == NTStatus.STATUS_SUCCESS)
				return shares.Where(share => !share.EndsWith("$"));

			throw new InvalidOperationException("Unable to list shares.");
		}

		
		public static async Task DownloadFileAsync(string hostAddress,    SmbAccountInfo accountInfo,
										string targetFilePath, string         destinationFilePath,
										bool createFolder = true)
		{
			var client = await ConnectAsync(hostAddress, accountInfo);

			var target = hostAddress  + targetFilePath;

			var targetNode = await Node.GetNode(target, accountInfo.Username, accountInfo.Password, true);
			
			targetNode.CreateFolder()

			using (var stream = await targetNode.Read())
			{
				using (var fileStream = new FileStream(destinationFilePath, FileMode.Create))
				{
					await stream.CopyToAsync(fileStream);

					fileStream.Close();
				}
				
				stream.Flush();
				stream.Close();
			}
			
			client.Disconnect();
		}

		#endregion
	}
}
