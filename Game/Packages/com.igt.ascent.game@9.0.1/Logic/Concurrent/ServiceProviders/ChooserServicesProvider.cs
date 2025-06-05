// -----------------------------------------------------------------------
// <copyright file = "ChooserServicesProvider.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.ServiceProviders
{
    using System;
    using IGT.Ascent.Communication.Platform.Interfaces;
    using IGT.Game.Core.Logic.Services;

    /// <summary>
    /// A provider of the machine's chooser services.
    /// </summary>
    public class ChooserServicesProvider : ObserverProviderBase<ChooserProperties>
    {
        #region Constants

        private const string DefaultName = nameof(ChooserServicesProvider);
        private const string OfferableServiceName = nameof(Offerable);

        #endregion

        #region Private Fields

        /// <devdoc>
        /// All of the services that are updated together when raising the Asynchronous Provider Changed Event.
        /// </devdoc>
        private static readonly ServiceSignature[] ChooserUpdateServiceSignatures =
            {
                new ServiceSignature(OfferableServiceName)
            };

        private ChooserProperties chooserProperties = new ChooserProperties(false);

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public ChooserServicesProvider(IObservable<ChooserProperties> observable, string name = DefaultName)
            : base(observable, name)
        {
        }

        #endregion

        #region Game Services

        /// <summary>
        /// Gets whether the chooser should be available for the player to request.
        /// </summary>
        [AsynchronousGameService]
        public bool Offerable => chooserProperties.Offerable;

        #endregion

        /// <inheritdoc/>
        public override void OnNext(ChooserProperties value)
        {
            if(value == null)
            {
                return;
            }

            chooserProperties = value;

            // TODO: provide a way to determine whether an update is transactional or not
            OnAsynchronousProviderChanged(new AsynchronousProviderChangedEventArgs(ChooserUpdateServiceSignatures, false));
        }
    }
}
