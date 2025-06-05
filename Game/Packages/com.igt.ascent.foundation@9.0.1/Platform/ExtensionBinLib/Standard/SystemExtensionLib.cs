// -----------------------------------------------------------------------
// <copyright file = "SystemExtensionLib.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib.Standard
{
    using System;
    using F2XLinks;
    using Interfaces;
    using Restricted.EventManagement.Interfaces;

    /// <summary>
    /// Implementation of <see cref="ISystemExtensionLib"/> backed by F2X communications.
    /// </summary>
    internal sealed class SystemExtensionLib : InnerLibBase, ISystemExtensionLib
    {
        #region Private Fields

        private readonly ISystemExtensionLink systemExtensionLink;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="SystemExtensionLib"/>.
        /// </summary>
        internal SystemExtensionLib(ISystemExtensionLink innerLink,
                                    IEventDispatcher transactionalEventDispatcher,
                                    IEventDispatcher nonTransactionalEventDispatcher)
            : base(innerLink,
                   transactionalEventDispatcher,
                   nonTransactionalEventDispatcher)
        {
            Context = new SystemExtensionContext();

            systemExtensionLink = innerLink;

            // Create event table.
            CreateEventTable();
        }

        #endregion

        #region ISystemExtensionLib Implementation

        /// <inheritdoc />
        public event EventHandler<ActivateInnerContextEventArgs<ISystemExtensionContext>> ActivateContextEvent;

        /// <inheritdoc />
        public event EventHandler<InactivateInnerContextEventArgs<ISystemExtensionContext>> InactivateContextEvent;

        /// <inheritdoc />
        public ISystemExtensionContext Context { get; private set; }

        #endregion

        #region Private Methods

        #region Implementing Event Table

        /// <summary>
        /// Creates the event lookup table.
        /// </summary>
        private void CreateEventTable()
        {
            // Events should be listed in alphabetical order!

            EventTable[typeof(ActivateInnerContextEventArgs<ISystemExtensionContext>)] =
                (s, e) => HandleBeforeExecute(s, e, PreHandleActivateContext, ActivateContextEvent);

            EventTable[typeof(InactivateInnerContextEventArgs<ISystemExtensionContext>)] =
                (s, e) => HandleBeforeExecute(s, e, PreHandleInactivateContext, InactivateContextEvent);
        }

        #endregion

        #region Event Handlers

        private ActivateInnerContextEventArgs<ISystemExtensionContext> PreHandleActivateContext(EventArgs eventArgs)
        {
            Context = new SystemExtensionContext(systemExtensionLink.SupportedExtensions);

            IsContextActive = true;

            return new ActivateInnerContextEventArgs<ISystemExtensionContext>(Context);
        }

        private InactivateInnerContextEventArgs<ISystemExtensionContext> PreHandleInactivateContext(EventArgs eventArgs)
        {
            var lastActiveContext = (SystemExtensionContext)Context;
            Context = new SystemExtensionContext();

            IsContextActive = false;

            return new InactivateInnerContextEventArgs<ISystemExtensionContext>(lastActiveContext);
        }

        #endregion

        #endregion
    }
}