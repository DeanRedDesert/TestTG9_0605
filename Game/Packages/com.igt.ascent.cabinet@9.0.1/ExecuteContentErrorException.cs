//-----------------------------------------------------------------------
// <copyright file = "ExecuteContentErrorException.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Execute content errors.
    /// </summary>
    public enum ExecuteContentError
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
        /// Error to notify client that the content could not be loaded.
        /// </summary>
        ContentNotLoaded,

        /// <summary>
        /// Error to notify client that they do not own the portal.
        /// </summary>
        ClientDoesNotOwnResource
    }

    /// <summary>
    /// Specific exception for executing content errors.
    /// </summary>
    public class ExecuteContentErrorException : PortalCategoryException
    {
        /// <summary>
        /// The error that caused the exception.
        /// </summary>
        public readonly ExecuteContentError Error;

        /// <summary>
        /// Create a new ExecuteContentErrorException.
        /// </summary>
        /// <param name="error">The error that caused the exception.</param>
        /// <param name="description">The description of the error.</param>
        public ExecuteContentErrorException(ExecuteContentError error,
            string description) : base(error.ToString(), description)
        {
            Error = error;
        }
    }
}
