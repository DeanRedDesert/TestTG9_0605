//-----------------------------------------------------------------------
// <copyright file = "GetVisibilityStateErrorException.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Getting visibility state errors.
    /// </summary>
    public enum GetVisibilityStateError
    {
        /// <summary>
        /// No error.
        /// </summary>
        None,

        /// <summary>
        /// Some undefined error occured.
        /// </summary>
        OtherError,

        /// <summary>
        /// Error to notify client that an invalid portal id was sent.
        /// </summary>
        InvalidPortalId
    }

    /// <summary>
    /// Specific exception for getting the visibility state.
    /// </summary>
    public class GetVisibilityStateErrorException : PortalCategoryException
    {
        /// <summary>
        /// The error that caused the exception.
        /// </summary>
        public readonly GetVisibilityStateError Error;

        /// <summary>
        /// Create a new GetVisibilityStateErrorException.
        /// </summary>
        /// <param name="error">The error that caused the exception.</param>
        /// <param name="description">The description of the error.</param>
        public GetVisibilityStateErrorException(GetVisibilityStateError error,
            string description) : base(error.ToString(), description)
        {
            Error = error;
        }
    }
}
