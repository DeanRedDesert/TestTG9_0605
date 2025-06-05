//-----------------------------------------------------------------------
// <copyright file = "InterceptorCommunicationModeChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;

    /// <summary>
    /// Event argument for events that are triggered when the communication mode has been changed.
    /// </summary>
    public class InterceptorCommunicationModeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor for InterceptorCommunicationModeChangedEventArgs
        /// </summary>
        /// <param name="communicationMode">Communication mode.</param>
        public InterceptorCommunicationModeChangedEventArgs(InterceptorCommunicationMode communicationMode)
        {
            CommunicationMode = communicationMode;
        }

        /// <summary>
        /// Gets communication mode.
        /// </summary>
        public InterceptorCommunicationMode CommunicationMode { get; private set; }
    }
}
