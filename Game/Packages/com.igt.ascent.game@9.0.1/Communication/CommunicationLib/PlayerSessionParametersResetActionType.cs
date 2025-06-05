//-----------------------------------------------------------------------
// <copyright file = "PlayerSessionParametersResetActionType.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.CommunicationLib
{
    using System;

    /// <summary>
    /// Action request type for player session parameters resetting.
    /// </summary>
    [Serializable]
    public enum PlayerSessionParametersResetActionType
    {
        /// <summary>
        /// The request action to start the parameters reset.
        /// </summary>
        StartParametersReset,

        /// <summary>
        /// The request action to report the parameters reset.
        /// </summary>
        ReportParametersReset,
    }
}
