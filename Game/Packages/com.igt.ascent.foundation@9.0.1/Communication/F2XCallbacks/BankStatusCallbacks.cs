//-----------------------------------------------------------------------
// <copyright file = "BankStatusCallbacks.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using Ascent.Communication.Platform.Interfaces;
    using F2X;
    using F2XTransport;
    using F2XType = F2X.Schemas.Internal.BankStatus;

    /// <summary>
    /// This class is responsible for handling callbacks from the <see cref="BankStatusCategory"/>.
    /// </summary>
    internal class BankStatusCallbacks : IBankStatusCategoryCallbacks
    {
        #region Private Fields

        /// <summary>
        /// The callback interface for handling transactional events.
        /// </summary>
        private readonly IEventCallbacks eventCallbacksInterface;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an instance of the <see cref="BankStatusCallbacks"/>.
        /// </summary>
        /// <param name="eventCallbacksInterface">
        /// The callback interface for handling transactional events.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacksInterface"/> is null.
        /// </exception>
        public BankStatusCallbacks(IEventCallbacks eventCallbacksInterface)
        {
            if(eventCallbacksInterface == null)
            {
                throw new ArgumentNullException("eventCallbacksInterface");
            }

            this.eventCallbacksInterface = eventCallbacksInterface;
        }

        #endregion

        #region IBankStatusCategoryCallbacks

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="bankEvent"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="bankEvent.Item"/> is not of one of the known types (CreditMovementEventData, CashoutStartEventData, CashoutEndEventData).
        /// Thrown when <paramref name="bankEvent.Item"/> or <paramref name="bankEvent.Item.CurrentPlayerMeters"/> is null.
        /// </exception>
        public string ProcessBankEvent(F2XType.BankEventData bankEvent)
        {
            if(bankEvent == null)
            {
                throw new ArgumentNullException(nameof(bankEvent));
            }

            if(bankEvent.Item == null)
            {
                throw new ArgumentException("bankEvent.Item must not be null.");
            }

            if(bankEvent.Item.CurrentPlayerMeters == null)
            {
                throw new ArgumentException("bankEvent.Item.CurrentPlayerMeters must not be null.");
            }

            var meters = bankEvent.Item.CurrentPlayerMeters.ToPublic();

            switch(bankEvent.Item)
            {
                case F2XType.CreditMovementEventData f2XEventData:
                {
                    var bankMeterEventArgs = f2XEventData.MovementTypeSpecified
                        ? new BankMeterEventArgs(meters, BankMeterEventType.CreditMovement,
                            (CreditMovementType)f2XEventData.MovementType)
                        : new BankMeterEventArgs(meters, BankMeterEventType.CreditMovement);

                    eventCallbacksInterface.PostEvent(bankMeterEventArgs);
                    break;
                }
                case F2XType.CashoutStartEventData _:
                    eventCallbacksInterface.PostEvent(new BankMeterEventArgs(meters, BankMeterEventType.CashoutStart));
                    break;
                case F2XType.CashoutEndEventData _:
                    eventCallbacksInterface.PostEvent(new BankMeterEventArgs(meters, BankMeterEventType.CashoutEnd));
                    break;
                default:
                    throw new ArgumentException("Unknown Bank status event type: " + bankEvent.Item.GetType());
            }

            return null;
        }

        #endregion
    }
}