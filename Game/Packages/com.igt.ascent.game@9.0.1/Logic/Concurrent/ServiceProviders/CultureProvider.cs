// -----------------------------------------------------------------------
// <copyright file = "CultureProvider.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.ServiceProviders
{
    using System;
    using Game.Core.Logic.Services;

    /// <summary>
    /// A provider for the current culture of the application.
    /// </summary>
    public class CultureProvider : ObserverProviderBase<string>
    {
        #region Constants

        private const string DefaultName = nameof(CultureProvider);
        private static readonly ServiceSignature CultureSignature = new ServiceSignature(nameof(Culture));

        #endregion

        #region Game Services

        /// <summary>
        /// Gets the current culture of the application.
        /// </summary>
        [AsynchronousGameService]
        public string Culture { get; private set; }

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public CultureProvider(IObservable<string> observable, string name = DefaultName)
            : base(observable, name)
        {
        }

        #endregion

        #region Observer

        /// <inheritdoc/>
        public override void OnNext(string value)
        {
            if(Culture == value)
            {
                return;
            }

            Culture = value;

            OnAsynchronousProviderChanged(new AsynchronousProviderChangedEventArgs(CultureSignature, false));
        }

        #endregion
    }
}