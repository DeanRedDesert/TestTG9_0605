// -----------------------------------------------------------------------
// <copyright file = "ObserverProviderBase.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.ServiceProviders
{
    using System;
    using Game.Core.Logic.Services;
    using ObserverPattern;

    /// <inheritdoc cref="ObserverBase{T}"/>
    /// <summary>
    /// A base class for observer providers.
    /// This type of provider can provide asynchronous game services
    /// by subscribing to an observable object.
    /// They should NOT subscribe to any Lib events!
    /// </summary>
    public abstract class ObserverProviderBase<T> : ObserverBase<T>, INotifyAsynchronousProviderChanged
    {
        #region INotifyAsynchronousProviderChanged implementation

        /// <inheritdoc/>
        public event EventHandler<AsynchronousProviderChangedEventArgs> AsynchronousProviderChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the observer.
        /// </summary>
        public string Name { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of an observer provider.
        /// </summary>
        /// <param name="observable">The item being observed.</param>
        /// <param name="name">An optional name for this provider.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="observable"/> is null.
        /// </exception>
        protected ObserverProviderBase(IObservable<T> observable, string name)
        {
            if(observable == null)
            {
                throw new ArgumentNullException(nameof(observable));
            }
            Subscribe(observable);

            Name = name ?? string.Empty;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Raises the <see cref="AsynchronousProviderChanged"/> event if there are any listeners.
        /// </summary>
        /// <param name="eventArgs">The <see cref="AsynchronousProviderChangedEventArgs"/> to use.</param>
        protected void OnAsynchronousProviderChanged(AsynchronousProviderChangedEventArgs eventArgs)
        {
            AsynchronousProviderChanged?.Invoke(this, eventArgs);
        }

        #endregion
    }
}