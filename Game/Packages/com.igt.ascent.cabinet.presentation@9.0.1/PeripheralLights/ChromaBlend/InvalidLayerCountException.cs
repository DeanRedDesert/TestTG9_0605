//-----------------------------------------------------------------------
// <copyright file = "InvalidLayerCountException.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.ChromaBlend
{
    using System;

    /// <summary>
    /// An exception for when a chroma blender has too many layers.
    /// </summary>
    [Serializable]
    public class InvalidLayerCountException : Exception
    {
        /// <summary>
        /// Construct a new exception given a message.
        /// </summary>
        /// <param name="message">The exception error message.</param>
        public InvalidLayerCountException(string message)
            : base(message, null)
        {

        }
    }
}
