// -----------------------------------------------------------------------
// <copyright file = "IExtensionImport.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// An interface for accessing information on the imported extension linked to the application.
    /// </summary>
    public interface IExtensionImport
    {
        /// <summary>
        /// Gets the extension identifier.
        /// </summary>
        Guid ExtensionIdentifier { get; }

        /// <summary>
        /// Gets the <see cref="Version"/> of the extension.
        /// </summary>
        /// <remarks>
        /// The version consists of a major version number, a minor version number , and a build version number.
        /// The build version number of the <see cref="Version"/> class correspond to the patch version of the extension.
        /// </remarks>
        Version ExtensionVersion { get; }

        /// <summary>
        /// Gets the absolute directory path to be used by extension clients as the root directory for locating
        /// resource extensions.
        /// </summary>
        /// <remarks>
        /// This path is only provided for resource extensions.
        /// </remarks>
        string ResourceDirectoryBase { get; }
    }
}
