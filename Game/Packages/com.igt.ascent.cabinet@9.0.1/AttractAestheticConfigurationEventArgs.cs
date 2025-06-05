//-----------------------------------------------------------------------
// <copyright file = "AttractAestheticConfigurationEventArgs.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using CSI.Schemas;

    /// <summary>
    /// The event arguments for the attract aesthetic configuration.
    /// </summary>
    public class AttractAestheticConfigurationEventArgs : EventArgs
    {
        /// <summary>
        /// Construct a new instance given the playlist group number and the attract style.
        /// </summary>
        /// <param name="playlistGroup">The playlist group number.</param>
        /// <param name="attractStyle">The attract style.</param>
        public AttractAestheticConfigurationEventArgs(uint playlistGroup, GameAttractStyle attractStyle)
        {
            PlaylistGroup = playlistGroup;
            AttractStyle = attractStyle;
        }

        #region Properties

        /// <summary>
        /// Gets the current attract playlist group number.
        /// </summary>
        public uint PlaylistGroup
        {
            get;
        }

        /// <summary>
        /// Gets the current attract style.
        /// </summary>
        public GameAttractStyle AttractStyle
        {
            get;
        }

        #endregion
    }
}
