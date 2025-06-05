//-----------------------------------------------------------------------
// <copyright file = "ShowFunctionException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception which indicates that a show mode function was called while not in show mode.
    /// </summary>
    public class ShowFunctionException : Exception
    {
        /// <summary>
        /// Message format for the exception.
        /// </summary>
        private const string MessageFormat = "Function: {0} may only be called in show mode.";

        /// <summary>
        /// The name of the method which was called while not in show mode.
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Construct an instance of the exception.
        /// </summary>
        /// <param name="functionName">
        /// A <see cref="string"/> containing the name of the function which was called.
        /// </param>
        public ShowFunctionException(string functionName) : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, functionName))
        {
            MethodName = functionName;
        }
    }
}

