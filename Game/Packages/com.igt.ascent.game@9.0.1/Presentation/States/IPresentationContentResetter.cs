//-----------------------------------------------------------------------
// <copyright file = "IPresentationContentResetter.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.States
{
    /// <summary>
    /// The <see cref="IPresentationContentResetter"/> interface provides functions to reset presentation content
    /// to its start-up state. Typically, this is done by unloading and reloading scenes.
    /// </summary>
    public interface IPresentationContentResetter : IPresentationContentResetQuery
    {
        /// <summary>
        /// Requests to reset the content at the first opportunity.
        /// </summary>
        void RequestReset();

        /// <summary>
        /// Starts resetting the content.
        /// </summary>
        void ResetContent();
    }
}
