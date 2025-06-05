// -----------------------------------------------------------------------
// <copyright file = "ISystemExtensionLink.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.F2XLinks
{
    using System.Collections.Generic;
    using ExtensionBinLib.Interfaces;

    /// <summary>
    /// This interface defines the functionality of the Inner Link on System level.
    /// </summary>
    internal interface ISystemExtensionLink : IInnerLink
    {
        /// <summary>
        /// Gets the list of system extension services that the Extension Bin is to provide,
        /// i.e. the services that the Extension Bin claimed to support, and “won” the right to support.
        /// </summary>
        IReadOnlyList<IExtensionIdentity> SupportedExtensions { get; }
    }
}