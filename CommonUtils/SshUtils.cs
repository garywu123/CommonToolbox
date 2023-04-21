#region License
// author:         Wu, Gary
// created:        9:07 AM
// description:
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace CommonUtils
{

	public class SshUtils
	{
		private static string _hostKey;
		private static string _fingerPrint;
		private static string _hostKeyName;

		/// <summary>
		///     初始化 Ssh Client 对象，方便后续使用
		/// </summary>
		/// <param name="hostIp">服务器IP</param>
		/// <param name="portNum">服务器 ssh 端口号</param>
		/// <param name="userName">登录该服务器的用户名</param>
		/// <param name="password">登录该服务器的密码</param>
		public static SshClient InitialSshClient(string hostIp, int portNum, string userName,
										  string password)
		{
			var client = new SshClient(hostIp, portNum, userName, password)
			{
				ConnectionInfo =
				{
					Timeout = TimeSpan.FromSeconds(5)
				}
			};

			client.HostKeyReceived += ClientOnHostKeyReceived;
			client.Connect();
			return client;
		}

		public static string GetHostKey()
		{
			return _hostKey;
			
		}

		public static string GetHostKeyName()
		{
			
				return _hostKeyName;
			
		}

		public static string GetFingerprint()
		{
			return _fingerPrint;
		}


		private static void ClientOnHostKeyReceived(object sender, HostKeyEventArgs e)
		{
			_hostKey = BitConverter.ToString(e.HostKey).Replace("-", ":");
			_fingerPrint = BitConverter.ToString(e.FingerPrint).Replace("-", ":");
			_hostKeyName = e.HostKeyName;
		}


		/// <summary>
		///     执行一个 Linux Command，并将 Command 的输出提示返回到 StringBuilder 上
		/// </summary>
		/// <param name="client"> ssh client</param>
		/// <param name="command">Linux Command</param>
		/// <param name="terminalFeedback">Command 执行的后的反馈</param>
		/// <param name="terminalErrorFeedback">Command 执行错误信息</param>
		/// <returns>返回执行后的结果</returns>
		private static async Task<string> ExecuteCommand(SshClient client, string command)
		{
			var cmdFeedback = new StringBuilder();

			if (client == null) return null;

			if (!client.IsConnected) await Task.Run(client.Connect);

			var cmd    = client.CreateCommand(command);
			var result = cmd.BeginExecute();

			using (var reader = new StreamReader(cmd.OutputStream, Encoding.UTF8, true, 1024, true))
			{
				while (!result.IsCompleted || !reader.EndOfStream)
				{
					var line = await reader.ReadLineAsync();
					if (line != null) cmdFeedback.Append(line + Environment.NewLine);
				}
			}

			await Task.Run(() => cmd.EndExecute(result));
			return cmdFeedback.ToString();
		}


		public static async Task<string> ExecuteCommand(string host,     int port, string username,
														string password, string command)
		{
			using var client = await Task.Run(() => InitialSshClient(host, port, username, password));
			return await ExecuteCommand(client, command);
		}

		/// <summary>
		///     异步执行一个 Shell Command，主要是针对服务器不支持 Exc Command 的服务器，比如 Scalance Radio
		/// </summary>
		/// <param name="client">SSH CLIENT</param>
		/// <param name="command">需要执行的命令</param>
		/// <returns>返回一系列执行结果</returns>
		public async Task<string> InitialShellStreamCmdAsync(SshClient client, string command)
		{
			if (!client.IsConnected)
			{
				client.Connect();
			}

			var sshShell = client.CreateShellStream("dumb", 120, 80, 0, 0,
															 200000);
			var commandBytes = System.Text.Encoding.ASCII.GetBytes(command + "\n");

			await sshShell.WriteAsync(commandBytes, 0, commandBytes.Length);

			// Read the output from the server
			var buffer        = new byte[1024];
			var outputBuilder = new StringBuilder();
			int readCount;
			while ((readCount = await sshShell.ReadAsync(buffer, 0, buffer.Length)) > 0)
			{
				outputBuilder.Append(Encoding.ASCII.GetString(buffer, 0, readCount));
			}

			return outputBuilder.ToString();
		}
	}
}
