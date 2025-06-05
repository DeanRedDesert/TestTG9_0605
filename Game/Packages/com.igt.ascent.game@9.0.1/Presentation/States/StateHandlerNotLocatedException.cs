//-----------------------------------------------------------------------
// <copyright file = "StateHandlerNotLocatedException.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.States
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;

    /// <summary>
    /// An exception to throw when a state handler locator can not locate the
    /// state handler it is configured for.
    /// </summary>
    [Serializable]
    public class StateHandlerNotLocatedException : Exception
    {
        private const string MessageFormatString = "The handler for state {0} could not be located.\nMessage: {1}";
        private const string StateNameKey = "StateName";

        /// <summary>
        /// The name of the state that the handler could not be located for.
        /// </summary>
        public string StateName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateHandlerNotLocatedException"/> class.
        /// </summary>
        /// <param name="message">A message describing the error.</param>
        /// <param name="stateName">The name of the state the handler could not be located for.</param>
        public StateHandlerNotLocatedException(string message, string stateName)
            :base(string.Format(CultureInfo.InvariantCulture, MessageFormatString, stateName, message))
        {
            StateName = stateName;
        }

        /// <summary>
        /// Serialization constructor.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> containing the serialized data.</param>
        /// <param name="context">
        /// A <see cref="StreamingContext"/> object containing additional context for the serialization operation.
        /// </param>
        protected StateHandlerNotLocatedException(SerializationInfo info, StreamingContext context)
            :base(info, context)
        {
            StateName = info.GetString(StateNameKey);
        }

        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(StateNameKey, StateName);
        }
    }
}
