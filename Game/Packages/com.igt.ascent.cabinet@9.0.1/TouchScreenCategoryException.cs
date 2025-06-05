//-----------------------------------------------------------------------
// <copyright file = "TouchScreenCategoryException.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Exception thrown when an error status is received from the TouchScreen category.
    /// </summary>
    public class TouchScreenCategoryException : CabinetCategoryExceptionBase
    {
        /// <summary>
        /// Construct a new instance of the exception.
        /// </summary>
        /// <param name="errorCode">The error code received.</param>
        /// <param name="description">A description of the error.</param>
        public TouchScreenCategoryException(string errorCode, string description)
            : base(errorCode, description)
        {
        }
    }
}