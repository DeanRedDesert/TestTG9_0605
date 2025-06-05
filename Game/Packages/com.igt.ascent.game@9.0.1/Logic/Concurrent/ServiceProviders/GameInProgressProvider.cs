// -----------------------------------------------------------------------
// <copyright file = "GameInProgressProvider.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.ServiceProviders
{
    using System;
    using Game.Core.Logic.Services;

    /// <summary>
    /// A provider for the game in progress flag.
    /// </summary>
    public class GameInProgressProvider: ObserverProviderBase<bool>
    {
        #region Constants

        private const string DefaultName = nameof(GameInProgressProvider);
        private static readonly ServiceSignature GameInProgressSignature = new ServiceSignature(nameof(GameInProgress));

        #endregion

        #region Game Services

        /// <summary>
        /// Gets the flag indicating whether an active coplayer game is in progress.
        /// </summary>
        [AsynchronousGameService]
        public bool GameInProgress { get; private set; }

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public GameInProgressProvider(IObservable<bool> observable, string name = DefaultName)
            : base(observable, name)
        {
        }

        #endregion

        #region Observer

        /// <inheritdoc/>
        public override void OnNext(bool value)
        {
            if(GameInProgress == value)
            {
                return;
            }

            GameInProgress = value;

            OnAsynchronousProviderChanged(new AsynchronousProviderChangedEventArgs(GameInProgressSignature, false));
        }

        #endregion
    }
}