// -----------------------------------------------------------------------
// <copyright file = "DisplayControlStateProvider.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.ServiceProviders
{
    using System;
    using Communication.Platform.Interfaces;
    using Game.Core.Logic.Services;

    /// <summary>
    /// A service provider that observes the value of <see cref="DisplayControlState"/>,
    /// and updates the corresponding service data accordingly.
    /// </summary>
    public class DisplayControlStateProvider : ObserverProviderBase<DisplayControlState>
    {
        #region Constants

        /// <summary>
        /// The default name of the provider.
        /// </summary>
        private const string DefaultName = nameof(DisplayControlStateProvider);

        #endregion

        #region Private Fields

        private static readonly ServiceSignature ServiceSignature = new ServiceSignature(nameof(DisplayControlState));

        #endregion

        #region Game Services

        /// <summary>
        /// Gets the current value of Display Control State.
        /// </summary>
        [AsynchronousGameService]
        public DisplayControlState DisplayControlState { get; private set; }

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public DisplayControlStateProvider(IObservable<DisplayControlState> observable, string name = DefaultName)
            : base(observable, name)
        {
        }

        #endregion

        #region ObserverBase Overrides

        /// <inheritdoc/>
        public override void OnNext(DisplayControlState value)
        {
            if(DisplayControlState == value)
            {
                return;
            }

            DisplayControlState = value;

            OnAsynchronousProviderChanged(new AsynchronousProviderChangedEventArgs(ServiceSignature, false));
        }

        #endregion
    }
}