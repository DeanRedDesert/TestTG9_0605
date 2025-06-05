// -----------------------------------------------------------------------
// <copyright file = "CultureRead.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using Ascent.Communication.Platform.ExtensionLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.Restricted.EventManagement.Interfaces;
    using F2X;
    using F2XCultureContext = F2X.Schemas.Internal.CultureRead.CultureContext;

    /// <summary>
    /// Implementation of <see cref="ICultureRead"/> interface that requests
    /// the culture for a context from the Foundation.
    /// </summary>
    internal class CultureRead : ICultureRead
    {
        #region Private Fields

        private readonly ITransactionVerification transactionVerification;
        private ICultureReadCategory cultureReadCategory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="CultureRead"/> with
        /// a transactional event dispatcher.
        /// </summary>
        /// <param name="transactionVerification">
        /// The interface for verifying that a transaction is open.</param>
        /// <param name="transactionalEventDispatcher">
        /// Interface for processing a transactional event.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any argument is null.
        /// </exception>
        public CultureRead(ITransactionVerification transactionVerification,
                           IEventDispatcher transactionalEventDispatcher)
        {
            if(transactionVerification == null)
            {
                throw new ArgumentNullException("transactionVerification");
            }

            if(transactionalEventDispatcher == null)
            {
                throw new ArgumentNullException("transactionalEventDispatcher");
            }

            this.transactionVerification = transactionVerification;
            
            transactionalEventDispatcher.EventDispatchedEvent +=
                (sender, dispatchedEvent) => RaiseEvent(dispatchedEvent, CultureChangedEvent);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes class members whose values become available after construction,
        /// e.g. when a connection is established with the Foundation.
        /// </summary>
        /// <param name="cultureReadHandler">
        /// The interface for communicating with the Foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="cultureReadHandler"/> is null.
        /// </exception>
        public void Initialize(ICultureReadCategory cultureReadHandler)
        {
            if(cultureReadHandler == null)
            {
                throw new ArgumentNullException("cultureReadHandler");
            }

            cultureReadCategory = cultureReadHandler;
        }

        #endregion

        #region Implementation of ICultureRead

        /// <inheritdoc />
        public string GetCulture(CultureContext cultureContext)
        {
            CheckInitialization();

            transactionVerification.MustHaveOpenTransaction();

            return cultureReadCategory.GetCulture((F2XCultureContext)cultureContext);
        }

        /// <inheritdoc />
        public event EventHandler<CultureChangedEventArgs> CultureChangedEvent;

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks if this object has been initialized correctly before being used.
        /// </summary>
        /// <exception cref="CommunicationInterfaceUninitializedException">
        /// Thrown when any API is called before Initialize is called.
        /// </exception>
        private void CheckInitialization()
        {
            if(cultureReadCategory == null)
            {
                throw new CommunicationInterfaceUninitializedException(
                    "CultureRead cannot be used without calling its Initialize method first.");
            }
        }

        /// <summary>
        /// Raise the dispatched event using the specified event handler.
        /// </summary>
        /// <typeparam name="TEventArgs">Type of the event to raise.</typeparam>
        /// <param name="dispatchedEventArgs">The arguments used for processing the dispatched event.</param>
        /// <param name="eventHandler">The event handler used for raising the event.</param>
        private void RaiseEvent<TEventArgs>(EventDispatchedEventArgs dispatchedEventArgs,
                                            EventHandler<TEventArgs> eventHandler) where TEventArgs : EventArgs
        {
            if(dispatchedEventArgs.DispatchedEventType == typeof(TEventArgs))
            {
                if(eventHandler != null)
                {
                    eventHandler(this, dispatchedEventArgs.DispatchedEvent as TEventArgs);

                    dispatchedEventArgs.IsHandled = true;
                }
            }
        }

        #endregion
    }
}
