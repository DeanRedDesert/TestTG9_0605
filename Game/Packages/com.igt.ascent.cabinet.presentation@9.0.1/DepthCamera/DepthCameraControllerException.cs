//-----------------------------------------------------------------------
// <copyright file = "DepthCameraControllerException.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.DepthCamera
{
    using System;

    /// <summary>
    /// Exception thrown by the <see cref="DepthCameraController"/>
    /// </summary>
    public class DepthCameraControllerException : Exception
    {
        /// <summary>
        /// Initializes the exception.
        /// </summary>
        /// <param name="message">Description of the exception.</param>
        public DepthCameraControllerException(string message)
            : base(message)
        {
        }
    }
}
