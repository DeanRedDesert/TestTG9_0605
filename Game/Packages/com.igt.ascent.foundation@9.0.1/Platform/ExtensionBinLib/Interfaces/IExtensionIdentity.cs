// -----------------------------------------------------------------------
// <copyright file = "IExtensionIdentity.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib.Interfaces
{
    using System;

    /// <summary>
    /// Defines an identity of an Extension, such as its Guid and version.
    /// </summary>
    public interface IExtensionIdentity
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
    }
}