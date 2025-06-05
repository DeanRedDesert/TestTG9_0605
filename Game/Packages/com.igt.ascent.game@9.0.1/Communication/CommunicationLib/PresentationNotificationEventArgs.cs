//-----------------------------------------------------------------------
// <copyright file = "PresentationNotificationEventArgs.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.CommunicationLib
{
    using System;

    /// <summary>
    /// This class is for notification of StartState and UpdateAsynchData messages being sent from the
    /// game logic to the presentation.
    /// </summary>
    public class PresentationNotificationEventArgs : EventArgs
    {
        /// <summary>
        /// The state which the notification is for.
        /// </summary>
        public string StateName
        { 
            private set;
            get;
        }

        /// <summary>
        /// The data which is associated with the notifications corresponding message.
        /// </summary>
        public DataItems Data
        {
            private set;
            get;
        }

        /// <summary>
        /// Construct an instance of PresentationNotificationEventArgs.
        /// </summary>
        /// <param name="stateName">The state which the notification is for.</param>
        /// <param name="data">The data from the message.</param>
        public PresentationNotificationEventArgs(string stateName, DataItems data)
        {
            StateName = stateName;
            Data = data;
        }
    }
}
