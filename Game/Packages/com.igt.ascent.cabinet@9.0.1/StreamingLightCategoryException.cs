//-----------------------------------------------------------------------
// <copyright file = "StreamingLightCategoryException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Exception thrown when an error status is received from the streaming light category.
    /// </summary>
    public class StreamingLightCategoryException : CabinetCategoryExceptionBase
    {
        /// <summary>
        /// Construct a new instance of the exception.
        /// </summary>
        /// <param name="errorCode">The error code received.</param>
        /// <param name="description">A description of the error.</param>
        public StreamingLightCategoryException(string errorCode, string description)
            : base(errorCode, description)
        {
        }
    }
}
