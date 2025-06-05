// -----------------------------------------------------------------------
// <copyright file = "SystemErrorWriter.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logging
{
    using System;
    using System.IO;

    /// <summary>
    /// This class writes a file, whose name and length comply to the Foundation's requirements,
    /// to report the system error to the Foundation.
    /// </summary>
    public class SystemErrorWriter
    {
        #region Constants

        /// <summary>
        /// Name of the system error file, specified by the Foundation.
        /// </summary>
        public const string SystemErrorFileName = "SystemError.txt";

        /// <summary>
        /// Max length of the system error message, in bytes, allowed by the Foundation.
        /// </summary>
        public const int SystemErrorMaxLength = 256;

        #endregion

        #region Fields

        /// <summary>
        /// The full path to the system error file.
        /// </summary>
        private readonly string systemErrorFilePath;

        /// <summary>
        /// Object for synchronizing the access to system error file from multiple threads.
        /// </summary>
        private readonly object fileLocker = new object();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of <see cref="SystemErrorWriter"/>.
        /// </summary>
        /// <param name="captureDirectory">
        /// The path of the capture directory specified by the Foundation.
        /// This is where to write the system error file.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="captureDirectory"/> is null or empty.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// Thrown when <paramref name="captureDirectory"/> does not exist.
        /// </exception>
        public SystemErrorWriter(string captureDirectory)
        {
            if(string.IsNullOrEmpty(captureDirectory))
            {
                throw new ArgumentException("The capture directory cannot be null or empty.", nameof(captureDirectory));
            }

            if(!Directory.Exists(captureDirectory))
            {
                throw new DirectoryNotFoundException($"Directory {captureDirectory} is not found.");
            }

            systemErrorFilePath = Path.Combine(captureDirectory, SystemErrorFileName);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Writes the caught exception to the system error file.
        /// </summary>
        /// <param name="exception">The exception thrown to write out.</param>
        public void WriteSystemError(Exception exception)
        {
            if(exception != null)
            {
                lock(fileLocker)
                {
                    var details = exception.ToString();

                    // Write system error.
                    using(var fileWriter = new StreamWriter(systemErrorFilePath))
                    {
                        fileWriter.Write(details.Substring(0, Math.Min(details.Length, SystemErrorMaxLength)));
                    }
                }
            }
        }

        #endregion
    }
}