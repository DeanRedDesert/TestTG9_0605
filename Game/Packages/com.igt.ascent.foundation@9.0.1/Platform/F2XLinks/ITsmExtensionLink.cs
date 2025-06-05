// -----------------------------------------------------------------------
// <copyright file = "ITsmExtensionLink.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.F2XLinks
{
    using System.Collections.Generic;
    using ExtensionBinLib.Interfaces;

    /// <summary>
    /// This interface defines the functionality of the Inner Link on Tsm level.
    /// </summary>
    internal interface ITsmExtensionLink : IInnerLink
    {
        /// <summary>
        /// Gets the Chooser identifier with which this extension is linked.
        /// </summary>
        string ChooserIdentifier { get; }

        /// <summary>
        /// List of extensions imported by the Chooser.
        /// The versions are what are compatible with the importing component, not specifically
        /// the (minor) versions of the importing component requested in the registry.
        /// </summary>
        IReadOnlyList<IExtensionIdentity> ChooserImportedExtensions { get; }
    }
}