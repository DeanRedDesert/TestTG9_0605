// -----------------------------------------------------------------------
// <copyright file = "PresentationTiltPosterNotImplementedException.cs" company = "IGT">
//     Copyright (c) 2024 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.MechanicalReels
{
    using System;

    /// <summary>
    /// Thrown when the PresentationTiltPoster cannot be found. 
    /// </summary>
    public class PresentationTiltPosterNotImplementedException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public PresentationTiltPosterNotImplementedException(string message)
            : base(message + " Please call MechanicalReelLocalizedTiltHelper.SetTiltPoster() to" +
                   " set  a custom IPresentationTiltPosterProxy implementation.")
        {
        }
    }
}