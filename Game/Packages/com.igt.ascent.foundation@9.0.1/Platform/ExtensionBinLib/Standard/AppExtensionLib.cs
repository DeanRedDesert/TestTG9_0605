// -----------------------------------------------------------------------
// <copyright file = "AppExtensionLib.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib.Standard
{
    using System;
    using F2XLinks;
    using Interfaces;
    using Platform.Interfaces;
    using Platform.Standard;
    using Restricted.EventManagement.Interfaces;

    /// <summary>
    /// Implementation of <see cref="IAppExtensionLib"/> backed by F2X communications.
    /// </summary>
    internal sealed class AppExtensionLib : InnerLibBase, IAppExtensionLib
    {
        #region Private Fields

        private readonly IAppExtensionLink appExtensionLink;

        private readonly ChooserServices<IAppExtensionContext> chooserServices;

        /// <summary>
        /// The flag indicating whether the internal interface modules have been initialized.
        /// </summary>
        private bool isInitialized;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="AppExtensionLib"/>.
        /// </summary>
        internal AppExtensionLib(IAppExtensionLink innerLink,
                                 IEventDispatcher transactionalEventDispatcher,
                                 IEventDispatcher nonTransactionalEventDispatcher)
            : base(innerLink,
                   transactionalEventDispatcher,
                   nonTransactionalEventDispatcher)
        {
            Context = new AppExtensionContext();

            appExtensionLink = innerLink;

            // Create interface modules.
            chooserServices = new ChooserServices<IAppExtensionContext>(this, transactionalEventDispatcher);

            // Create event table.
            CreateEventTable();

            // Subscribe event handlers.
            DisplayControlEvent += HandleDisplayControl;
        }

        #endregion

        #region IAppExtensionLib Implementation

        /// <inheritdoc />
        public event EventHandler<ActivateInnerContextEventArgs<IAppExtensionContext>> ActivateContextEvent;

        /// <inheritdoc />
        public event EventHandler<InactivateInnerContextEventArgs<IAppExtensionContext>> InactivateContextEvent;

        /// <inheritdoc />
        public event EventHandler<DisplayControlEventArgs> DisplayControlEvent;

        /// <inheritdoc />
        public IAppExtensionContext Context { get; private set; }

        /// <inheritdoc />
        public DisplayControlState DisplayControlState { get; private set; }

        /// <inheritdoc />
        public IChooserServices ChooserServices => InService() ? chooserServices : null;

        #endregion

        #region Private Methods

        #region Implementing Event Table

        /// <summary>
        /// Creates the event lookup table.
        /// </summary>
        private void CreateEventTable()
        {
            // Events should be listed in alphabetical order!

            EventTable[typeof(ActivateInnerContextEventArgs<IAppExtensionContext>)] =
                (s, e) => HandleBeforeExecute(s, e, PreHandleActivateContext, ActivateContextEvent);

            EventTable[typeof(DisplayControlEventArgs)] =
                (s, e) => ExecuteEventHandler(s, e, DisplayControlEvent);

            EventTable[typeof(InactivateInnerContextEventArgs<IAppExtensionContext>)] =
                (s, e) => HandleBeforeExecute(s, e, PreHandleInactivateContext, InactivateContextEvent);
        }

        #endregion

        #region Event Handlers

        private ActivateInnerContextEventArgs<IAppExtensionContext> PreHandleActivateContext(EventArgs eventArgs)
        {
            InitializeOnce();

            Context = new AppExtensionContext(appExtensionLink.ActiveAppExtension);

            IsContextActive = true;

            return new ActivateInnerContextEventArgs<IAppExtensionContext>(Context);
        }

        private InactivateInnerContextEventArgs<IAppExtensionContext> PreHandleInactivateContext(EventArgs eventArgs)
        {
            var lastActiveContext = (AppExtensionContext)Context;
            Context = new AppExtensionContext();

            IsContextActive = false;

            return new InactivateInnerContextEventArgs<IAppExtensionContext>(lastActiveContext);
        }

        private void HandleDisplayControl(object sender, DisplayControlEventArgs eventArgs)
        {
            DisplayControlState = eventArgs.DisplayControlState;
        }

        #endregion

        #region Initialization and Status Check

        /// <summary>
        /// One-time initialization of the internal interface modules.
        /// </summary>
        /// <devdoc>
        /// This is supposed to be called once after the first round of API negotiation,
        /// assuming that the availability of a category will not change across rounds of negotiation.
        ///
        /// This should be called at the first chance the lib is able do it, maybe upon
        /// NewContext or ActivateContext events.
        ///
        /// We don't want to do it in Link's ProcessAppApiNegotiation method, as it is called on a pool thread,
        /// which will force us to implement thread safety in all the modules.
        ///
        /// New/ActivateContext events are  always called after (not during) the negotiation.  So we should be okay
        /// as long as the initialization only involves getting references from the link.
        /// </devdoc>
        private void InitializeOnce()
        {
            if(isInitialized)
            {
                return;
            }

            isInitialized = true;

            chooserServices.Initialize(appExtensionLink.ChooserServicesCategory);
        }

        /// <summary>
        /// Checks whether an interface implementation can be provided to users.
        /// </summary>
        /// <returns>
        /// The flag indicating the check result.
        /// </returns>
        private bool InService()
        {
            return IsConnected && isInitialized;
        }

        #endregion

        #endregion
    }
}