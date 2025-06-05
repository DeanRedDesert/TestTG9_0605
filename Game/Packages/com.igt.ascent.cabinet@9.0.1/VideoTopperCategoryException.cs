//-----------------------------------------------------------------------
// <copyright file = "VideoTopperCategoryException.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Exception thrown when an error status is received from the video topper category.
    /// </summary>
    public class VideoTopperCategoryException : CabinetCategoryExceptionBase
    {
        /// <summary>
        /// Construct a new instance of the exception.
        /// </summary>
        /// <param name="errorCode">The error code received.</param>
        /// <param name="description">A description of the error.</param>
        public VideoTopperCategoryException(string errorCode, string description)
            : base(errorCode, description)
        {
        }
    }
}
