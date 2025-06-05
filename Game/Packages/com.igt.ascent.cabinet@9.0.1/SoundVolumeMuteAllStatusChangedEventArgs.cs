//-----------------------------------------------------------------------
// <copyright file = "SoundVolumeMuteAllStatusChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Event indicating that the sound volume Mute All status has changed.
    /// </summary>
    public class SoundVolumeMuteAllStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The Mute status.
        /// </summary>
        public bool MuteAll { get; }

        /// <summary>
        /// Construct an instance with the given Mute All status.
        /// </summary>
        /// <param name="muteAll">The Mute All status.</param>
        public SoundVolumeMuteAllStatusChangedEventArgs(bool muteAll)
        {
            MuteAll = muteAll;
        }
    }
}
