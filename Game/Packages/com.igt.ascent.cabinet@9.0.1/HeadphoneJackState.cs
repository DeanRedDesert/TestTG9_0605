//-----------------------------------------------------------------------
// <copyright file = "HeadphoneJackState.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Supported states for the headphone jack.
    /// </summary>
    public enum HeadphoneJackState
    {
        /// <summary>
        /// Headphones have been inserted.
        /// </summary>
        Inserted,
        /// <summary>
        /// Headphones have been removed.
        /// </summary>
        Removed
    }
}
