// -----------------------------------------------------------------------
// <copyright file = "GameCycleState.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    /// <summary>
    /// Type to enumerate all the states of the game cycle that are maintained by the Foundation.
    /// </summary>
    public enum GameCycleState
    {
        /// <summary>
        /// The game cycle state is Invalid.
        /// </summary>
        Invalid,

        /// <summary>
        /// The game is being Idle.
        /// </summary>
        Idle,

        /// <summary>
        /// The game is being Committed.
        /// </summary>
        Committed,

        /// <summary>
        /// The game is being EnrollPending.
        /// </summary>
        EnrollPending,

        /// <summary>
        /// The game is being EnrollComplete.
        /// </summary>
        EnrollComplete,

        /// <summary>
        /// The game is being Playing.
        /// </summary>
        Playing,

        /// <summary>
        /// The game is being EvaluatePending.
        /// </summary>
        EvaluatePending,

        /// <summary>
        /// The game is being MainPlayComplete.
        /// </summary>
        MainPlayComplete,

        /// <summary>
        /// The game is being AncillaryPlaying.
        /// </summary>
        AncillaryPlaying,

        /// <summary>
        /// The game is being AncillaryEvaluatePending.
        /// </summary>
        AncillaryEvaluatePending,

        /// <summary>
        /// The game is being AncillaryPlayComplete.
        /// </summary>
        AncillaryPlayComplete,

        /// <summary>
        /// The game is being BonusPlaying.
        /// </summary>
        BonusPlaying,

        /// <summary>
        /// The game is being BonusEvaluatePending.
        /// </summary>
        BonusEvaluatePending,

        /// <summary>
        /// The game is being BonusPlayComplete.
        /// </summary>
        BonusPlayComplete,

        /// <summary>
        /// The game is being AbortPending.
        /// </summary>
        AbortPending,

        /// <summary>
        /// The game is being FinalizeAwardPending.
        /// </summary>
        FinalizeAwardPending,

        /// <summary>
        /// The game is being Finalized.
        /// </summary>
        Finalized
    }
}
