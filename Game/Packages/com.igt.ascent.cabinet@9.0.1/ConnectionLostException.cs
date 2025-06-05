//-----------------------------------------------------------------------
// <copyright file = "ConnectionLostException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Exception to be thrown when the buttons are not disconnected.
    /// </summary>
    public class ConnectionLostException:ButtonCategoryException
    {
        /// <summary>
        /// Construct a new instance of the exception.
        /// </summary>
        /// <param name="errorCode">The error code received.</param>
        /// <param name="description">A description of the error.</param>
        public ConnectionLostException(string errorCode, string description) : base(errorCode, description)
        {
        }
    }
}
