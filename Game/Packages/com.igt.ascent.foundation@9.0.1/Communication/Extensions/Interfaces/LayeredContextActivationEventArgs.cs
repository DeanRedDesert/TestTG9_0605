// -----------------------------------------------------------------------
// <copyright file = "LayeredContextActivationEventArgs.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    using System;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Event that occurs when a context at a specific layer is
    /// activated and/or inactivated.
    /// </summary>
    [Serializable]
    public sealed class LayeredContextActivationEventArgs : TransactionalEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the layer whose context is being activated or inactivated.
        /// </summary>
        public ContextLayer ContextLayer { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="LayeredContextActivationEventArgs"/>.
        /// </summary>
        /// <param name="contextLayer"></param>
        public LayeredContextActivationEventArgs(ContextLayer contextLayer)
        {
            ContextLayer = contextLayer;
        }

        #endregion
    }
}