// -----------------------------------------------------------------------
// <copyright file = "CrashCaptureService.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.CrashCapture
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Game.Core.Logging;

    /// <summary>
    /// Singleton crash capture service.
    /// </summary>
    public sealed class CrashCaptureService : ICrashCaptureService, ICrashCaptureServiceRestricted
    {
        #region Singleton Definition

        /// <summary>
        /// The single instance of the service. Used to ensure there is only one.
        /// </summary>
        private static CrashCaptureService instance;

        /// <summary>
        /// Synchronization object to lock when accessing the singleton object <see cref="instance"/>.
        /// </summary>
        private static readonly object InstanceLock = new object();

        /// <summary>
        /// Private constructor for the singleton instance.
        /// </summary>
        private CrashCaptureService()
        {
        }

        /// <summary>
        /// Gets the singleton instance, creating it first if necessary.
        /// </summary>
        public static ICrashCaptureService Instance
        {
            get
            {
                lock(InstanceLock)
                {
                    return instance ?? (instance = new CrashCaptureService());
                }
            }
        }

        #endregion Singleton Definition

        #region Private Fields

        /// <summary>
        /// Collection of callbacks and their crash data file names.
        /// </summary>
        private readonly IDictionary<string, CrashDataCreatorHandler> crashDataCaptureCallbacks =
            new ConcurrentDictionary<string, CrashDataCreatorHandler>();

        /// <summary>
        /// Predefined size of capture directory, when the value not provided in command line parameters.
        /// The value is set to 1024kB. This value is used in foundation also, when in development mode.
        /// (in file CaptureDirectoryManager.cpp)
        /// </summary>
        private readonly long predefinedCaptureDirectoryQuota = 1024 * 1024;

        /// <summary>
        /// Capture directory path field.
        /// </summary>
        private string captureDirectoryPath;

        /// <summary>
        /// Capture directory quota field.
        /// </summary>
        private long  captureDirectoryQuota;

        #endregion Private Fields

        #region ICrashCaptureService

        /// <inheritdoc />
        public void RegisterCrashDataCreator(string fileName, CrashDataCreatorHandler cbHandler)
        {
            if(fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName), "fileName is null.");
            }

            if(cbHandler == null)
            {
                throw new ArgumentNullException(nameof(cbHandler), "CbHandler is null.");
            }

            // Validate that the filename without path was specified.
            var pureFileName = Path.GetFileName(fileName);
            if(pureFileName != fileName)
            {
                throw new ArgumentException($"fileName parameter '{fileName}' should not contain directory path.", nameof(fileName));
            }

            if(crashDataCaptureCallbacks.ContainsKey(fileName))
            {
                Log.Write($"Replacing a previously registered callback for {fileName}...");
            }

            crashDataCaptureCallbacks[fileName] = cbHandler;
            Log.Write($"Callback with the crash file name: {fileName} successfully registered.");
        }

        /// <inheritdoc />
        public void UnregisterCrashDataCreator(string fileName)
        {
            if(crashDataCaptureCallbacks.Remove(fileName))
            {
                Log.Write($"Callback with the crash file name '{fileName}' successfully removed.");
            }
        }

        #endregion ICrashCaptureService

        #region ICrashCaptureServiceRestricted

        /// <inheritdoc />
        public void StartCapturingProcess()
        {
            try
            {
                if(Directory.Exists(captureDirectoryPath))
                {
                    // calculate free space on the destination directory
                    var maxFileSize = GetCaptureDirectoryFreeSpace();

                    // loop through all items in the collection
                    foreach(var crashDataCaptureCallback in crashDataCaptureCallbacks)
                    {
                        var crashDumpSize = CreateCrashDump(crashDataCaptureCallback.Key, crashDataCaptureCallback.Value, maxFileSize);
                        maxFileSize -= crashDumpSize;
                    }
                }
                else
                {
                    Log.WriteWarning($"Capturing process failed! Capture directory doesn't exist: '{captureDirectoryPath}'!");
                }
            }
            catch(Exception ex)
            {
                Log.WriteWarning($"Capturing process failed! Exception thrown: '{ex}'!");
            }

        }

        /// <inheritdoc />
        public void SetCaptureDirectoryQuota(long directoryQuota)
        {
            captureDirectoryQuota = directoryQuota;
        }

        /// <inheritdoc />
        public void SetCaptureDirectoryPath(string directoryPath)
        {
            captureDirectoryPath = directoryPath;
        }

        #endregion ICrashCaptureServiceRestricted

        #region Private Methods

        /// <summary>
        /// Get capture directory quota size.
        /// </summary>
        /// <returns>
        /// Capture directory quota as size. If not set to value different from zero - set predefined value.
        /// </returns>
        private long GetCaptureDirectoryQuota()
        {
            return captureDirectoryQuota == 0 ? predefinedCaptureDirectoryQuota : captureDirectoryQuota;
        }

        /// <summary>
        /// Calculate free space in capture directory.
        /// </summary>
        /// <returns>
        /// Free space in bytes in capture directory. If no free space available it returns 0.
        /// </returns>
        private long GetCaptureDirectoryFreeSpace()
        {
            DirectoryInfo dir = new DirectoryInfo(captureDirectoryPath);
            var totalSpaceUsed = dir.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
            var directoryQuota = GetCaptureDirectoryQuota();
            return directoryQuota >= totalSpaceUsed ? directoryQuota - totalSpaceUsed : 0;
        }

        /// <summary>
        /// Create crash dump file in capture directory.
        /// </summary>
        /// <param name="fileName">Name of the crash data file.</param>
        /// <param name="callBackHandler">Callback method which will create the crash data.</param>
        /// <param name="maxCrashDataSize">Max crash data size.</param>
        /// <returns>
        /// The size of created and persisted crash dump in bytes.
        /// </returns>
        private long CreateCrashDump(string fileName, CrashDataCreatorHandler callBackHandler, long maxCrashDataSize)
        {
            long crashDumpSize = 0;
            try
            {
                // Call the call back handler, which will create crash dump
                var crashDump = callBackHandler(maxCrashDataSize);
                crashDumpSize = Encoding.UTF8.GetByteCount(crashDump);
                if(crashDumpSize <= maxCrashDataSize)
                {
                    var fileNameWithPath = Path.Combine(captureDirectoryPath, fileName);
                    // UTF8 without BOM encoding is needed, because SAE editor not able to import unicode encoded file.
                    using(var streamWriter = new StreamWriter(fileNameWithPath, false, new UTF8Encoding(false)))
                    {
                        streamWriter.Write(crashDump);
                        Log.Write($"Crash file {fileName} persisted to the capture directory.");
                    }
                }
                else
                {
                    crashDumpSize = 0;
                    Log.WriteWarning(
                        $"Crash file {fileName} not created! Crash dump too large: ({crashDump.Length} bytes)!");
                }
            }
            catch(Exception ex)
            {
                Log.WriteWarning($"Crash file {fileName} not created! Exception thrown: '{ex}'!");
            }

            return crashDumpSize;
        }

        #endregion Private Methods

    }
}
