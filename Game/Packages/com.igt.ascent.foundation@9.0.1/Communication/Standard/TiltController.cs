// -----------------------------------------------------------------------
// <copyright file = "TiltController.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.Interfaces.TiltControl;
    using Ascent.Restricted.EventManagement.Interfaces;
    using F2X;
    using F2X.Schemas.Internal.TiltControl;
    using Foundation;
    using TiltControl;
    using Tilts;

    /// <summary>
    /// A class that allows for <see cref="IActiveTilt"/> to be posted and cleared, while maintaining the Foundation
    /// driven tilt requirements.
    /// </summary>
    internal class TiltController : BaseTiltController, ITiltController
    {
        #region Private Fields

        /// <summary>
        /// The <see cref="ITiltControlCategory"/> implementation.
        /// </summary>
        private ITiltControlCategory tiltControlCategory;

        /// <summary>
        /// The <see cref="ITransactionVerification"/> implementation.
        /// </summary>
        private readonly ITransactionVerification transactionVerifier;

        /// <summary>
        /// The current locale.
        /// </summary>
        /// <remarks>
        /// US English is assumed since there is no available culture provided to the extension at this time.
        /// </remarks>
        private const string AvailableLocale = "en-US";

        #endregion Private Fields

        #region Constructors

        /// <summary>
        /// Constructs an instance of <see cref="TiltController"/>.
        /// </summary>
        /// <param name="transactionVerifier">
        /// The interface used to verify a transaction exists for the functions that require one.
        /// </param>
        /// <param name="transactionalEventDispatcher">
        /// The interface for handling incoming events.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any of the parameters is null.
        /// </exception>
        public TiltController(ITransactionVerification transactionVerifier,
                              IEventDispatcher transactionalEventDispatcher)
        {
            if(transactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(transactionalEventDispatcher));
            }

            this.transactionVerifier = transactionVerifier ?? throw new ArgumentNullException(nameof(transactionVerifier));

            transactionalEventDispatcher.EventDispatchedEvent += HandleTiltClearedByAttendantEvent;
        }

        #endregion Constructors

        #region Event Handlers

        /// <summary>
        /// Handles the tilt cleared by attendant event.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="dispatchedEventArgs">The arguments related to the tilt clear by attendant event.</param>
        protected virtual void HandleTiltClearedByAttendantEvent(object sender,
            EventDispatchedEventArgs dispatchedEventArgs)
        {
            if(dispatchedEventArgs.DispatchedEvent is TiltClearedByAttendantEventArgs eventArgs)
            {
                // Clear this tilt but do not notify the foundation.
                ClearTilt(eventArgs.TiltName, false);

                var handler = TiltClearedByAttendantEvent;
                if(handler != null)
                {
                    handler(this, eventArgs);
                    dispatchedEventArgs.IsHandled = true;
                }
            }
        }

        #endregion Event Handlers

        #region Public Methods

        /// <summary>
        /// A method that handles clearing of the tilts list when the active context gets inactivated.
        /// </summary>
        /// <remarks>
        /// The Foundation will automatically clear the tilts posted when the active context gets inactivated.
        /// </remarks>
        public void ClearTiltInfo()
        {
            TiltInfoList.Clear();
        }

        /// <summary>
        /// Initializes the <see cref="TiltController"/> instance.
        /// </summary>
        /// <param name="tiltControlCategoryProvider">
        /// The <see cref="ITiltControlCategory"/> that will be used to post/clear extension tilts with the Foundation.
        /// </param>
        public void Initialize(ITiltControlCategory tiltControlCategoryProvider)
        {
            tiltControlCategory = tiltControlCategoryProvider;
        }

        #endregion Public Methods

        #region Protected Fields

        /// <inheritdoc/>
        /// <remarks>
        /// Overrides the maximum length of the tilt after appending the key to the entity guid.
        /// </remarks>
        protected override int MaxTiltNameLength => 76; // Limited by Foundation.

        #endregion

        #region Protected Methods

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// Thrown if the tilt control category is null. The tilt control category must be initialized before use.
        /// </exception>
        protected override bool SendClearTilt(string tiltKey)
        {
            if(tiltControlCategory == null)
            {
                throw new InvalidOperationException("The tilt control category is null. The tilt control category has not " +
                                                    "been initialized.");
            }

            return tiltControlCategory.RequestClearTilt(tiltKey);
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// Thrown if the tilt control category is null. The tilt control category must be initialized before use.
        /// </exception>
        protected override bool SendRequestTilt(TiltInfo registeredTiltInfo)
        {
            if(tiltControlCategory == null)
            {
                throw new InvalidOperationException("The tilt control category is null. The tilt control category has not " +
                                                    "been initialized.");
            }

            var registeredTilt = registeredTiltInfo.Tilt;

            var unformattedMessage = registeredTilt.GetLocalizedMessage(AvailableLocale);
            var unformattedTitle = registeredTilt.GetLocalizedTitle(AvailableLocale);

            if(!string.IsNullOrEmpty(unformattedTitle) && !string.IsNullOrEmpty(unformattedMessage))
            {
                // Creates a tilt localization list.
                var tiltLocalizations = new List<TiltLocalization>
                                            {
                                                new TiltLocalization
                                                    {
                                                        Culture = AvailableLocale,
                                                        Message = string.Format(unformattedMessage,
                                                            registeredTiltInfo.MessageFormat),
                                                        Title = string.Format(unformattedTitle,
                                                            registeredTiltInfo.TitleFormat),
                                                    }
                                            };

                // Creates a tilt attribute list.
                var tiltAttributes = new List<TiltAttribute>();

                if(registeredTilt.GamePlayBehavior == TiltGamePlayBehavior.Blocking)
                {
                    tiltAttributes.Add(TiltAttribute.PreventGamePlay);
                }
                if(registeredTilt.UserInterventionRequired)
                {
                    tiltAttributes.Add(TiltAttribute.NotifyProtocols);
                }
                if(registeredTilt.ProgressiveLinkDown)
                {
                    tiltAttributes.Add(TiltAttribute.ProgressiveLinkDown);
                }
                if(registeredTilt.AttendantClear)
                {
                    tiltAttributes.Add(TiltAttribute.AttendantClear);
                }
                if(registeredTilt.DelayPreventGamePlayUntilNoGameInProgress)
                {
                    tiltAttributes.Add(TiltAttribute.DelayPreventGamePlayUntilNoGameInProgress);
                }

                return tiltControlCategory.RequestTilt(registeredTiltInfo.Key, tiltLocalizations, tiltAttributes);
            }

            return false;
        }

        #endregion Protected Methods

        #region Implementation of ITiltController

        /// <inheritdoc cref="ITiltController"/>
        public override bool PostTilt(IActiveTilt tilt, string key, IEnumerable<object> titleFormat,
                                      IEnumerable<object> messageFormat)
        {
            transactionVerifier.MustHaveOpenTransaction();
            return base.PostTilt(tilt, key, titleFormat, messageFormat);
        }

        /// <inheritdoc cref="ITiltController"/>
        public override bool ClearTilt(string key)
        {
            transactionVerifier.MustHaveOpenTransaction();
            return base.ClearTilt(key);
        }

        /// <inheritdoc />
        public event EventHandler<TiltClearedByAttendantEventArgs> TiltClearedByAttendantEvent;

        #endregion
    }
}