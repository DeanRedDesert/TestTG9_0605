// -----------------------------------------------------------------------
// <copyright file = "AscribedChooserExtensionLib.cs" company = "IGT">
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
    /// Implementation of <see cref="IAscribedChooserExtensionLib"/> backed by F2X communications.
    /// </summary>
    internal sealed class AscribedChooserExtensionLib : InnerLibBase, IAscribedChooserExtensionLib
    {
        #region Private Fields

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="AscribedChooserExtensionLib"/>.
        /// </summary>
        internal AscribedChooserExtensionLib(ITsmExtensionLink innerLink,
                                             IEventDispatcher transactionalEventDispatcher,
                                             IEventDispatcher nonTransactionalEventDispatcher)
            : base(innerLink,
                   transactionalEventDispatcher,
                   nonTransactionalEventDispatcher)
        {
            AscribedChooserContext = new AscribedChooserContext();

            // Create event table.
            CreateEventTable();
        }

        #endregion

        #region IAscribedChooserExtensionLib Implementation

        /// <inheritdoc />
        public event EventHandler<ActivateInnerContextEventArgs<IAscribedChooserContext>> ActivateContextEvent;

        /// <inheritdoc />
        public event EventHandler<InactivateInnerContextEventArgs<IAscribedChooserContext>> InactivateContextEvent;

        /// <inheritdoc />
        public IAscribedChooserContext AscribedChooserContext { get; }

        #endregion

        #region Private Methods

        #region Implementing Event Table

        /// <summary>
        /// Creates the event lookup table.
        /// </summary>
        private void CreateEventTable()
        {
            // Events should be listed in alphabetical order!

            EventTable[typeof(ActivateInnerContextEventArgs<IAscribedChooserContext>)] =
                (s, e) => HandleBeforeExecute(s, e, PreHandleActivateContext, ActivateContextEvent);

            EventTable[typeof(InactivateInnerContextEventArgs<IAscribedChooserContext>)] =
                (s, e) => HandleBeforeExecute(s, e, PreHandleInactivateContext, InactivateContextEvent);
        }

        #endregion

        #region Event Handlers

        private ActivateInnerContextEventArgs<IAscribedChooserContext> PreHandleActivateContext(EventArgs eventArgs)
        {
            IsContextActive = true;
            return new ActivateInnerContextEventArgs<IAscribedChooserContext>(AscribedChooserContext);
        }

        private InactivateInnerContextEventArgs<IAscribedChooserContext> PreHandleInactivateContext(EventArgs eventArgs)
        {
            IsContextActive = false;
            return new InactivateInnerContextEventArgs<IAscribedChooserContext>(AscribedChooserContext);
        }

        #endregion

        #endregion
    }
}