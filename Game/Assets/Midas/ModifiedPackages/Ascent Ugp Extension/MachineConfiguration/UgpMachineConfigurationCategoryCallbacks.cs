//-----------------------------------------------------------------------
// <copyright file = "UgpMachineConfigurationCategoryCallbacks.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MachineConfiguration
{
    using System;
    using F2XTransport;

    /// <summary>
    /// This class is responsible for handling callbacks from the UgpMachineConfiguration category.
    /// </summary>
    class UgpMachineConfigurationCategoryCallbacks : IUgpMachineConfigurationCategoryCallbacks
    {
        /// <summary>
        /// The callback interface for handling transactional events.
        /// </summary>
        private readonly IEventCallbacks eventCallbacksInterface;

        /// <summary>
        /// Initializes an instance of <see cref="UgpMachineConfigurationCategoryCallbacks"/>.
        /// </summary>
        /// <param name="eventCallbacksInterface">
        /// The callback interface for the handling transactional events.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacksInterface"/> is null.
        /// </exception>
        public UgpMachineConfigurationCategoryCallbacks(IEventCallbacks eventCallbacksInterface)
        {
            this.eventCallbacksInterface = eventCallbacksInterface ?? throw new ArgumentNullException(nameof(eventCallbacksInterface));
        }

        #region IUgpMachineConfigurationCategoryCallbacks Implementation

        /// <inheritdoc />
        public string ProcessSetParameters(MachineConfigurationParameters parameters)
        {
            eventCallbacksInterface.PostEvent(new MachineConfigurationChangedEventArgs(parameters));
            return null;
        }

        #endregion
    }
}
