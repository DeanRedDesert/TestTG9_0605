//-----------------------------------------------------------------------
// <copyright file = "CabinetEventArgs.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The event arguments for cabinet events.
    /// </summary>
    public class CabinetEventArgs : EventArgs
    {
        /// <summary>
        /// Construct a new instance given the event name and optional data.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="eventData">Optional data associated with the event.</param>
        public CabinetEventArgs(string eventName, IList<string> eventData)
        {
            EventName = eventName;
            if(eventData != null)
            {
                EventData = eventData;
            }
        }

        #region Properties

        /// <summary>
        /// The event name.
        /// </summary>
        public string EventName
        {
            get;
        }

        /// <summary>
        /// Optional data associated with the event.
        /// </summary>
        public IList<string> EventData
        {
            get;
        }

        #endregion Properties
    }
}
