//-----------------------------------------------------------------------
// <copyright file = "IGameCycleEventsCategoryCallbacks.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// <auto-generated>
//     This code was generated by C3G.
//
//     Changes to this file may cause incorrect behavior
//     and will be lost if the code is regenerated.
// </auto-generated>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2X
{
    using Schemas.Internal.GameCycleEvents;

    /// <summary>
    /// Interface that handles callbacks from the F2X <see cref="GameCycleEvents"/> category.
    /// Game Cycle Events category of messages.  Category: 3017  Version: 1
    /// Category: 3017; Major Version: 1
    /// </summary>
    /// <remarks>
    /// All documentation is generated from the XSD schema files.
    /// </remarks>
    public interface IGameCycleEventsCategoryCallbacks
    {
        /// <summary>
        /// A registered game cycle event has occurred.
        /// </summary>
        /// <param name="gameCycleEvent">
        /// Event that occurred.
        /// </param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessGameCycleEvent(GameCycleEventType gameCycleEvent);

    }

}

