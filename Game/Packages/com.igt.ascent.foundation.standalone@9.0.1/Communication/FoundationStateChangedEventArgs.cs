//-----------------------------------------------------------------------
// <copyright file = "FoundationStateChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Standalone
{
    using System;

    /// <summary>
    /// Event indicating the Foundation State has changed.
    /// </summary>
    public class FoundationStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor for FoundationStateChangedEventArgs
        /// </summary>
        /// <param name="isStateIdle">True if the Foundation State is Idle, false otherwise.</param>
        public FoundationStateChangedEventArgs(bool isStateIdle)
        {
            IsStateIdle = isStateIdle;
        }

        /// <summary>
        /// True if the Foundation State is Idle, false otherwise.
        /// </summary>
        public bool IsStateIdle { get; }
    }
}
