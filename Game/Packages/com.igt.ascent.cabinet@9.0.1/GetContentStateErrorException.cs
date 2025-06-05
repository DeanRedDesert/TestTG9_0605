//-----------------------------------------------------------------------
// <copyright file = "GetContentStateErrorException.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Getting content state errors.
    /// </summary>
    public enum GetContentStateError
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
    /// Specific exception for getting the content state.
    /// </summary>
    public class GetContentStateErrorException : PortalCategoryException
    {
        /// <summary>
        /// The error that caused the exception.
        /// </summary>
        public readonly GetContentStateError Error;

        /// <summary>
        /// Create a new GetContentStateErrorException.
        /// </summary>
        /// <param name="error">The error that caused the exception.</param>
        /// <param name="description">The description of the error.</param>
        public GetContentStateErrorException(GetContentStateError error,
            string description) : base(error.ToString(), description)
        {
            Error = error;
        }
    }
}
