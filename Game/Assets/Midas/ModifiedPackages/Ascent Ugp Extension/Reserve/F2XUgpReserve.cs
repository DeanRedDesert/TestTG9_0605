//-----------------------------------------------------------------------
// <copyright file = "F2XUgpReserve.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Reserve
{
    using System;
    using Ascent.Restricted.EventManagement.Interfaces;
    using Interfaces;

    /// <summary>
    /// Implementation of the UgpRandomNumber extended interface that is backed by F2X.
    /// </summary>
    internal class F2XUgpReserve : IUgpReserve, IInterfaceExtension
    {
        #region Private Fields

        /// <summary>
        /// The UgpReserve category handler.
        /// </summary>
        private readonly IUgpReserveCategory ugpReserveCategory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="F2XUgpReserve"/>.
        /// </summary>
        /// <param name="ugpReserveCategory">
        /// The UgpReserve category handler used to communicate with the foundation.
        /// </param>
        /// <param name="transactionalEventDispatcher">
        /// Interface for processing a transactional event.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the arguments is null.
        /// </exception>
        public F2XUgpReserve(IUgpReserveCategory ugpReserveCategory,
                             IEventDispatcher transactionalEventDispatcher)
        {
            if(transactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(transactionalEventDispatcher));
            }

            this.ugpReserveCategory = ugpReserveCategory ?? throw new ArgumentNullException(nameof(ugpReserveCategory));

            transactionalEventDispatcher.EventDispatchedEvent +=
                            (sender, dispatchedEvent) => dispatchedEvent.RaiseWith(this, ReserveParametersChanged);
        }

        #endregion

        #region IUgpReserve Implementation

        /// <inheritdoc/>
        public event EventHandler<ReserveParametersChangedEventArgs> ReserveParametersChanged;

        /// <inheritdoc/>
		public ReserveParameters GetReserveParameters()
		{
			return ugpReserveCategory.GetReserveParameters();
		}

        /// <inheritdoc/>
        public void SendActivationChanged(bool isActive)
        {
            ugpReserveCategory.SendActivationChanged(isActive);
        }

        #endregion
    }
}
