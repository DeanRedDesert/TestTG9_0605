//-----------------------------------------------------------------------
// <copyright file = "WindowZOrderEventArgs.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using CSI.Schemas;

    /// <summary>
    /// Event indicating that the window z order should be changed.
    /// </summary>
    public class WindowZOrderEventArgs : EventArgs
    {
        /// <summary>
        /// Desired window z position.
        /// </summary>
        public ChangeZOrderRequest Position { get; }

        /// <summary>
        /// Construct an instance with the given z position.
        /// </summary>
        /// <param name="position">Desired position of the window.</param>
        public WindowZOrderEventArgs(ChangeZOrderRequest position)
        {
            Position = position;
        }
    }
}