//-----------------------------------------------------------------------
// <copyright file = "PaytableTransformationException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;

    /// <summary>
    /// An exception which is thrown if a paytable transformation is not successful.
    /// </summary>
    [Serializable]
    public class PaytableTransformationException : Exception
    {
        /// <summary>
        /// The format string used to produce this exception's <see cref="Exception.Message"/>.
        /// </summary>
        private const string MessageFormat =
            "An error occurred while transforming the paytable from version {0} to version {1}.";

        private const string FromVersionKey = "FromVersion";
        private const string ToVersionKey = "ToVersion";

        /// <summary>
        /// Gets the <see cref="PaytableVersion"/> that was being transformed from when the error occurred.
        /// </summary>
        public PaytableVersion FromVersion { get; private set; }
        
        /// <summary>
        /// Gets the <see cref="PaytableVersion"/> that was being transformed to when the error occurred.
        /// </summary>
        public PaytableVersion ToVersion { get; private set; }

        /// <summary>
        /// Initializes an instance of the <see cref="PaytableTransformationException"/> class with the given
        /// to and from <see cref="PaytableVersion"/> and the given <see cref="Exception"/>.
        /// </summary>
        /// <param name="fromVersion">The <see cref="PaytableVersion"/> being transformed from.</param>
        /// <param name="toVersion">The <see cref="PaytableVersion"/> being transformed to.</param>
        /// <param name="innerException">The <see cref="Exception"/> that occurred during the transformation.</param>
        public PaytableTransformationException(
            PaytableVersion fromVersion, PaytableVersion toVersion, Exception innerException)
            :base(string.Format(CultureInfo.InvariantCulture, MessageFormat, fromVersion, toVersion), innerException)
        {
            FromVersion = fromVersion;
            ToVersion = toVersion;
        }

        /// <summary>
        /// Initializes an instance of the <see cref="PaytableTransformationException"/> class with the given 
        /// <see cref="SerializationInfo"/> and <see cref="StreamingContext"/>.
        /// </summary>
        /// <param name="info">A <see cref="SerializationInfo"/> containing serialized object data.</param>
        /// <param name="context">
        /// A <see cref="StreamingContext"/> containing additional information about the serialization stream.
        /// </param>
        protected PaytableTransformationException(SerializationInfo info, StreamingContext context)
            :base(info, context)
        {
            FromVersion = (PaytableVersion)info.GetValue(FromVersionKey, typeof(PaytableVersion));
            ToVersion = (PaytableVersion)info.GetValue(ToVersionKey, typeof(PaytableVersion));
        }

        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(FromVersionKey, FromVersion);
            info.AddValue(ToVersionKey, ToVersion);
        }
    }
}
