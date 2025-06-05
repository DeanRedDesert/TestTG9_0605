// -----------------------------------------------------------------------
// <copyright file = "IGameCycleStateQuery.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    /// <summary>
    /// This interface defines the method to query the game cycle state.
    /// </summary>
    public interface IGameCycleStateQuery
    {
        /// <summary>
        /// Gets the current game cycle state.
        /// </summary>
        GameCycleState GameCycleState { get; }
    }
}
