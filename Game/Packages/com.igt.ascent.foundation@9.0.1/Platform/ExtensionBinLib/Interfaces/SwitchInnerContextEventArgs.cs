// -----------------------------------------------------------------------
// <copyright file = "SwitchInnerContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib.Interfaces
{
    using System;
    using Platform.Interfaces;

    /// <summary>
    /// The event indicating that the inner context changes without being inactivated first.
    /// </summary>
    /// <typeparam name="TContext">Type of the inner context.</typeparam>
    /// <devdoc>
    /// So far this event is only needed/used by <see cref="IAscribedGameExtensionLib"/>.
    /// </devdoc>
    [Serializable]
    public sealed class SwitchInnerContextEventArgs<TContext> : TransactionalEventArgs where TContext : IInnerContext
    {
        #region Properties

        /// <summary>
        /// Gets the information on the context being switched to.
        /// </summary>
        public TContext NewContext { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="SwitchInnerContextEventArgs{TContext}"/>.
        /// </summary>
        /// <param name="newContext">
        /// The information on the context being switched to.
        /// </param>
        public SwitchInnerContextEventArgs(TContext newContext = default)
        {
            NewContext = newContext;
        }

        #endregion
    }
}