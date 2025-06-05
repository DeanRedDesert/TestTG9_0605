//-----------------------------------------------------------------------
// <copyright file = "SetupValidationFaultArea.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.Interfaces
{
    /// <summary>
    /// Enumeration used to set the fault area.
    /// </summary>
    public enum SetupValidationFaultArea
    {
        /// <summary>
        /// The unknown type of fault area.
        /// </summary>
        Uncategorized,

        /// <summary>
        /// The progressive type of fault area.
        /// </summary>
        Progressive,

        /// <summary>
        /// The game configuration type of fault area.
        /// </summary>
        GameConfiguration,

        /// <summary>
        /// The EGM configuration type of fault area.
        /// </summary>
        EgmConfiguration,

        /// <summary>
        /// The tournament type of fault area.
        /// </summary>
        Tournament
    }
}
