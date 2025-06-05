// -----------------------------------------------------------------------
// <copyright file = "DisposableCollection.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform
{
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// A wrapper class that manages a collection of disposable objects,
    /// and provides APIs to add and dispose objects.
    /// </summary>
    public class DisposableCollection : IDisposable
    {
        #region Private Fields

        /// <summary>
        /// Collection of disposable objects.
        /// </summary>
        private readonly ConcurrentBag<IDisposable> disposableObjects = new ConcurrentBag<IDisposable>();

        #endregion

        #region Public Methods

        /// <summary>
        /// Check if an object is disposable, if yes, add it to the disposable object list.
        /// </summary>
        /// <remarks>
        /// This function must be called immediately after a disposable object is instantiated.
        /// </remarks>
        /// <param name="candidate">The object to check and add.</param>
        public void Add(object candidate)
        {
            if(candidate is IDisposable disposableObject)
            {
                disposableObjects.Add(disposableObject);
            }
        }

        #endregion

        #region IDisposable Implementation

        /// <inheritdoc />
        public void Dispose()
        {
            foreach(var disposableObject in disposableObjects)
            {
                disposableObject.Dispose();
            }
        }

        #endregion
    }
}