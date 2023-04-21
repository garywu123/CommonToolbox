#region License
// author:         Wu, Gary
// created:        1:53 PM
// description:
#endregion

using System;
using System.IO;
using System.Reflection;

namespace CommonUtils
{
	public class FileUtils
	{
		public static string GetRunningAssemblyFolder()
		{
			return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}

		public static bool TryParseFilePath(string path, bool createDirectory = false)
		{
			var directoryPath = Path.GetDirectoryName(path);

			if (Directory.Exists(directoryPath)) return true;

			if (createDirectory)
			{
				Directory.CreateDirectory(directoryPath ?? throw new InvalidOperationException("Unable to retrieve the directory path from the file path."));
			}

			return false;
		}

		public static void DeleteAll(string directoryPath)
		{
			if (!Directory.Exists(directoryPath))
			{
				throw new DirectoryNotFoundException("The directory does not exist.");
			}

			// Delete all files inside the directory
			foreach (var filePath in Directory.GetFiles(directoryPath))
			{
				File.Delete(filePath);
			}

			// Delete all subdirectories and their contents
			foreach (var subDir in Directory.GetDirectories(directoryPath))
			{
				Directory.Delete(subDir, true);
			}
		}

		/// <summary>
		///		Check if the path is a directory path
		/// </summary>
		/// <param name="path">the path string</param>
		/// <returns>return true if it is a file path</returns>
		/// <exception cref="ArgumentException"></exception>
		public static bool IsDirPath(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentException("The provided path cannot be null or empty.");
			}

			try
			{

				var attributes = File.GetAttributes(path);
				return (attributes & FileAttributes.Directory) == FileAttributes.Directory;
			}
			catch (FileNotFoundException)
			{
				// If the path does not exist, check if it has an extension (assuming it's a file path)
				return string.IsNullOrEmpty(Path.GetExtension(path));
			}
			catch (Exception)
			{
				throw new ArgumentException("An error occurred while verifying the path.");
			}
		}
	}
}
