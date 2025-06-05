//-----------------------------------------------------------------------
// <copyright file = "IdleStateHelperDataKeys.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine.StateHelpers
{
    /// <summary>
    /// This class contains the generic data keys used by IdleStateHelper
    /// </summary>
    public static class IdleStateHelperDataKeys
    {
        /// <summary>
        /// ID to use when requesting a bet level.
        /// </summary>
        public const string RequestedBetLevel = "RequestedBetLevel";

        /// <summary>
        /// ID to use when requesting a change to auto-play.
        /// </summary>
        public const string AutoPlayStarted = "AutoPlayStarted";

        /// <summary>
        /// ID to use when requesting a new language.
        /// </summary>
        public const string Language = "Language";

        /// <summary>
        /// ID to use when requesting a new denomination.
        /// </summary>
        public const string Denomination = "Denomination";
    }
}
