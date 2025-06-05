//-----------------------------------------------------------------------
// <copyright file = "IHistoryPresentationState.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.States
{
    /// <summary>
    /// A function to be called when in history mode and the presentation state is complete.
    /// </summary>
    public delegate void HistoryPresentationCompleteDelegate();

    /// <summary>
    /// History presentation state interface, which contains the properties and functions 
    /// that every History presentation interface needs.
    /// </summary>
    public interface IHistoryPresentationState
    {
        /// <summary>
        /// Flag indicating if it is in History mode. 
        /// </summary>
        bool HistoryMode { get; set; }

        /// <summary>
        /// Flag indicating if it is in power hit Recovery mode. 
        /// </summary>
        bool RecoveryMode { get; set; }

        /// <summary>
        /// Defines what triggers this history presentation state to advance to the next history step.
        /// </summary>
        HistoryNextStepTrigger HistoryNextStepTrigger { get; }

        /// <summary>
        /// Function to show this presentation state. 
        /// </summary>
        void ShowHistoryState();

        /// <summary>
        /// Function to hide this presentation state. 
        /// </summary>
        void HideHistoryState();

        /// <summary>
        /// This function handles the History step change event.
        /// History step change event is usually triggered by pressing step buttons. 
        /// </summary>
        /// <param name="sender">The button being pressed.</param>
        /// <param name="eventArgs">Event arguments.</param>
        void OnHistoryStepsChanged(object sender, HistoryStepsChangedEventArgs eventArgs);

        /// <summary>
        /// Set the delegate used to handle PresentationComplete in history mode.
        /// </summary>
        void SetHistoryPresentationCompleteDelegate(HistoryPresentationCompleteDelegate historyPresentationCompleteDelegate);
    }
}