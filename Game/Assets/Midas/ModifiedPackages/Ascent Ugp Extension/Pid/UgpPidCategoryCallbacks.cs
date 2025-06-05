//-----------------------------------------------------------------------
// <copyright file = "UgpPidCategoryCallbacks.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Pid
{
    using System;
    using F2XTransport;

    /// <summary>
    /// This class is responsible for handling callbacks from the UgpPid category.
    /// </summary>
    class UgpPidCategoryCallbacks : IUgpPidCategoryCallbacks
    {
        /// <summary>
        /// The callback interface for handling transactional events.
        /// </summary>
        private readonly IEventCallbacks eventCallbacksInterface;

        /// <summary>
        /// Initializes an instance of <see cref="UgpPidCategoryCallbacks"/>.
        /// </summary>
        /// <param name="eventCallbacksInterface">
        /// The callback interface for the handling transactional events.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacksInterface"/> is null.
        /// </exception>
        public UgpPidCategoryCallbacks(IEventCallbacks eventCallbacksInterface)
        {
            this.eventCallbacksInterface = eventCallbacksInterface ?? throw new ArgumentNullException(nameof(eventCallbacksInterface));
        }

        #region IUgpPidCategoryCallbacks Implementation

        /// <inheritdoc />
        public string ProcessPidConfigurationChanged()
        {
            eventCallbacksInterface.PostEvent(new PidConfigurationChangedEventArgs());
            return null;
        }

        /// <inheritdoc />
        public string ProcessPidActivation(bool status)
        {
            eventCallbacksInterface.PostEvent(new PidActivationEventArgs { Status = status });
            return null;
        }

        /// <inheritdoc />
        public string NotifyAttendantServiceRequested(bool isServiceRequested)
        {
            eventCallbacksInterface.PostEvent(
                        new PidServiceRequestedChangedEventArgs { IsServiceRequested = isServiceRequested });
            return null;
        }

        #endregion
    }
}
