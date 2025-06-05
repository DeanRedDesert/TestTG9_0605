//-----------------------------------------------------------------------
// <copyright file = "Playlist.Extension.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization
{
    using System.Linq;

    public partial class Playlist
    {
        /// <summary>
        /// Gets the length of the playlist in milliseconds.
        /// </summary>
        public long TotalDisplayTime
        {
            get
            {
                return Items?.Sum(entry => entry.DisplayTime) ?? 0;
            }
        }
    }
}
