//-----------------------------------------------------------------------
// <copyright file = "BetPresentationConstants.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.BetFramework
{
    /// <summary>
    /// All constant string values used by the bet framework. This includes
    /// well-known modifiers and the Presentation Complete payloads and actions.
    /// </summary>
    public class BetPresentationConstants
    {
        /// <summary>
        /// The action for a Presentation Complete.
        /// </summary>
        public const string Action = "Bet Update";

        /// <summary>
        /// The dictionary payload key for a Presentation Complete.
        /// This item in the dictionary should contain a modifier name.
        /// </summary>
        public const string Payload = "Modifier";

        /// <summary>
        /// The well-known name of the "Max Bet" modifier.
        /// </summary>
        public const string MaxBetModifier = "Max Bet";

        /// <summary>
        /// The well-known name of the "Repeat Bet" modifier.
        /// </summary>
        public const string RepeatBetModifier = "Repeat Bet";

        /// <summary>
        /// The well-known name of the "Bet One" modifier.
        /// </summary>
        public const string BetOneModifier = "Bet One";
    }
}
