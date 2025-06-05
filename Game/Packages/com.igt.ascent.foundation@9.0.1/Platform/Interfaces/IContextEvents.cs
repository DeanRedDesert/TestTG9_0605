//-----------------------------------------------------------------------
// <copyright file = "IContextEvents.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// Interface for providing current context events.
    /// </summary>
    public interface IContextEvents
    {
       /// <summary>
        /// Event handler of ActivateContextEvent.
        /// </summary>
        event EventHandler<ActivateContextEventArgs> ActivateContextEvent;

        /// <summary>
        /// Event handler of InactivateContextEvent.
        /// </summary>
        event EventHandler<InactivateContextEventArgs> InactivateContextEvent;
    }
}
