//-----------------------------------------------------------------------
// <copyright file = "ActivityStatusEventArgs.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Event indicating that the activity status has changed.
    /// </summary>
    public class ActivityStatusEventArgs : EventArgs
    {
        /// <summary>
        /// The machine activity status data.
        /// </summary>
        public MachineActivityStatus ActivityStatus { get; }

        /// <summary>
        /// Construct an instance with the given activity status data.
        /// </summary>
        /// <param name="activityStatus">The activity status data.</param>
        public ActivityStatusEventArgs(MachineActivityStatus activityStatus)
        {
            ActivityStatus = activityStatus;
        }
    }
}