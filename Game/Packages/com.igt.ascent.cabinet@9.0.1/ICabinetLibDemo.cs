//-----------------------------------------------------------------------
// <copyright file = "ICabinetLibDemo.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// CabinetLib interface which contains functionalities that simulate
    /// the operations outside Cabinet Lib.
    /// </summary>
    public interface ICabinetLibDemo
    {
        /// <summary>
        /// En-queue the given event.
        /// </summary>
        /// <param name="sender">Originator of the event.</param>
        /// <param name="eventArgs">The event to en-queue.</param>
        void EnqueueEvent(object sender, EventArgs eventArgs);

        /// <summary>
        /// Reports the user input.
        /// </summary>
        void ReportUserInput();
    }
}
