//-----------------------------------------------------------------------
// <copyright file = "ReleaseContentErrorException.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Release content errors.
    /// </summary>
    public enum ReleaseContentError
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
        InvalidPortalId,

        /// <summary>
        /// Error to notify client that the content was not loaded.
        /// </summary>
        ContentNotLoaded,

        /// <summary>
        /// Error to notify client that they do not own the portal.
        /// </summary>
        ClientDoesNotOwnResource
    }

    /// <summary>
    /// Specific exceptions for releasing content on a portal.
    /// </summary>
    public class ReleaseContentErrorException : PortalCategoryException
    {
        /// <summary>
        /// The error that caused the exception.
        /// </summary>
        public readonly ReleaseContentError Error;

        /// <summary>
        /// Create a new ReleaseContentErrorException.
        /// </summary>
        /// <param name="error">The error that caused the exception.</param>
        /// <param name="description">The description of the error.</param>
        public ReleaseContentErrorException(ReleaseContentError error,
            string description) : base(error.ToString(), description)
        {
            Error = error;
        }
    }
}
