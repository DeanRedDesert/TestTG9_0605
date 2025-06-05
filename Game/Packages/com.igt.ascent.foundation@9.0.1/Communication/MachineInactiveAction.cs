//-----------------------------------------------------------------------
// <copyright file = "MachineInactiveAction.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using System;

    /// <summary>
    /// The MachineInactiveAction enumeration is used to represent the type of a Machine Active event.
    /// </summary>
    [Serializable]
    public enum MachineInactiveAction
    {
        /// <summary>
        /// Begin to play attract movies or sounds
        /// </summary>
        Attract,

        /// <summary>
        /// Reset Credit Formatter
        /// </summary>
        ResetCreditFormatter,

        /// <summary>
        /// Hide Paylines
        /// </summary>
        HidePaylines,

        /// <summary>
        /// Do nothing
        /// </summary>
        None,
    }
}