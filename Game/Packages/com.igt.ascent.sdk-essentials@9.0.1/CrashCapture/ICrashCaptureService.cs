// -----------------------------------------------------------------------
// <copyright file = "ICrashCaptureService.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.CrashCapture
{
    using System;

    /// <summary>
    /// Delegate type for callback, which will be called on game crash.
    /// </summary>
    /// <param name="maxSize">Maximum allowed size in bytes.</param>
    public delegate string CrashDataCreatorHandler(long maxSize); 

    /// <summary>
    /// Interface for crash capture service.
    /// </summary>
    public interface ICrashCaptureService
    {
        /// <summary>
        /// Registers callback, which will create crash data file in capture folder.
        /// If a callback was registered before, replaces it with the new one.
        /// </summary>
        /// <param name="fileName">Name of the file in capture folder, where crash data will be persisted.</param>
        /// <param name="cbHandler">Callback method, which will create crash data.</param>
        /// <exception cref="ArgumentException">
        /// Exception will be thrown, when <paramref name="fileName"/> not properly defined. Only <paramref name="fileName"/> without path is allowed.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Exception will be thrown, when <paramref name="fileName"/> or <paramref name="cbHandler"/> is null.
        /// </exception>
        void RegisterCrashDataCreator(string fileName, CrashDataCreatorHandler cbHandler);

        /// <summary>
        /// Unregisters callback by its filename.
        /// </summary>
        /// <param name="fileName">Name of the file in capture folder, where crash data will be persisted.</param>
        /// <exception cref="ArgumentNullException">
        /// Exception will be thrown, when <paramref name="fileName"/> is null.
        /// </exception>
        void UnregisterCrashDataCreator(string fileName);
    }
}
