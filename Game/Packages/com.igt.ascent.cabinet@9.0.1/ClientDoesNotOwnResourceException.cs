//-----------------------------------------------------------------------
// <copyright file = "ClientDoesNotOwnResourceException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Exception thrown when the client loses control of the buttons.
    /// </summary>
    public class ClientDoesNotOwnResourceException:ButtonCategoryException
    {
        /// <summary>
        /// Construct a new instance of the exception.
        /// </summary>
        /// <param name="errorCode">The error code received.</param>
        /// <param name="description">A description of the error.</param>
        public ClientDoesNotOwnResourceException(string errorCode, string description) : base(errorCode, description)
        {
        }
    }
}
