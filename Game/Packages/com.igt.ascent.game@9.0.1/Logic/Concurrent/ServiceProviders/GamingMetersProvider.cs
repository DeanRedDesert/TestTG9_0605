// -----------------------------------------------------------------------
// <copyright file = "GamingMetersProvider.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.ServiceProviders
{
    using System;
    using IGT.Ascent.Communication.Platform.ShellLib.Interfaces;
    using IGT.Game.Core.Logic.Services;

    /// <summary>
    /// A provider of the machine's gaming meters displayable to the player.
    /// </summary>
    public class GamingMetersProvider : ObserverProviderBase<GamingMeters>
    {
        #region Constants

        private const string DefaultName = nameof(GamingMetersProvider);
        private const string BettableMeterServiceName = nameof(BettableMeter);
        private const string PaidMeterServiceName = nameof(PaidMeter);
        private const string TransferableMeterServiceName = nameof(TransferableMeter);

        #endregion

        #region Private Fields

        /// <devdoc>
        /// All of the services that are updated together when raising the Asynchronous Provider Changed Event.
        /// </devdoc>
        private static readonly ServiceSignature[] MeterUpdateServiceSignatures =
            {
                new ServiceSignature(BettableMeterServiceName),
                new ServiceSignature(PaidMeterServiceName),
                new ServiceSignature(TransferableMeterServiceName)
            };

        private GamingMeters gamingMeters = new GamingMeters(0, 0, 0);

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public GamingMetersProvider(IObservable<GamingMeters> observable, string name = DefaultName)
            : base(observable, name)
        {
        }

        #endregion

        #region Game Services

        /// <summary>
        /// Gets the amount of money available for player betting that is suitable for display to the player, in base units.
        /// </summary>
        [AsynchronousGameService]
        public long BettableMeter => gamingMeters.Bettable;

        /// <summary>
        /// Gets the amount paid to the player during the last/current cashout,
        /// and is suitable for display to the player, in base units.
        /// </summary>
        [AsynchronousGameService]
        public long PaidMeter => gamingMeters.Paid;

        /// <summary>
        /// Gets the amount of money that is transferable to the player bettable meter,
        /// and is suitable for display to the player, in base units.
        /// </summary>
        [AsynchronousGameService]
        public long TransferableMeter => gamingMeters.Transferable;

        #endregion

        #region ObserverBase Overrides

        /// <inheritdoc/>
        public override void OnNext(GamingMeters value)
        {
            gamingMeters = value;

            OnAsynchronousProviderChanged(new AsynchronousProviderChangedEventArgs(MeterUpdateServiceSignatures, false));
        }

        #endregion
    }
}
