//-----------------------------------------------------------------------
// <copyright file = "PidServiceRequestedChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Pid
{
    using System;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Event arguments for requested pid service being changed.
    /// </summary>
    [Serializable]
    public class PidServiceRequestedChangedEventArgs : TransactionalEventArgs
    {
        /// <summary>
        /// Gets or sets the flag indicating if PID service is requested or not.
        /// </summary>
        public bool IsServiceRequested { get; set; }
    }
}
