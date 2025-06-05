//-----------------------------------------------------------------------
// <copyright file = "TiltSerializationException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces.TiltControl
{
    using System;

    ///<summary>
    /// Used to notify that there was a problem serializing or deserializing a tilt.
    ///</summary>
    [Serializable]
    public class TiltSerializationException : Exception
    {
        /// <summary>
        /// Create a TiltSerializationException with the given message and inner exception.
        /// </summary>
        /// <param name="message">A message describing why the exception occurred.</param>
        /// <param name="inner">The exception thrown by the serialization or deserialization function.</param>
        public TiltSerializationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}