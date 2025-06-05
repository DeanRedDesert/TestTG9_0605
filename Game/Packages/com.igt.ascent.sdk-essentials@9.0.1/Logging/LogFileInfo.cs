// -----------------------------------------------------------------------
// <copyright file = "LogFileInfo.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logging
{
    /// <summary>
    /// This class represents a record of a log file that has been written.
    /// </summary>
    internal class LogFileInfo
    {
        /// <summary>
        /// Gets the path of the log file.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Gets the 1-based sequence number of the log file.
        /// </summary>
        public int SequenceNumber { get; }

        /// <summary>
        /// Gets or sets the size of the log file.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="LogFileInfo"/>.
        /// </summary>
        /// <remarks>
        /// This constructor does not validate arguments.
        /// The caller is responsible for passing in valid values.
        /// </remarks>
        /// <param name="filePath">The path of the log file.</param>
        /// <param name="sequenceNumber">The sequence number of a log file.</param>
        /// <param name="size">The size of a log file.</param>
        public LogFileInfo(string filePath, int sequenceNumber, long size)
        {
            FilePath = filePath;
            SequenceNumber = sequenceNumber;
            Size = size;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"File({FilePath}) / Sequence Number({SequenceNumber}) / Size({Size})";
        }
    }
}
