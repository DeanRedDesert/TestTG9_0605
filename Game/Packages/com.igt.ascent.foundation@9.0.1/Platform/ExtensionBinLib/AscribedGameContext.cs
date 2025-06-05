// -----------------------------------------------------------------------
// <copyright file = "AscribedGameContext.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using ExtensionLib.Interfaces;
    using Interfaces;
    using Platform.Interfaces;

    /// <summary>
    /// A simple implementation of <see cref="IAscribedGameContext"/>.
    /// </summary>
    internal class AscribedGameContext : IAscribedGameContext
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of <see cref="AscribedGameContext"/>.
        /// </summary>
        /// <param name="gameMode">
        /// The game mode which the linked game runs in.
        /// </param>
        /// <param name="paytableDenominationInfo">
        /// The paytable denomination info if available.
        /// Usually it is only available for Ascribed Theme.
        /// </param>
        /// <param name="ascribedGameEntity">
        /// The type and identifier of the linked game.
        /// </param>
        /// <param name="ascribedGameImportedExtensions">
        /// The list of extensions imported by the Ascribed Game.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="paytableDenominationInfo"/> is null while
        /// <paramref name="ascribedGameEntity"/> is of type <see cref="AscribedGameType.Theme"/>,
        /// or when <paramref name="paytableDenominationInfo"/> is not null while
        /// <paramref name="ascribedGameEntity"/> is of type <see cref="AscribedGameType.Shell"/>,
        /// </exception>
        public AscribedGameContext(GameMode gameMode = GameMode.Invalid,
                                   PaytableDenominationInfo? paytableDenominationInfo = null,
                                   AscribedGameEntity ascribedGameEntity = null,
                                   IReadOnlyList<IExtensionIdentity> ascribedGameImportedExtensions = null)
        {
            switch(ascribedGameEntity?.AscribedGameType)
            {
                case AscribedGameType.Theme when paytableDenominationInfo == null:
                {
                    throw new ArgumentException($"{nameof(paytableDenominationInfo)} cannot be null when ascribed game type is Theme.");
                }
                case AscribedGameType.Shell when paytableDenominationInfo != null:
                {
                    throw new ArgumentException($"{nameof(paytableDenominationInfo)} must be null when ascribed game type is Shell.");
                }
            }

            AscribedGameMode = gameMode;
            PaytableDenominationInfo = paytableDenominationInfo;
            AscribedGameEntity = ascribedGameEntity;
            AscribedGameImportedExtensions = ascribedGameImportedExtensions ?? new List<IExtensionIdentity>();
        }

        #endregion

        #region IAscribedGameContext Implementation

        /// <inheritdoc />
        public AscribedGameEntity AscribedGameEntity { get; }

        /// <inheritdoc />
        public IReadOnlyList<IExtensionIdentity> AscribedGameImportedExtensions { get; }

        /// <inheritdoc />
        public GameMode AscribedGameMode { get; }

        /// <inheritdoc />
        public PaytableDenominationInfo? PaytableDenominationInfo { get; }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder()
                          .AppendLine("AscribedGameContext -")
                          .AppendLine($"\tGameMode: {AscribedGameMode}");

            if(AscribedGameEntity != null)
            {
                builder.AppendLine($"\t{AscribedGameEntity}");
            }

            if(PaytableDenominationInfo != null)
            {
                builder.AppendLine($"\t{PaytableDenominationInfo}");
            }

            builder.AppendLine($"\tTotal {AscribedGameImportedExtensions.Count} extensions imported by the Ascribed Game:");
            foreach(var extensionIdentity in AscribedGameImportedExtensions)
            {
                builder.AppendLine($"\t\t{extensionIdentity}");
            }

            return builder.ToString();
        }

        #endregion
    }
}
