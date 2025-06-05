// -----------------------------------------------------------------------
// <copyright file = "ICrashCaptureServiceRestricted.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.CrashCapture
{
    /// <summary>
    /// Interface for crash capture service restricted.
    /// </summary>
    public interface ICrashCaptureServiceRestricted
    {
        /// <summary>
        /// Start capturing process for all registered crash data creators.
        /// </summary>
        void StartCapturingProcess();

        /// <summary>
        /// Set capture directory path.
        /// </summary>
        /// <param name="captureDirectoryPath">Capture directory path.</param>
        void SetCaptureDirectoryPath(string captureDirectoryPath);

        /// <summary>
        /// Set capture directory quota.
        /// </summary>
        /// <param name="captureDirectoryQuota">Capture directory quota size in bytes.</param>
        void SetCaptureDirectoryQuota(long captureDirectoryQuota);
    }
}
