// -----------------------------------------------------------------------
// <copyright file = "CanBetFlagProvider.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.ServiceProviders
{
    using System;
    using Game.Core.Logic.Services;

    /// <summary>
    /// A provider for the current Can Bet Flag of the coplayer.
    /// </summary>
    public sealed class CanBetFlagProvider : ObserverProviderBase<bool>
    {
        #region Constants

        private const string DefaultName = nameof(CanBetFlagProvider);
        private static readonly ServiceSignature CanBetFlagSignature = new ServiceSignature(nameof(CanBetFlag));

        #endregion

        #region Game Services

        /// <summary>
        /// Gets the current Can Bet flag of the coplayer.
        /// </summary>
        [AsynchronousGameService]
        public bool CanBetFlag { get; private set; }

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public CanBetFlagProvider(IObservable<bool> observable, string name = DefaultName)
            : base(observable, name)
        {
        }

        #endregion

        #region Observer

        /// <inheritdoc/>
        public override void OnNext(bool value)
        {
            if(CanBetFlag == value)
            {
                return;
            }

            CanBetFlag = value;

            OnAsynchronousProviderChanged(new AsynchronousProviderChangedEventArgs(CanBetFlagSignature, false));
        }

        #endregion
    }
}