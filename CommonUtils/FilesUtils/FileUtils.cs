#region License

// author:         Wu, Gary
// created:        1:53 PM
// description:

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CommonUtils;

public static class FileUtils
{
    #region Static Methods


    /// <summary>
    ///     Copy the files to directory async.
    /// </summary>
    /// <param name="srcFilePaths">source files list</param>
    /// <param name="destDirectoryPath">destination folder</param>
    /// <param name="progress">the progress. </param>
    /// <returns>return how many files copied and the failed files.</returns>
    public static async Task<(int, List<(string, string)>)> CopyFilesAsync(
        IEnumerable<string> srcFilePaths, string destDirectoryPath, IProgress<int> progress = null)
    {
        var srcFiles = srcFilePaths.ToArray();
        var destFilePaths = new string[srcFilePaths.Count()];

        for (int i = 0; i < srcFilePaths.Count(); i++)
        {
            var fileName = Path.GetFileName(srcFiles[i]);
            destFilePaths[i] = Path.Combine(destDirectoryPath, fileName);
        }

        return await CopyFilesAsync(srcFiles, destFilePaths, progress);
    }


    /// <summary>
    ///     Copy files async.
    /// </summary>
    /// <param name="srcFilePath">source files list</param>
    /// <param name="destFilePath">destination file list</param>
    /// <param name="progress">the progress</param>
    /// <returns>return how many files copied and the failed files.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static async Task<(int, List<(string, string)>)> CopyFilesAsync(
        string[] srcFilePath, string[] destFilePath, IProgress<int> progress = null)
    {
        if (srcFilePath.Length != destFilePath.Length)
            throw new ArgumentException(
                "The source and destination file path arrays must have the same length."
            );

        var filesCopied = 0;
        var failedFiles = new List<(string, string)>();

        for (var i = 0; i < srcFilePath.Length; i++)
        {
            if (!File.Exists(srcFilePath[i]))
            {
                failedFiles.Add((srcFilePath[i], $"The file {srcFilePath[i]} does not exist."));
                continue;
            }

            var destDirPath = Path.GetDirectoryName(destFilePath[i]);

            if (!Directory.Exists(destDirPath))
                try { Directory.CreateDirectory(destDirPath); }
                catch (Exception ex)
                {
                    failedFiles.Add(
                        (destDirPath, $"Failed to create directory {destDirPath}: {ex.Message}")
                    );

                    continue;
                }

            try
            {
                await Task.Run(() => File.Copy(srcFilePath[i], destFilePath[i], true));
                filesCopied++;
                progress?.Report(filesCopied);
            }
            catch (Exception ex)
            {
                failedFiles.Add(
                    (srcFilePath[i],
                        $"Failed to copy file {srcFilePath[i]} to {destFilePath[i]}: {ex.Message}")
                );
            }
        }

        return (filesCopied, failedFiles);
    }

    

    /// <summary>
    ///     Delete all the files in the <see cref="directoryPath" />
    /// </summary>
    /// <param name="directoryPath">the target directory</param>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public static void DeleteFilesInDirectory(string directoryPath, bool deleteSubFolders = true)
    {
        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException("The directory does not exist.");

        // Delete all files inside the directory
        foreach (var filePath in Directory.GetFiles(directoryPath)) File.Delete(filePath);

        if (!deleteSubFolders) return;

        var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            File.Delete(file);
        }
    }

    /// <summary>
    ///     Get current running assembly file location.
    /// </summary>
    /// <returns>file directory</returns>
    public static string GetRunningAssemblyFolder()
    {
        return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }


    /// <summary>
    ///     Check if the path is a directory path
    /// </summary>
    /// <param name="path">the path string</param>
    /// <returns>return true if it is a file path</returns>
    /// <exception cref="ArgumentException"></exception>
    public static bool IsDirPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("The provided path cannot be null or empty.");

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


    /// <summary>
    ///     Return the file directory information. You can create the directory for the
    ///     file if the directory doesn't exist.
    /// </summary>
    /// <param name="filePath">The target file</param>
    /// <param name="createDirectory">Create directory if true.</param>
    /// <returns>
    ///     return true if you can get file directory from the
    ///     <see cref="filePath" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static bool TryParseFilePath(string filePath, bool createDirectory = false)
    {
        var directoryPath = Path.GetDirectoryName(filePath);

        if (Directory.Exists(directoryPath)) return true;

        if (createDirectory)
            Directory.CreateDirectory(
                directoryPath
             ?? throw new InvalidOperationException(
                    "Unable to retrieve the directory path from the file path."
                )
            );

        return false;
    }

    /// <summary>
    ///     Find files that have the specified extension in the directory.
    /// </summary>
    /// <param name="directoryPath">folder path</param>
    /// <param name="extension">extension name</param>
    /// <returns>return the file list</returns>
    public static string[] FindFilesByExtension(string directoryPath, string extension)
    {
        string[] files = Directory.GetFiles(directoryPath, $"*.{extension}");

        return files;
    }

    #endregion
}
