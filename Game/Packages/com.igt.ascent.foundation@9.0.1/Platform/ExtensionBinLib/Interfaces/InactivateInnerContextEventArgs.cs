// -----------------------------------------------------------------------
// <copyright file = "InactivateInnerContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib.Interfaces
{
    using System;
    using Platform.Interfaces;

    /// <summary>
    /// The event indicating that the inner context is being inactivated.
    /// </summary>
    /// <typeparam name="TContext">Type of the inner context.</typeparam>
    [Serializable]
    public sealed class InactivateInnerContextEventArgs<TContext> : TransactionalEventArgs where TContext : IInnerContext
    {
        #region Properties

        /// <summary>
        /// Gets the information on the context being inactivated.
        /// </summary>
        public TContext LastActiveContext { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="InactivateInnerContextEventArgs{TContext}"/>.
        /// </summary>
        /// <param name="lastActiveContext">
        /// The information on the context being inactivated.
        /// </param>
        public InactivateInnerContextEventArgs(TContext lastActiveContext = default)
        {
            LastActiveContext = lastActiveContext;
        }

        #endregion
    }
}