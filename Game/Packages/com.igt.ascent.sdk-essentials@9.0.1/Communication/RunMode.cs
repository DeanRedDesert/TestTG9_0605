// -----------------------------------------------------------------------
// <copyright file = "RunMode.cs" company = "IGT">
//     Copyright (c) 2022 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication
{
    /// <summary>
    /// Indicates the mode an application is running in.
    /// </summary>
    /// <devdoc>
    /// At the moment it is unnecessary to map each GameType to a RunMode yet.
    /// New enum values can be added in the future as needed.
    /// </devdoc>
    public enum RunMode
    {
        /// <summary>
        /// The run mode is not specified.
        /// </summary>
        Unspecified,

        /// <summary>
        /// Standard mode, running with a Foundation.
        /// </summary>
        Standard,

        /// <summary>
        /// Standalone mode, running without a Foundation.
        /// This could be either running inside a Unity Editor,
        /// or outside a Unity Editor as a standalone build.
        /// </summary>
        Standalone,

        /// <summary>
        /// Fast play mode, where the application is being
        /// fast played for math verification purposes.
        /// </summary>
        FastPlay
    }
}