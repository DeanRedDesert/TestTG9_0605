//-----------------------------------------------------------------------
// <copyright file = "TiltControlCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using Ascent.Communication.Platform.Interfaces.TiltControl;
    using F2X;
    using F2XTransport;

    /// <summary>
    /// This class implements callback methods for the TiltControl F2X category.
    /// </summary>
    internal class TiltControlCallbackHandler : ITiltControlCategoryCallbacks
    {
        /// <summary>
        /// The callback interface for handling events.
        /// </summary>
        private readonly IEventCallbacks eventCallbacks;

        /// <summary>
        /// Initializes an instance of <see cref="TiltControlCallbackHandler"/>.
        /// </summary>
        /// <param name="eventCallbacks">The callback interface for handling events.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacks"/> is null.
        /// </exception>
        public TiltControlCallbackHandler(IEventCallbacks eventCallbacks)
        {
            this.eventCallbacks = eventCallbacks ?? throw new ArgumentNullException(nameof(eventCallbacks));
        }

        #region Implementation of ITiltControlCategoryCallbacks

        /// <inheritdoc />
        public string ProcessTiltClearedByAttendant(string tiltName)
        {
            var eventArgs = new TiltClearedByAttendantEventArgs(tiltName);

            eventCallbacks.PostEvent(eventArgs);

            return null;
        }

        #endregion
    }
}
