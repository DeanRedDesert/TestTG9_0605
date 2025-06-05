//-----------------------------------------------------------------------
// <copyright file = "OutcomeOrigin.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.OutcomeList.Interfaces
{
    /// <summary>
    /// The origin of where this outcome was created.
    /// </summary>
    [System.Serializable()]
    public enum OutcomeOrigin
    {
        /// <summary>
        /// Created by the Client.
        /// </summary>
        Client,
        
        /// <summary>
        /// Created by the Foundation.
        /// </summary>
        Foundation,
    }
}