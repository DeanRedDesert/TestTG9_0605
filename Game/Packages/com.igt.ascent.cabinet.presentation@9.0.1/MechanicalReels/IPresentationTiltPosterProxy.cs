// -----------------------------------------------------------------------
// <copyright file = "IPresentationTiltPosterProxy" company = "IGT">
//     Copyright (c) 2024 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.MechanicalReels
{
    using Tilts;

    /// <summary>
    /// This interface defines a proxy to the presentation tilt poster.
    /// </summary>
    public interface IPresentationTiltPosterProxy
    {   
        /// <summary>
        /// Gets the initialization state of the presentation tilt poster.
        /// </summary>
        bool Initialized { get; }
        
        /// <summary>
        /// Post a presentation tilt whose title and message do not require
        /// argument objects for formatting.
        /// </summary>
        /// <param name="tiltKey">The key used to track the presentation tilt.</param>
        /// <param name="presentationTilt">The presentation tilt to post.</param>
        void PostTilt(string tiltKey, ITilt presentationTilt);

        /// <summary>
        /// Clear a presentation tilt.
        /// </summary>
        /// <param name="tiltKey">The key of the presentation tilt to be cleared.</param>
        void ClearTilt(string tiltKey);
    }
}