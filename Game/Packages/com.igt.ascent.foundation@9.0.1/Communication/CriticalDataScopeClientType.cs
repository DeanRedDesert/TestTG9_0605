//-----------------------------------------------------------------------
// <copyright file = "CriticalDataScopeClientType.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    /// <summary>
    /// Client type of accessing critical data.
    /// </summary>
    internal enum CriticalDataScopeClientType
    {
        /// <summary>
        /// The client of accessing critical data is game.
        /// </summary>
        Game,

        /// <summary>
        /// The client of accessing critical data is report.
        /// </summary>
        Report,

        /// <summary>
        /// The client of accessing critical data is extension.
        /// </summary>
        Extension
    }
}
