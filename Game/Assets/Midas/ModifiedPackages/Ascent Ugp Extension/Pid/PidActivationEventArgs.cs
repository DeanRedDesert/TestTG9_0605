//-----------------------------------------------------------------------
// <copyright file = "PidActivationEventArgs.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Pid
{
    using System;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Event arguments for pid being activated.
    /// </summary>
    [Serializable]
    public class PidActivationEventArgs : TransactionalEventArgs
    {
        /// <summary>
        /// Gets or sets the flag indicating if PID is activated or not.
        /// </summary>
        public bool Status { get; set; }
    }
}
