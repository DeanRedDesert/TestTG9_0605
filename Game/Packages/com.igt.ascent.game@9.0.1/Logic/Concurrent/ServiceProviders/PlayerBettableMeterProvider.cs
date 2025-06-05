// -----------------------------------------------------------------------
// <copyright file = "PlayerBettableMeterProvider.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.ServiceProviders
{
    using System;
    using Game.Core.Logic.Services;

    /// <summary>
    /// A provider for the Player Bettable Meter of the coplayer.
    /// </summary>
    public sealed class PlayerBettableMeterProvider : ObserverProviderBase<long>
    {
        #region Constants

        private const string DefaultName = nameof(PlayerBettableMeterProvider);
        private static readonly ServiceSignature PlayerBettableMeterSignature = new ServiceSignature(nameof(PlayerBettableMeter));

        #endregion

        #region Game Services

        /// <summary>
        /// Gets the current Can Bet flag of the coplayer.
        /// </summary>
        [AsynchronousGameService]
        public long PlayerBettableMeter { get; private set; }

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public PlayerBettableMeterProvider(IObservable<long> observable, string name = DefaultName)
            : base(observable, name)
        {
        }

        #endregion

        #region Observer

        /// <inheritdoc/>
        public override void OnNext(long value)
        {
            if(PlayerBettableMeter == value)
            {
                return;
            }

            PlayerBettableMeter = value;

            OnAsynchronousProviderChanged(new AsynchronousProviderChangedEventArgs(PlayerBettableMeterSignature, false));
        }

        #endregion
    }
}