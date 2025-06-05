//-----------------------------------------------------------------------
// <copyright file = "IPresentationTransition.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.CommunicationLib
{
    /// <summary>Interface for the presentation transition.</summary>
    public interface IPresentationTransition
    {
        /// <summary>
        /// Park the presentation.
        /// </summary>
        void Park();

        /// <summary>
        /// Un-park the presentation.
        /// </summary>
        void Unpark();

        /// <summary>
        /// Exit from the play context.
        /// </summary>
        void ExitPlayContext();

        /// <summary>
        /// Exit from the history context.
        /// </summary>
        void ExitHistoryContext();

        /// <summary>
        /// Exit from the utility context.
        /// </summary>
        void ExitUtilityContext();

        /// <summary>
        /// Enter the history context after exiting the play context.
        /// </summary>
        void EnterHistoryFromPlayContext();

        /// <summary>
        /// Enter the utility context after exiting the play context.
        /// </summary>
        void EnterUtilityFromPlayContext();
    }
}
