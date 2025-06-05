//-----------------------------------------------------------------------
// <copyright file = "PlaylistEntry.Extension.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization
{
    public abstract partial class PlaylistEntry
    {
        /// <summary>
        /// Gets the display time of the playlist entry in milliseconds.
        /// </summary>
        public abstract long DisplayTime
        {
            get;
        }

        /// <summary>
        /// Initializes this object using the game mount point.
        /// </summary>
        /// <param name="gameMountPoint">The mount point of the game.</param>
        internal virtual void Initialize(string gameMountPoint)
        {
            
        }
    }
}
