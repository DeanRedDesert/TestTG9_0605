//-----------------------------------------------------------------------
// <copyright file = "IGameLibEventListener.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    /// <summary>
    /// This interface defines a method to unregister the Game Lib event handlers.
    /// It should be implemented by all value types and classes that listen to any
    /// Game Lib event.
    /// </summary>
    public interface IGameLibEventListener
    {
        /// <summary>
        /// Unregister all the Game Lib event handlers that have been registered.
        /// </summary>
        /// <param name="gameLib">Reference to the Game Lib.</param>
        void UnregisterGameLibEvents(IGameLib gameLib);
    }
}
