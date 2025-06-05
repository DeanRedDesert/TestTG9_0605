//-----------------------------------------------------------------------
// <copyright file = "InvalidBetTypeException.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.BetFramework.Exceptions
{
    using System;

    /// <summary>
    /// Thrown if a bet framework object is cast to an unsupported type.
    /// </summary>
    public class InvalidBetTypeException : Exception
    {
        private const string MessageFormat = @"Invalid typecast from {0} to {1}";

        /// <summary>
        /// Original type of the object being cast.
        /// </summary>
        public Type OriginalType { get; private set; }

        /// <summary>
        /// Type cast to.
        /// </summary>
        public Type RequestedType { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="originalType">Original type of the object being cast.</param>
        /// <param name="requestedType">Type cast to.</param>
        public InvalidBetTypeException(Type originalType, Type requestedType)
            : base(string.Format(MessageFormat, originalType, requestedType))
        {
            OriginalType = originalType;
            RequestedType = requestedType;
        }
    }
}
