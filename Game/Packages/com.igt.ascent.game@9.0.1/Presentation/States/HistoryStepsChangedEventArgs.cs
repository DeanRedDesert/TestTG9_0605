//-----------------------------------------------------------------------
// <copyright file = "HistoryStepsChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.States
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Arguement in the event posed when History menus step button is pressed.
    /// </summary>
    public class HistoryStepsChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Set to the string name of the button pressed.
        /// </summary>
        public string ActionRequest;

        /// <summary>
        /// The generic data which will be carried back to the logic.
        /// </summary>
        public Dictionary<string, object> Data;

        /// <summary>
        /// Constructor for HistoryStepsButtonPressedEventArgs.
        /// </summary>
        /// <param name="firstStepButtonPressed">Flag indicating if the FirstStep button was pressed.</param>
        public HistoryStepsChangedEventArgs(bool firstStepButtonPressed)
        {
            ActionRequest = firstStepButtonPressed ? "FirstStep" : "NextStep";

            Data = new Dictionary<string, object>();
        }

    }
}