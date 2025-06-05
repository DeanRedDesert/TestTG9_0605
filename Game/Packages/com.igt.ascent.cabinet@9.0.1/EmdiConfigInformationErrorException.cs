//-----------------------------------------------------------------------
// <copyright file = "EmdiConfigInformationErrorException.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Specific exception for getting the EMDI config.
    /// </summary>
    public class EmdiConfigInformationErrorException : PortalCategoryException
    {
        /// <summary>
        /// The error that caused the exception.
        /// </summary>
        public readonly EMDIConfigInformationError Error;

        /// <summary>
        /// Create a new EMDIConfigInformationError.
        /// </summary>
        /// <param name="error">The error that caused the exception.</param>
        /// <param name="description">The description of the error.</param>
        public EmdiConfigInformationErrorException(EMDIConfigInformationError error,
            string description) : base(error.ToString(), description)
        {
            Error = error;
        }
    }
}
