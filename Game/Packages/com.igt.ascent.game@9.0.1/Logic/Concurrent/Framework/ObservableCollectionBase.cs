// -----------------------------------------------------------------------
// <copyright file = "ObservableCollectionBase.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    /// <summary>
    /// A base implementation of a collection of observables.
    /// </summary>
    internal abstract class ObservableCollectionBase
    {
        #region Public Methods

        /// <summary>
        /// Dismisses all observables as well as performs other clean up jobs, such as unregister event handlers etc.
        /// </summary>
        public abstract void Dismiss();

        #endregion
    }
}