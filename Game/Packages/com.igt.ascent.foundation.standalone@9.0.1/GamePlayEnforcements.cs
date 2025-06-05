// -----------------------------------------------------------------------
// <copyright file = "GamePlayEnforcements.cs" company = "IGT">
//     Copyright (c) 2023 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;

    /// <summary>
    /// Types of game play enforcements.
    /// </summary>
    [Flags]
    internal enum GamePlayEnforcements
    {
        /// <summary>
        /// No enforcement.
        /// </summary>
        None = 0,

        /// <summary>
        /// Denomination change requests should fail.
        /// </summary>
        DenominationChangeFail = 1 << 0,

        /// <summary>
        /// Commit game cycle calls should fail.
        /// </summary>
        CommitGameCycleFail = 1 << 1,

        /// <summary>
        /// Enroll game cycle calls should fail.
        /// </summary>
        EnrollGameCycleFail = 1 << 2,
    }
}