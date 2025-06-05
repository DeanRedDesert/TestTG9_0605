//-----------------------------------------------------------------------
// <copyright file = "PresentationTransition.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Presentation.CommServices
{
    /// <summary>
    /// Presentation Transition enumeration type.
    /// </summary>
    public enum PresentationTransition
    {
        /// <summary>
        /// It's not a presentation transition.
        /// </summary>
        Invalid,
        /// <summary>
        /// Presentation transition for park.
        /// </summary>
        Park,
        /// <summary>
        /// Presentation transition for Unpark.
        /// </summary>
        Unpark,
        /// <summary>
        /// Presentation transition for exiting from play context mode.
        /// </summary>
        ExitPlayContext,
        /// <summary>
        /// Presentation transition for exiting from history context mode.
        /// </summary>
        ExitHistoryContext,
        /// <summary>
        /// Presentation transition for exiting from utility context mode.
        /// </summary>
        ExitUtilityContext,
        /// <summary>
        /// Presentation transition for entering history context mode after exiting play context mode.
        /// </summary>
        EnterHistoryFromPlayContext,
        /// <summary>
        /// Presentation transition for entering utility context mode after exiting play context mode.
        /// </summary>
        EnterUtilityFromPlayContext,
    }
}
