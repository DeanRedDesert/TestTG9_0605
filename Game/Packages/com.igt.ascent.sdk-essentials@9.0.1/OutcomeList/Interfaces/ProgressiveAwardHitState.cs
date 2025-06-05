//-----------------------------------------------------------------------
// <copyright file = "ProgressiveAwardHitState.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.OutcomeList.Interfaces
{
    /// <summary>
    /// Enum for determining if this progressive was a hit or not.
    /// </summary>
    [System.Serializable()]
    public enum ProgressiveAwardHitState
    {

        /// <remarks/>
        PotentialHit,

        /// <remarks/>
        Hit,

        /// <remarks/>
        NotHit,
    }
}