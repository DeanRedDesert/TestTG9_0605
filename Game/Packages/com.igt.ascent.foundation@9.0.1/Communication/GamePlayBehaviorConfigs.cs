// -----------------------------------------------------------------------
// <copyright file = "GamePlayBehaviorConfigs.cs" company = "IGT">
//     Copyright (c) 2022 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// A simple implementation of <see cref="IGamePlayBehaviorConfigs"/>.
    /// All properties are mutable to save the effort of implementing constructors.
    /// </summary>
    internal sealed class GamePlayBehaviorConfigs : IGamePlayBehaviorConfigs
    {
        #region Implementation of IGamePlayBehaviorConfigs

        /// <inheritdoc />
        public BetSelectionStyleInfo DefaultBetSelectionStyle { get; set; }

        /// <inheritdoc />
        public bool RtpOrderedByBetRequired { get; set; }

        #endregion
    }
}