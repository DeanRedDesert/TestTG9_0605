//-----------------------------------------------------------------------
// <copyright file = "IAscribedGameContext.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib.Interfaces
{
    using System.Collections.Generic;
    using ExtensionLib.Interfaces;
    using Platform.Interfaces;

    /// <summary>
    /// This interface defines data pertaining to the current active Game (either a Theme or a Shell),
    /// that is linked to one of the executable extensions.
    /// </summary>
    public interface IAscribedGameContext : IInnerContext
    {
        /// <summary>
        /// Gets the entity of the current active Game (either a Theme or a Shell)
        /// that is linked to one of the executable extensions.
        /// Null if no game is being active or linked.
        /// </summary>
        AscribedGameEntity AscribedGameEntity { get; }

        /// <summary>
        /// Gets the list of extensions imported by the Ascribed Game.
        /// </summary>
        /// <remarks>
        /// The versions are the versions of the extension services that the game requested in the game registry.
        /// </remarks>
        IReadOnlyList<IExtensionIdentity> AscribedGameImportedExtensions { get; }

        /// <summary>
        /// Gets the game mode which the Ascribed Game runs in.
        /// </summary>
        GameMode AscribedGameMode { get; }

        /// <summary>
        /// Gets the paytable identifier and the denomination of an Ascribed Theme.
        /// Only valid when <see cref="AscribedGameEntity"/> is of type <see cref="AscribedGameType.Theme"/>.
        /// Null if <see cref="AscribedGameEntity"/> is of type <see cref="AscribedGameType.Shell"/>.
        /// </summary>
        PaytableDenominationInfo? PaytableDenominationInfo { get; }
    }
}
