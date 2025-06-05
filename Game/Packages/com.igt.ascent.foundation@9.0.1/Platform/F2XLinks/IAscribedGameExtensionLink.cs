// -----------------------------------------------------------------------
// <copyright file = "IAscribedGameExtensionLink.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.F2XLinks
{
    using System.Collections.Generic;
    using ExtensionBinLib.Interfaces;
    using ExtensionLib.Interfaces;

    /// <summary>
    /// This interface defines the functionality of the Inner Link on AscribedGame level.
    /// </summary>
    internal interface IAscribedGameExtensionLink : IInnerLink
    {
        /// <summary>
        /// Gets the ascribed game entity with which this extension is linked.
        /// </summary>
        AscribedGameEntity AscribedGameEntity { get; }

        /// <summary>
        /// List of extensions imported by the Ascribed Game.
        /// The versions are what are compatible with the importing component, not specifically
        /// the (minor) versions of the importing component requested in the registry.
        /// </summary>
        IReadOnlyList<IExtensionIdentity> AscribedGameImportedExtensions { get; }
    }
}