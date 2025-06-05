// -----------------------------------------------------------------------
// <copyright file = "ActivateInnerContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib.Interfaces
{
    using System;
    using Platform.Interfaces;

    /// <summary>
    /// The event indicating that the inner context is being activated.
    /// </summary>
    /// <typeparam name="TContext">Type of the inner context.</typeparam>
    [Serializable]
    public sealed class ActivateInnerContextEventArgs<TContext> : TransactionalEventArgs where TContext : IInnerContext
    {
        #region Properties

        /// <summary>
        /// Gets the information on the context being activated.
        /// </summary>
        public TContext Context { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ActivateInnerContextEventArgs{TContext}"/>.
        /// </summary>
        /// <param name="context">
        /// The information on the context being activated.
        /// </param>
        public ActivateInnerContextEventArgs(TContext context = default)
        {
            Context = context;
        }

        #endregion
    }
}