// -----------------------------------------------------------------------
// <copyright file = "Observable.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.ObserverPattern
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// This class implements a simple observable that provides push-based notifications.
    /// </summary>
    /// <inheritdoc/>
    public class Observable<T> : IObservable<T>
    {
        #region Nested Class

        private class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<T>> observers;
            private readonly IObserver<T> observer;

            public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
            {
                this.observers = observers;
                this.observer = observer;
            }

            public void Dispose()
            {
                if(observer != null && observers.Contains(observer))
                {
                    observers.Remove(observer);
                }
            }
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// The list of observers.
        /// </summary>
        protected readonly List<IObserver<T>> Observers = new List<IObserver<T>>();

        /// <summary>
        /// Current value of the observable.
        /// </summary>
        protected T CurrentDataValue;

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets a new value of the observable.
        /// </summary>
        /// <param name="value">The new value of the observable.</param>
        public virtual void SetValue(T value)
        {
            CurrentDataValue = value;

            foreach(var observer in Observers)
            {
                observer.OnNext(value);
            }
        }

        /// <summary>
        /// Dismisses the observable object.  There will be no more update for this object.
        /// </summary>
        public virtual void Dismiss()
        {
            var collectionCopy = Observers.ToList();

            foreach(var observer in collectionCopy)
            {
                observer.OnCompleted();
            }

            Observers.Clear();
        }

        #endregion

        #region Interface Implementation

        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            if(!Observers.Contains(observer))
            {
                Observers.Add(observer);

                // Initialize the observer's value to the current one.
                observer.OnNext(CurrentDataValue);
            }

            return new Unsubscriber(Observers, observer);
        }

        #endregion
    }
}