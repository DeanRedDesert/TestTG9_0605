//-----------------------------------------------------------------------
// <copyright file = "ConfigurationCacheConsistencyException.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard
{
    using System;
    using System.Globalization;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Thrown when there is an issue with the consistency of the configuration item cache.
    /// </summary>
    [Serializable]
    class ConfigurationCacheConsistencyException : Exception
    {
        private const string TypeMismatchFormatString = "The type requested from the cache does not match the" +
                                                        " cached type. Requested type: {0} Cached Type: {1}";

        /// <summary>
        /// Construct an instance of the exception with a message.
        /// </summary>
        /// <param name="message">Message for the exception.</param>
        public ConfigurationCacheConsistencyException(string message) : base(message)
        {
        }

        /// <summary>
        /// Create an instance of the exception for a mismatch of the cached type and the expected type from the caller.
        /// </summary>
        /// <param name="expectedType">The type the caller expected.</param>
        /// <param name="cachedType">The type that was cached.</param>
        public ConfigurationCacheConsistencyException(ConfigurationItemType expectedType, ConfigurationItemType cachedType)
            : base(string.Format(CultureInfo.InvariantCulture, TypeMismatchFormatString, expectedType, cachedType))
        {
        }
   }
}
