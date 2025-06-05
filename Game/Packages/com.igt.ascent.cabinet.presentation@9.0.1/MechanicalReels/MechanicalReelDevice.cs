//-----------------------------------------------------------------------
// <copyright file = "MechanicalReelDevice.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.MechanicalReels
{
    using System;
    using System.Linq;
    using Communication.Cabinet.CSI.Schemas;
    using Communication.Cabinet.MechanicalReels;
    using Communication.Cabinet;

    /// <summary>
    /// Abstract Base class that all specific reel device implementations inherit from.
    /// </summary>
    public abstract class MechanicalReelDevice
    {
        #region Private and Protected Data

        /// <summary>
        /// The <see cref="ReelFeatureDescription"/> of this device instance.
        /// </summary>
        private ReelFeatureDescription description;

        /// <summary>
        /// The current <see cref="ReelsSpunState"/>
        /// </summary>
        private volatile ReelsSpunState currentDeviceState = ReelsSpunState.AllStopped;

        /// <summary>
        ///  Gets/private sets the current device state.
        /// </summary>
        protected ReelsSpunState CurrentDeviceState
        {
            get => currentDeviceState;
            private set => currentDeviceState = value;
        }

        // Device level events.
        private event EventHandler<EventArgs> OnReelDeviceSpinStarted;
        private event EventHandler<EventArgs> OnReelDeviceAtConstantSpeed;
        private event EventHandler<EventArgs> OnReelDeviceSpinningDown;
        private event EventHandler<EventArgs> OnReelDeviceSpinComplete;
        private event EventHandler<EventArgs> OnReelDeviceMovingIrregularly;

        // Individual reel events
        private event EventHandler<ReelEventArgs> OnReelSpinStarted;
        private event EventHandler<ReelEventArgs> OnReelSpinAtConstantSpeed;
        private event EventHandler<ReelEventArgs> OnReelSpinDecelerating;
        private event EventHandler<ReelEventArgs> OnReelSpinComplete;
        private event EventHandler<ReelEventArgs> OnReelSpinIrregular;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets/sets a flag indicating whether the device has been acquired successfully.
        /// </summary>
        public bool DeviceAcquired
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets/sets a flag indicating whether the device has been initialized.
        /// </summary>
        public bool DeviceInitialized
        {
            get;
            private set;
        }

        /// <summary>
        /// The description of the reel device feature.
        /// </summary>
        public ReelFeatureDescription Description
        {
            get => description;
            protected set
            {
                description = value;
                ReelCount = description?.ReelDescriptions.Count() ?? 0;
            }
        }

        /// <summary>
        /// The reason an acquisition has failed. Null on successful acquisition.
        /// </summary>
        public DeviceAcquisitionFailureReason? AcquireFailureReason
        {
            get;
            private set;
        }

        /// <summary>
        /// The number of reels on the reel device.
        /// </summary>
        public int ReelCount { get; private set; }

        #endregion

        #region Public Events

        #region Reel Device Events

        /// <summary>
        /// Event posted when all reels have begun to spin.
        /// </summary>
        public event EventHandler<EventArgs> ReelDeviceSpinStartedEvent
        {
            add
            {
                if(!IsEventHandlerAlreadySubscribed(value, OnReelDeviceSpinStarted))
                {
                    OnReelDeviceSpinStarted += value;
                }
            }
            remove => OnReelDeviceSpinStarted -= value;
        }

        /// <summary>
        /// Event posted when all reels have reached constant speed.
        /// </summary>
        public event EventHandler<EventArgs> ReelDeviceAtConstantSpeedEvent
        {
            add
            {
                if(!IsEventHandlerAlreadySubscribed(value, OnReelDeviceAtConstantSpeed))
                {
                    OnReelDeviceAtConstantSpeed += value;
                }
            }
            remove => OnReelDeviceAtConstantSpeed -= value;
        }

        /// <summary>
        /// Event posted when all reels have started spinning down;
        /// </summary>
        public event EventHandler<EventArgs> ReelDeviceSpinningDownEvent
        {
            add
            {
                if(!IsEventHandlerAlreadySubscribed(value, OnReelDeviceSpinningDown))
                {
                    OnReelDeviceSpinningDown += value;
                }
            }
            remove => OnReelDeviceSpinningDown -= value;
        }

        /// <summary>
        /// Event posted when all reels have stopped spinning.
        /// </summary>
        public event EventHandler<EventArgs> ReelDeviceSpinCompleteEvent
        {
            add
            {
                if(!IsEventHandlerAlreadySubscribed(value, OnReelDeviceSpinComplete))
                {
                    OnReelDeviceSpinComplete += value;
                }
            }
            remove => OnReelDeviceSpinComplete -= value;
        }

        /// <summary>
        /// Event posted when all reels have started movving irregularily.
        /// </summary>
        public event EventHandler<EventArgs> ReelDeviceMovingIrregularlyEvent
        {
            add
            {
                if(!IsEventHandlerAlreadySubscribed(value, OnReelDeviceMovingIrregularly))
                {
                    OnReelDeviceMovingIrregularly += value;
                }
            }
            remove => OnReelDeviceMovingIrregularly -= value;
        }



        #endregion

        #region Individual Reel Events

        /// <summary>
        /// Event posted for each reel as it starts to spin.
        /// </summary>
        public event EventHandler<ReelEventArgs> ReelSpinStartedEvent
        {
            add
            {
                if(!IsEventHandlerAlreadySubscribed(value, OnReelSpinStarted))
                {
                    OnReelSpinStarted += value;
                }
            }
            remove => OnReelSpinStarted -= value;
        }

        /// <summary>
        /// Event posted for each reel as it stops.
        /// </summary>
        public event EventHandler<ReelEventArgs> ReelSpinCompleteEvent
        {
            add
            {
                if(!IsEventHandlerAlreadySubscribed(value, OnReelSpinComplete))
                {
                    OnReelSpinComplete += value;
                }
            }
            remove => OnReelSpinComplete -= value;
        }

        /// <summary>
        /// Event posted for each reel when it has reached its constant spin speed.
        /// </summary>
        public event EventHandler<ReelEventArgs> ReelSpinAtConstantSpeedEvent
        {
            add
            {
                if(!IsEventHandlerAlreadySubscribed(value, OnReelSpinAtConstantSpeed))
                {
                    OnReelSpinAtConstantSpeed += value;
                }
            }
            remove => OnReelSpinAtConstantSpeed -= value;
        }

        /// <summary>
        /// Event posted for each reel as it begins to decelerate.
        /// </summary>
        public event EventHandler<ReelEventArgs> ReelSpinDeceleratingEvent
        {
            add
            {
                if(!IsEventHandlerAlreadySubscribed(value, OnReelSpinDecelerating))
                {
                    OnReelSpinDecelerating += value;
                }
            }
            remove => OnReelSpinDecelerating -= value;
        }

        /// <summary>
        /// Event posted for each reel as it begins to decelerate.
        /// </summary>
        public event EventHandler<ReelEventArgs> ReelSpinIrregularEvent
        {
            add
            {
                if(!IsEventHandlerAlreadySubscribed(value, OnReelSpinIrregular))
                {
                    OnReelSpinIrregular += value;
                }
            }
            remove => OnReelSpinIrregular -= value;
        }

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor taking a <see cref="ReelFeatureDescription"/> argument.
        /// </summary>
        /// <param name="description">The <see cref="ReelFeatureDescription"/> of this reel device.</param>
        /// <exception cref="ArgumentNullException">Thrown if description is null.</exception>
        protected MechanicalReelDevice(ReelFeatureDescription description)
        {
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Notify the CSI manager that this device is required. Required devices will tilt the game when they aren't available.
        /// </summary>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        public ReelCommandResult RequireDevice()
        {
            var result = MechanicalReels.RequireDevice(Description.FeatureId);
            return result;
        }

        /// <summary>
        /// Clears internal event handlers, should be called along with the 'ClearStatusRegistration'
        /// message sent to the CSI.
        /// </summary>
        public void ClearAllEventHandlers()
        {
            OnReelSpinStarted = null;
            OnReelSpinAtConstantSpeed = null;
            OnReelSpinComplete = null;
            OnReelSpinIrregular = null;
            OnReelSpinDecelerating = null;

            OnReelDeviceSpinStarted = null;
            OnReelDeviceAtConstantSpeed = null;
            OnReelDeviceSpinningDown = null;
            OnReelDeviceSpinComplete = null;
            OnReelDeviceMovingIrregularly = null;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Initializer for the MechanicalReelDevice.
        /// </summary>
        /// <param name="mechanicalReels">Handle to the MechanicalReels cabinet interface.</param>
        /// <exception cref="ArgumentNullException">Thrown if mechanicalReels is null.</exception>
        internal virtual void Initialize(IMechanicalReels mechanicalReels)
        {
            MechanicalReels = mechanicalReels ?? throw new ArgumentNullException(nameof(mechanicalReels));
            MechanicalReels.ReelStatusChangedEvent += OnReelStatusChanged;
            MechanicalReels.ReelsSpunStateChangedEvent += OnReelsSpunStateChanged;
            DeviceInitialized = true;
        }

        /// <summary>
        /// Deinitialize the device object.
        /// </summary>
        internal virtual void Deinitialize()
        {
            DeviceInitialized = false;
            if(MechanicalReels != null)
            {
                MechanicalReels.ReelStatusChangedEvent -= OnReelStatusChanged;
                MechanicalReels.ReelsSpunStateChangedEvent -= OnReelsSpunStateChanged;
            }

            ClearAllEventHandlers();
        }

        /// <summary>
        /// Set the acquired state.
        /// </summary>
        internal virtual void SetAcquired(AcquireDeviceResult acquireDeviceResult)
        {
            DeviceAcquired = acquireDeviceResult.Acquired;
            AcquireFailureReason = acquireDeviceResult.Reason;
            if(MechanicalReels != null && Description != null)
            {
                AcquireFailureReason = MechanicalReels.SetOnlineStatus(Description.FeatureId, DeviceAcquired) == ReelCommandResult.CommandIgnoredAsReelDeviceIsTilted?
                    DeviceAcquisitionFailureReason.DeviceTilted:
                        AcquireFailureReason;
            }
        }
        #endregion

        #region Protected Properties

        /// <summary>
        /// Handle to the MechanicalReels cabinet interface.
        /// </summary>
        protected IMechanicalReels MechanicalReels;

        #endregion

        #region Private Methods

        /// <summary>
        /// Handler for the ReelStatusChanged event.
        /// </summary>
        /// <param name="sender">The reel controller.</param>
        /// <param name="args">Description of the status event of type <see cref="ReelStatusEventArgs"/>.</param>
        private void OnReelStatusChanged(object sender, ReelStatusEventArgs args)
        {
            switch(args.Status)
            {
                case ReelStatus.Accelerating:
                    PostReelSpinStarted(args.ReelNumber);
                    break;

                case ReelStatus.Stopped:
                    PostReelSpinComplete(args.ReelNumber);
                    break;

                case ReelStatus.ConstantSpeed:
                    PostReelSpinAtConstantSpeed(args.ReelNumber);
                    break;

                case ReelStatus.Decelerating:
                    PostReelSpinDecelerating(args.ReelNumber);
                    break;

                case ReelStatus.MovingIrregularly:
                    PostReelMovingIrregularly(args.ReelNumber);
                    break;
            }
        }

        /// <summary>
        /// Handler for the ReelsSpunStateChanged event.
        /// </summary>
        /// <param name="sender">The reel controller.</param>
        /// <param name="args">Parameters of type <see cref="ReelsSpunEventArgs"/>.</param>
        private void OnReelsSpunStateChanged(object sender, ReelsSpunEventArgs args)
        {
            switch(args.ReelsSpunState)
            {
                case ReelsSpunState.AllSpinningUp:
                    PostReelShelfSpinStarted();
                    break;

                case ReelsSpunState.AllCompletedSpinUp:
                    PostReelShelfAtConstantSpeed();
                    break;

                case ReelsSpunState.AllSpinningDown:
                    PostReelShelfSpinningDown();
                    break;

                case ReelsSpunState.AllStopped:
                    PostReelShelfSpinComplete();
                    break;

                case ReelsSpunState.AllMovingIrregularly:
                    PostReelShelfMovingIrreg();
                    break;
            }
        }

        /// <summary>
        /// Checks if an event handler delegate is already in an event handler's list .
        /// </summary>
        /// <param name="prospectiveHandler">The <see cref="Delegate"/> that is going to be added.</param>
        /// <param name="handler">The <see cref="EventHandler{T}"/> where T is based on <see cref="EventArgs"/></param>.
        /// <returns>Flag indicating if this event is already handled by the prospective handler.</returns>
        private bool IsEventHandlerAlreadySubscribed<TEventArgs>(Delegate prospectiveHandler, EventHandler<TEventArgs> handler) where TEventArgs : EventArgs
        {
            if(handler != null)
            {
                var invocationList = handler.GetInvocationList();

                if(invocationList.Any() && invocationList.Contains(prospectiveHandler))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Device State Event Posters

        /// <summary>
        /// Post the ReelDeviceSpinStarted event.
        /// </summary>
        private void PostReelShelfSpinStarted()
        {
            CurrentDeviceState = ReelsSpunState.AllSpinningUp;

            OnReelDeviceSpinStarted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Post the ReelDeviceAtConstantSpeed event.
        /// </summary>
        private void PostReelShelfAtConstantSpeed()
        {
            CurrentDeviceState = ReelsSpunState.AllCompletedSpinUp;

            OnReelDeviceAtConstantSpeed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Post the ReelDeviceSpinningDown event.
        /// </summary>
        private void PostReelShelfSpinningDown()
        {
            CurrentDeviceState = ReelsSpunState.AllSpinningDown;

            OnReelDeviceSpinningDown?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Post the ReelDeviceSpinComplete event.
        /// </summary>
        private void PostReelShelfSpinComplete()
        {
            CurrentDeviceState = ReelsSpunState.AllStopped;

            OnReelDeviceSpinComplete?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Post the ReelDeviceMovingIrregularly event.
        /// </summary>
        private void PostReelShelfMovingIrreg()
        {
            CurrentDeviceState = ReelsSpunState.AllMovingIrregularly;

            OnReelDeviceMovingIrregularly?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Individual Reel Event Posters

        /// <summary>
        /// Post the ReelSpinStarted event.
        /// </summary>
        /// <param name="reelIndex">Index of the specified reel.</param>
        private void PostReelSpinStarted(byte reelIndex)
        {
            OnReelSpinStarted?.Invoke(this, new ReelEventArgs { ReelNumber = reelIndex });
        }

        /// <summary>
        /// Post the ReelSpinComplete event.
        /// </summary>
        /// <param name="reelIndex">Index of the specified reel.</param>
        private void PostReelSpinComplete(byte reelIndex)
        {
            OnReelSpinComplete?.Invoke(this, new ReelEventArgs { ReelNumber = reelIndex });
        }

        /// <summary>
        /// Post the ReelSpinAtConstantSpeed event.
        /// </summary>
        /// <param name="reelIndex">Index of the specified reel.</param>
        private void PostReelSpinAtConstantSpeed(byte reelIndex)
        {
            OnReelSpinAtConstantSpeed?.Invoke(this, new ReelEventArgs { ReelNumber = reelIndex });
        }

        /// <summary>
        /// Post the ReelSpinDecelerating event.
        /// </summary>
        /// <param name="reelIndex">Index of the specified reel.</param>
        private void PostReelSpinDecelerating(byte reelIndex)
        {
            OnReelSpinDecelerating?.Invoke(this, new ReelEventArgs { ReelNumber = reelIndex });
        }

        /// <summary>
        /// Post the ReelMovingIrregularly event.
        /// </summary>
        /// <param name="reelIndex">Index of the specified reel.</param>
        private void PostReelMovingIrregularly(byte reelIndex)
        {
            OnReelSpinIrregular?.Invoke(this, new ReelEventArgs { ReelNumber = reelIndex });
        }

        #endregion
    }
}
