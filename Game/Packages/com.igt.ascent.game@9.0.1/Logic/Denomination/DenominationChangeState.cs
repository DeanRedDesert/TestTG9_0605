//-----------------------------------------------------------------------
// <copyright file = "DenominationChangeState.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Denomination
{
    using System;

    /// <summary>
    /// This enumeration is used to indicate the state of the denomination change.
    /// </summary>
    [Serializable]
    public enum DenominationChangeState
    {
        /// <summary>
        /// The denomination is set.
        /// </summary>
        DenominationSet = 0,

        /// <summary>
        /// The denomination is pending on change.
        /// </summary>
        DenominationChanging,

        /// <summary>
        /// The denomination change request is cancelled.
        /// </summary>
        DenominationChangeCancelled
    }
}
