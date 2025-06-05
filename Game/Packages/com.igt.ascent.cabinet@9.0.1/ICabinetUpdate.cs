//-----------------------------------------------------------------------
// <copyright file = "ICabinetUpdate.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Interface that should be implemented by a cabinet category if it needs regular updates synced
    /// with the cabinet lib update. Usually called from a game's frame timer and used to dequeue events.
    /// </summary>
    public interface ICabinetUpdate
    {
        /// <summary>
        /// Perform any service that needs to be done in sync. with the cabinet lib Update().
        /// </summary>
        void Update();
    }
}