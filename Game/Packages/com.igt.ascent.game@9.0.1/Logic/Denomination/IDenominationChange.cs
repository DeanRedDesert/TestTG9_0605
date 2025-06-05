//-----------------------------------------------------------------------
// <copyright file = "IDenominationChange.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Denomination
{
    /// <summary>
    /// This interface is used to maintain the denomination change state while the Game 
    /// is communicating with the Foundation on a denomination change request.
    /// </summary>
    public interface IDenominationChange
    {
        /// <summary>
        /// Get the denomination change state.
        /// </summary>
        DenominationChangeState DenominationChangeState { get; }

        /// <summary>
        /// Request that the Foundation change the denomination within the current theme.
        /// </summary>
        /// <param name="newDenomination">The new denomination to be set.</param>
        /// <returns>True if the request was accepted.</returns>
        bool RequestDenominationChange(long newDenomination);
    }
}
