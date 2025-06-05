//-----------------------------------------------------------------------
// <copyright file = "PriorityMuteState.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using CSI.Schemas;

    /// <summary>
    /// Structure used to represent the settings for Priority-Mute State pair.
    /// </summary>
    public struct PriorityMuteState
    {
        /// <summary>
        /// The priority value for which the mute state is associated.
        /// </summary>
        public Priority PriorityType { get; }

        /// <summary>
        /// True if the associated priority is or is intended to be muted, 
        /// or false if the associated priority is or is intended to be unmuted.
        /// </summary>
        public bool Muted { get; }

        /// <summary>
        /// Instantiate an instance of the <see cref="PriorityMuteState"/> structure.
        /// </summary>
        /// <param name="priorityType">
        /// The priority value for which the mute state is associated.
        /// </param>
        /// <param name="muted">
        /// True if the associated priority is or is intended to be muted, 
        /// or false if the associated priority is or is intended to be unmuted.
        /// </param>
        public PriorityMuteState(Priority priorityType, bool muted) : this()
        {
            PriorityType = priorityType;
            Muted = muted;
        }
    }
}
