//-----------------------------------------------------------------------
// <copyright file = "GameLogicInterceptorErrorMsg.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;
    using System.Text;

    /// <summary>
    /// Message object that is comprised of the parameters of the IGameLogicInterceptorService
    /// interface function SendErrorMessage.
    /// </summary>
    [Serializable]
    public class GameLogicInterceptorErrorMsg : InterceptorGenericMsg
    {
        /// <summary>
        /// Empty Constructor required for certain types of serialization.
        /// </summary>
        private GameLogicInterceptorErrorMsg() { }

        /// <summary>
        /// Constructor for GameLogicInterceptorErrorMsg.
        /// </summary>
        /// <param name="errorType">Error type encountered.</param>
        /// <param name="errorDescription">
        /// Error string that provides additional information about the error encountered.
        /// </param>
        /// <exception cref="ArgumentException">Thrown if errorString is empty or null.</exception>
        public GameLogicInterceptorErrorMsg(InterceptorError errorType, string errorDescription)
        {
            if (string.IsNullOrEmpty(errorDescription))
            {
                throw new ArgumentException("errorDescription cannot be a null or empty string.", "errorDescription");
            }

            ErrorType = errorType;
            ErrorDescription = errorDescription;
        }

        /// <summary>
        /// Display contents of object as string.
        /// </summary>
        /// <returns>string representation of object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine(base.ToString());
            builder.AppendLine("\tErrorType:" + ErrorType);
            builder.AppendLine("\tErrorDescription:" + ErrorDescription);

            return builder.ToString();
        }

        /// <summary>
        /// Gets error type encountered.
        /// </summary>
        public InterceptorError ErrorType { get; private set; }

        /// <summary>
        /// Gets string that provides additional information about the error encountered.
        /// </summary>
        public string ErrorDescription { get; private set; }
    }
}
