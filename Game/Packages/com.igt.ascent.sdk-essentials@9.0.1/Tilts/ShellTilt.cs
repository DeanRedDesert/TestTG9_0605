//-----------------------------------------------------------------------
// <copyright file = "ShellTilt.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Tilts
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This class represents a game side tilt to be used by concurrent games.
    /// </summary>
    /// <devdoc>
    /// It supports different flags than <see cref="GameTilt"/>.
    /// </devdoc>
    [Serializable]
    public class ShellTilt : GameTilt
    {
        #region Constructors

        /// <summary>
        /// Default parameterless constructor needed by Compact Serialization.
        /// </summary>
        public ShellTilt()
        {
        }

        /// <inheritdoc>
        ///     <cref>ActiveTilt</cref>
        /// </inheritdoc>
        /// <devdoc>
        /// Always use TiltDiscardBehavior.OnGameTermination.
        /// We do not support TiltDiscardBehavior.Never for Shells yet.
        /// </devdoc>
        public ShellTilt(TiltPriority priority,
                         Dictionary<string, string> messageDictionary,
                         Dictionary<string, string> titleDictionary,
                         TiltGamePlayBehavior gamePlayBehavior = TiltGamePlayBehavior.NonBlocking,
                         bool userInterventionRequired = false,
                         bool progressiveLinkDown = false,
                         bool attendantClear = false,
                         bool delayPreventGamePlayUntilNoGameInProgress = false)
        : base(priority,
               messageDictionary,
               titleDictionary,
               gamePlayBehavior,
               userInterventionRequired,
               progressiveLinkDown,
               attendantClear,
               delayPreventGamePlayUntilNoGameInProgress,
               TiltDiscardBehavior.OnGameTermination)
        {
        }

        #endregion
    }
}
