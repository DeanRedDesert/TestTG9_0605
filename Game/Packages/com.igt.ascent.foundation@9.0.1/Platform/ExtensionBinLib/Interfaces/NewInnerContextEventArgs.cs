// -----------------------------------------------------------------------
// <copyright file = "NewInnerContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib.Interfaces
{
    using System;
    using Platform.Interfaces;

    /// <summary>
    /// The event indicating that a new inner context is about to be activated.
    /// </summary>
    /// <typeparam name="TContext">Type of the inner context.</typeparam>
    [Serializable]
    public sealed class NewInnerContextEventArgs<TContext> : TransactionalEventArgs where TContext : IInnerContext
    {
        #region Properties

        /// <summary>
        /// Gets the information on the context that is about to be activated.
        /// </summary>
        public TContext Context { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="NewInnerContextEventArgs{TContext}"/>.
        /// </summary>
        /// <param name="context">
        /// The information on the context that is about to be activated.
        /// </param>
        public NewInnerContextEventArgs(TContext context = default)
        {
            Context = context;
        }

        #endregion
    }
}