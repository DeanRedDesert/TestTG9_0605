// -----------------------------------------------------------------------
// <copyright file = "RollingFilesWriter.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logging
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// This class uses multiple log files to write rolling logs.
    /// When the maximum number of log files is reached, the oldest
    /// log file is deleted while a new one is created.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>
    /// Write is thread safe.
    /// </item>
    /// <item>
    /// The log file is opened and closed for each write.
    /// </item>
    /// </list>
    /// </remarks>
    public class RollingFilesWriter
    {
        #region Fields

        /// <summary>
        /// The location to store the files.
        /// </summary>
        private readonly string folderPath;

        /// <summary>
        /// The prefix of the individual log file name.
        /// </summary>
        private readonly string fileNamePrefix;

        /// <summary>
        /// The maximum number of log files to keep.
        /// </summary>
        private readonly int maxFileCount;

        /// <summary>
        /// The maximum size, in bytes, of a single log file.
        /// </summary>
        private readonly int maxFileSize;

        /// <summary>
        /// The queue of information on log files that have been written.
        /// </summary>
        private readonly Queue<LogFileInfo> fileInfoQueue;

        /// <summary>
        /// The information on the current file for writing.
        /// </summary>
        private LogFileInfo currentFile;

        /// <summary>
        /// Object for synchronizing the access to log files from multiple threads.
        /// </summary>
        private readonly object fileLocker = new object();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of <see cref="RollingFilesWriter"/>.
        /// </summary>
        /// <remarks>
        /// Files will be written to <paramref name="folderPath"/>.
        /// The file name format is something like "FileNamePrefix123.txt".
        /// </remarks>
        /// <param name="folderPath">The location to store the files.</param>
        /// <param name="fileNamePrefix">The prefix of the individual log file name.</param>
        /// <param name="maxFileCount">The maximum number of log files to keep.</param>
        /// <param name="maxFileSize">The maximum size, in bytes, of a single log file.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="folderPath"/> or <paramref name="fileNamePrefix"/> is null or empty,
        /// or when <paramref name="maxFileCount"/> or <paramref name="maxFileSize"/> is negative or zero.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// Thrown when <paramref name="folderPath"/> does not exist.
        /// </exception>
        public RollingFilesWriter(string folderPath, string fileNamePrefix, int maxFileCount, int maxFileSize)
        {
            if(string.IsNullOrEmpty(folderPath))
            {
                throw new ArgumentException("The path for saving files cannot be null or empty.", nameof(folderPath));
            }

            if(string.IsNullOrEmpty(fileNamePrefix))
            {
                throw new ArgumentException("The prefix of the file names cannot be null or empty.", nameof(fileNamePrefix));
            }

            if(maxFileCount <= 0)
            {
                throw new ArgumentException($"The max file count {maxFileCount} is invalid", nameof(maxFileCount));
            }

            if(maxFileSize <= 0)
            {
                throw new ArgumentException($"The max file size {maxFileSize} is invalid", nameof(maxFileSize));
            }

            this.folderPath = folderPath;
            this.fileNamePrefix = fileNamePrefix;
            this.maxFileCount = maxFileCount;
            this.maxFileSize = maxFileSize;

            if(!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"Directory {folderPath} is not found.");
            }

            // Search under the folder for any exiting log files.
            fileInfoQueue = DiscoverLogFiles(folderPath, fileNamePrefix + "*.txt");

            if(fileInfoQueue.Any())
            {
                // Write to the last file in queue.
                currentFile = fileInfoQueue.Last();
            }
            else
            {
                // Create a new file for write.
                currentFile = new LogFileInfo(BuildFilePath(1), 1, 0);
                fileInfoQueue.Enqueue(currentFile);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Writes a message to the rolling log files.
        /// </summary>
        /// <remarks>
        /// The message is written as is, without any additional formatting.
        /// </remarks>
        /// <param name="message">The message to write.</param>
        public void Write(string message)
        {
            if(string.IsNullOrEmpty(message))
            {
                return;
            }

            lock(fileLocker)
            {
                if(message.Length <= maxFileSize)
                {
                    if(currentFile.Size + message.Length > maxFileSize)
                    {
                        currentFile = CreateNextFile(currentFile);
                    }

                    WriteToFile(currentFile, message);
                }
                else
                {
                    // A message longer than the max file size is unlikely to happen,
                    // but we handled it anyway.
                    var subMessages = new List<string>();
                    var offset = 0;
                    var remaining = message.Length;

                    // Break the message into chunks of maxFileSize.
                    while(remaining > maxFileSize)
                    {
                        subMessages.Add(message.Substring(offset, maxFileSize));
                        offset += maxFileSize;
                        remaining -= maxFileSize;
                    }

                    // Add the last part of the message.
                    subMessages.Add(message.Substring(offset));

                    foreach(var subMessage in subMessages)
                    {
                        // Recursive call.
                        Write(subMessage);
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Searches at the specified location for all the files that
        /// comply with the given naming pattern, and returns a queue
        /// of file information ordered by the last write time.
        /// </summary>
        /// <param name="discoverPath">
        /// The path to discover the files.
        /// </param>
        /// <param name="discoverPattern">
        /// The pattern of the file names to discover.
        /// </param>
        /// <returns>
        /// The list of <see cref="LogFileInfo"/> of discovered files.
        /// </returns>
        private Queue<LogFileInfo> DiscoverLogFiles(string discoverPath, string discoverPattern)
        {
            var directoryInfo = new DirectoryInfo(discoverPath);
            var fileInfoList = new List<FileInfo>(directoryInfo.GetFiles(discoverPattern));

            var result = from fileInfo in fileInfoList
                         orderby fileInfo.LastWriteTimeUtc
                         select new LogFileInfo(fileInfo.FullName, RetrieveSequenceNumber(fileInfo.Name), (int)fileInfo.Length);

            return new Queue<LogFileInfo>(result);
        }

        /// <summary>
        /// Retrieves the sequence number from a log file name.
        /// </summary>
        /// <remarks>
        /// The file name format is something like "FileNamePrefix123.txt".
        /// </remarks>
        /// <param name="fileName">The log file name.</param>
        /// <returns>The sequence number retrieved.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="fileName"/> does not comply with the naming pattern.
        /// </exception>
        private int RetrieveSequenceNumber(string fileName)
        {
            var startIndex = fileName.IndexOf(fileNamePrefix, StringComparison.Ordinal) + fileNamePrefix.Length;
            var endIndex = fileName.LastIndexOf('.');
            var numberString = fileName.Substring(startIndex, endIndex - startIndex);

            if(!int.TryParse(numberString, out var result))
            {
                throw new ArgumentException($"Failed to retrieve sequence number from file name {fileName}.");
            }

            return result;
        }

        /// <summary>
        /// Builds the full path of a log file based on its sequence number.
        /// </summary>
        /// <remarks>
        /// The file name format is something like "FileNamePrefix123.txt".
        /// </remarks>
        /// <param name="sequenceNumber">The sequence number of the log file.</param>
        /// <returns>The full path of the log file.</returns>
        private string BuildFilePath(int sequenceNumber)
        {
            return Path.Combine(folderPath, fileNamePrefix + sequenceNumber + ".txt");
        }

        /// <summary>
        /// Creates the next file in sequence that follows a given file.
        /// </summary>
        /// <param name="currentFileInfo">The file that precedes the next file.</param>
        /// <returns>The next file that succeeds the given file.</returns>
        private LogFileInfo CreateNextFile(LogFileInfo currentFileInfo)
        {
            // Roll over the sequence number if necessary.
            int nextSequenceNumber;
            if(currentFileInfo.SequenceNumber < int.MaxValue)
            {
                nextSequenceNumber = currentFileInfo.SequenceNumber + 1;
            }
            else
            {
                nextSequenceNumber = 1;
            }

            // Create a new log file.
            var newFile = new LogFileInfo(BuildFilePath(nextSequenceNumber), nextSequenceNumber, 0);

            fileInfoQueue.Enqueue(newFile);

            // If the file count reaches the limit, delete the oldest one.
            if(fileInfoQueue.Count > maxFileCount)
            {
                var deletedFileInfo = fileInfoQueue.Dequeue();
                File.Delete(deletedFileInfo.FilePath);
            }

            return newFile;
        }

        /// <summary>
        /// Writes a message to the specified file, and update its size info afterwards.
        /// </summary>
        /// <param name="logFileInfo">The information on the log file to write to.</param>
        /// <param name="message">The message to write.</param>
        private static void WriteToFile(LogFileInfo logFileInfo, string message)
        {
            if(!string.IsNullOrEmpty(logFileInfo.FilePath))
            {
                using(var fileWriter = new StreamWriter(logFileInfo.FilePath, true) { AutoFlush = true })
                {
                    fileWriter.Write(message);

                    // Update the file size.
                    logFileInfo.Size = fileWriter.BaseStream.Length;
                }
            }
        }

        #endregion
    }
}
