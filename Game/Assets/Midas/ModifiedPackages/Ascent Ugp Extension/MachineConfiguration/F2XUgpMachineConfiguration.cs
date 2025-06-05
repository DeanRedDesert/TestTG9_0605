//-----------------------------------------------------------------------
// <copyright file = "F2XUgpMachineConfiguration.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MachineConfiguration
{
    using System;
    using Ascent.Restricted.EventManagement.Interfaces;
    using Interfaces;

    /// <summary>
    /// Implementation of the UgpMachineConfiguration extended interface that is backed by F2X.
    /// </summary>
    internal class F2XUgpMachineConfiguration : IUgpMachineConfiguration, IInterfaceExtension
    {
        #region Fields

        /// <summary>
        /// The UgpMachineConfiguration category handler.
        /// </summary>
        private readonly IUgpMachineConfigurationCategory ugpMachineConfigurationCategory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="F2XUgpMachineConfiguration"/>.
        /// </summary>
        /// <param name="ugpMachineConfigurationCategory">
        /// The UgpMachineConfiguration category used to communicate with the foundation.
        /// </param>
        /// <param name="transactionalEventDispatcher">
        /// Interface for processing a transactional event.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the arguments is null.
        /// </exception>
        public F2XUgpMachineConfiguration(IUgpMachineConfigurationCategory ugpMachineConfigurationCategory,
                                          IEventDispatcher transactionalEventDispatcher)
        {
            if(transactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(transactionalEventDispatcher));
            }

            this.ugpMachineConfigurationCategory = ugpMachineConfigurationCategory ?? throw new ArgumentNullException(nameof(ugpMachineConfigurationCategory));

            transactionalEventDispatcher.EventDispatchedEvent +=
                            (sender, dispatchedEvent) => dispatchedEvent.RaiseWith(this, MachineConfigurationChanged);
        }

        #endregion

        #region IUgpMachineConfiguration Implementation

        /// <inheritdoc/>
        public event EventHandler<MachineConfigurationChangedEventArgs> MachineConfigurationChanged;

        /// <inheritdoc/>
		/// <inheritdoc/>
		public MachineConfigurationParameters GetMachineConfigurationParameters()
		{
			return ugpMachineConfigurationCategory.GetMachineConfigurationParameters();
		}

        #endregion
    }
}
