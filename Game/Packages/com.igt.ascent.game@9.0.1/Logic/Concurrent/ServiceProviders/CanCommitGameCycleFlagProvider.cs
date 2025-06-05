// -----------------------------------------------------------------------
// <copyright file = "CanCommitGameCycleFlagProvider.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.ServiceProviders
{
    using System;
    using Game.Core.Logic.Services;

    /// <summary>
    /// A provider for the current Can Commit Game Cycle Flag of the coplayer.
    /// </summary>
    public sealed class CanCommitGameCycleFlagProvider : ObserverProviderBase<bool>
    {
        #region Constants

        private const string DefaultName = nameof(CanCommitGameCycleFlagProvider);
        private static readonly ServiceSignature CanCommitGameCycleFlagSignature = new ServiceSignature(nameof(CanCommitGameCycleFlag));

        #endregion

        #region Game Services

        /// <summary>
        /// Gets the current Can Bet flag of the coplayer.
        /// </summary>
        [AsynchronousGameService]
        public bool CanCommitGameCycleFlag { get; private set; }

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public CanCommitGameCycleFlagProvider(IObservable<bool> observable, string name = DefaultName)
            : base(observable, name)
        {
        }

        #endregion

        #region Observer

        /// <inheritdoc/>
        public override void OnNext(bool value)
        {
            if(CanCommitGameCycleFlag == value)
            {
                return;
            }

            CanCommitGameCycleFlag = value;

            OnAsynchronousProviderChanged(new AsynchronousProviderChangedEventArgs(CanCommitGameCycleFlagSignature, false));
        }

        #endregion
    }
}