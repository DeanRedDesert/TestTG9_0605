// -----------------------------------------------------------------------
// <copyright file = "ObserverBase.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.ObserverPattern
{
    using System;

    /// <inheritdoc/>
    /// <summary>
    /// Base class for implementing an observer that receives push-based notifications.
    /// </summary>
    public abstract class ObserverBase<T> : IObserver<T>
    {
        #region Private Fields

        private IDisposable unsubscriber;

        #endregion

        #region Public Methods

        /// <summary>
        /// Subscribes to a provider.
        /// </summary>
        /// <param name="provider">
        /// The provider to provide the notification information.
        /// </param>
        public void Subscribe(IObservable<T> provider)
        {
            if(provider != null)
            {
                unsubscriber = provider.Subscribe(this);
            }
        }

        /// <summary>
        /// Un-subscribes from the provider.
        /// </summary>
        public void Unsubscribe()
        {
            unsubscriber?.Dispose();
        }

        #endregion

        #region IObserver Implementation

        /// <inheritdoc/>
        public abstract void OnNext(T value);

        /// <inheritdoc/>
        public virtual void OnCompleted()
        {
            Unsubscribe();
        }

        /// <inheritdoc/>
        public virtual void OnError(Exception error)
        {
            // No implementation.
        }

        #endregion
    }
}