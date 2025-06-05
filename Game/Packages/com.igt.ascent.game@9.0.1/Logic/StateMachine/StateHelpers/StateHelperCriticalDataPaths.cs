//-----------------------------------------------------------------------
// <copyright file = "StateHelperCriticalDataPaths.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine.StateHelpers
{
    /// <summary>
    /// This class contains the paths the state helpers use to store their data in safe storage.
    /// </summary>
    public static class StateHelperCriticalDataPaths
    {
        ///<summary>
        ///The prefix to attach to all critical data paths.
        ///This is so it is less likely of a name collision from game developers.
        ///</summary>
        private const string StateHelperCriticalDataPathsPrefix = "StateHelperCriticalData/";

        ///<summary>
        /// Path to store the IdleStateComplete message.
        ///</summary>
        public const string IdlePresentationCompletePath = StateHelperCriticalDataPathsPrefix + "IdlePresentationComplete";

        ///<summary>
        /// Path to store the OutcomeResponse message.
        ///</summary>
        public const string OutcomeResponse = StateHelperCriticalDataPathsPrefix + "OutcomeResponse";

        ///<summary>
        /// Path to store the EnrollResponse message.
        ///</summary>
        public const string EnrollResponse = StateHelperCriticalDataPathsPrefix + "EnrollResponse";

        ///<summary>
        /// Path to store the Finalized message.
        ///</summary>
        public const string Finalized = StateHelperCriticalDataPathsPrefix + "Finalized";

        /// <summary>
        /// Path to store the flag indicating if new game has been requested in DoubleUpOfferState.
        /// </summary>
        public const string NewGameRequestedWhenDoubleUpOfferPath = StateHelperCriticalDataPathsPrefix + "NewGameRequestedWhenDoubleUpOffer";
    }
}
