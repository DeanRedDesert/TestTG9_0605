//-----------------------------------------------------------------------
// <copyright file = "ChooserServicesCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using Ascent.Communication.Platform.Interfaces;
    using F2X;
    using F2XTransport;
    using F2XChooserProperties = F2X.Schemas.Internal.ChooserServices.ChooserServicesProperties;

    /// <summary>
    /// This class is responsible for handling callbacks from the <see cref="ChooserServicesCategory"/>.
    /// </summary>
    internal class ChooserServicesCallbackHandler : IChooserServicesCategoryCallbacks
    {
        #region Private Fields

        /// <summary>
        /// The callback interface for handling non transactional events.
        /// </summary>
        private readonly INonTransactionalEventCallbacks nonTransactionalEventCallbacks;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an instance of the <see cref="ChooserServicesCallbackHandler"/>.
        /// </summary>
        /// <param name="nonTransactionalEventCallbacks">
        /// The callback interface for handling non transactional events.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="nonTransactionalEventCallbacks"/> is null.
        /// </exception>
        public ChooserServicesCallbackHandler(INonTransactionalEventCallbacks nonTransactionalEventCallbacks)
        {
            this.nonTransactionalEventCallbacks = nonTransactionalEventCallbacks ?? throw new ArgumentNullException(nameof(nonTransactionalEventCallbacks));
        }

        #endregion

        #region IChooserServicesCategoryCallbacks

        /// <inheritdoc/>
        public string ProcessUpdateChooserServicesProperties(F2XChooserProperties chooserServicesProperties)
        {
            nonTransactionalEventCallbacks.EnqueueEvent(new ChooserPropertiesUpdateEventArgs(
                chooserServicesProperties.OfferableSpecified ? (bool?)chooserServicesProperties.Offerable : null));

            return null;
        }

        #endregion
    }
}