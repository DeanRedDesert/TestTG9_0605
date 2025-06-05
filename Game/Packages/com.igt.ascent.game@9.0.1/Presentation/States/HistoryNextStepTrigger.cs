//-----------------------------------------------------------------------
// <copyright file = "HistoryNextStepTrigger.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.States
{
    /// <summary>
    /// Defines the different way that a history presentation state can trigger the next history step.
    /// </summary>
    /// <remarks>
    /// To select the HistoryNextStepTrigger for a presentation state, you can follow this flowchart:
    /// 
    /// 1. Does the state have an animation or presentation sequence that must complete before
    ///    advancing to the next step?  If yes, go to 2.  If no go to 5.
    /// 2. The state must post a presentation complete at the end of its animation or sequence.  Does the
    ///    state show results that the operator may want to pause to view?  If yes, go to 3.  If no, go to 4.
    /// 3. Use PresentationCompleteThenButton.  This allows the animations to complete, then allows the
    ///    operator to decide when to advance to the next step.
    /// 4. Use PresentationComplete.  This advances to the next step as soon as the animation completes.
    /// 5. Does the state show results that the operator may want to pause to view?  If yes, go to 6.  If no, go to 7.
    /// 6. Use Button.  This allows the operator to decide when to advance to the next step.
    /// 7. Does the state perform all of its functionality in OnEnter()?  If yes, go to 8.  If no, go to 9.
    /// 8. Use Immediate.  This enters the state, then immediately advances to the next state.
    /// 9. Use Button.  This is the default if the presentation can be advanced at any time.
    /// </remarks>
    public enum HistoryNextStepTrigger
    {
        /// <summary>
        /// The operator must press the "Next Step" button to advance to the next history step.
        /// 
        /// Use this when all of the following are true:
        ///   * The presentation doesn't have to complete any animation or presentation sequence before advancing to the next step.
        ///   * The presentation state contains results that the operator may want to pause on.
        /// </summary>
        Button,

        /// <summary>
        /// The state must post a presentation complete message, then the operator must
        /// press the "Next Step" button to advance to the next history step.
        /// 
        /// Use this when all of the following are true:
        ///   * The presentation state posts a presentation complete message.
        ///   * The presentation must complete before advancing to the next step.
        ///   * The presentation state contains results that the operator may want to pause on.
        /// </summary>
        PresentationCompleteThenButton,

        /// <summary>
        /// The state must post a presentation complete message, after which
        /// the history will automatically advance to the next step.
        /// 
        /// Use this when all of the following are true:
        ///   * The presentation state posts a presentation complete message.
        ///   * The presentation state doesn't present any results that the operator may want to pause on.
        /// </summary>
        PresentationComplete,

        /// <summary>
        /// History will display the next step immediately after entering this presentation state.
        /// 
        /// Use this when all of the following are true:
        ///   * The presentation state contains only functionality in its OnEnter() function.
        ///   * The presentation state doesn't present any results that the operator may want to pause on.
        /// </summary>
        Immediate
    }
}
