//-----------------------------------------------------------------------
// <copyright file = "MechanicalReelsCategory.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using CSI.Schemas;
    using CSI.Schemas.Internal;
    using Foundation.Transport;
    using MechanicalReels;
    using CsiTransport;
    using Monitor = System.Threading.Monitor;
    using InternalCsiDirectionEnum = CSI.Schemas.Internal.Direction;

    /// <summary>
    /// Category for mechanical reels management.
    /// </summary>
    internal class MechanicalReelsCategory : CategoryBase<CsiReel>, IMechanicalReels, ICabinetUpdate
    {
        #region Private Fields

        /// <summary>
        /// CSI error response description to check for an overflowed command buffer in the firmware.
        /// The game should know about the overflow but it is not really an error that should throw an exception.
        /// </summary>
        private const string ChangeSpeedFirmwareBufferOverflowed = "Change_Speed_Command_Ignored_Buffer_Overflowed";

        /// <summary>
        /// Dictionary of reel features supported by this mechanical reels instance.
        /// </summary>
        private readonly Dictionary<string, ReelFeatureDescription> reelFeatures =
            new Dictionary<string, ReelFeatureDescription>();

        /// <summary>
        /// Queue of events - from the CSI Manager and internally generated.
        /// </summary>
        private readonly Queue<ReelStatusEventArgs> reelEventQueue = new Queue<ReelStatusEventArgs>();

        /// <summary>
        /// Dictionary of reel feature id vs. device and contained reel states.
        /// </summary>
        private readonly Dictionary<string, ReelsSpunStateManager> allReelStatuses =
            new Dictionary<string, ReelsSpunStateManager>();

        /// <summary>
        /// The list of all events the CSI supports. Internally we register for all of them to handle shelf-level
        /// state transitions.
        /// </summary>
        private readonly List<ReelStatus> internallyRegisteredEvents = new List<ReelStatus>
        {
            ReelStatus.Stopped,
            ReelStatus.Decelerating,
            ReelStatus.ConstantSpeed,
            ReelStatus.Accelerating,
            ReelStatus.MovingIrregularly,
        };

        /// <summary>
        /// Event handler for <see cref="ReelStatusEventArgs"/> individual reel events.
        /// </summary>
        private event EventHandler<ReelStatusEventArgs> OnReelStatusChanged;

        /// <summary>
        /// Event handler for <see cref="ReelsSpunEventArgs"/> device-level events.
        /// </summary>
        private event EventHandler<ReelsSpunEventArgs> OnReelsSpunStateChanged;

        #endregion Private Fields

        #region Properties

        /// <summary>
        /// The <see cref="FoundationTarget"/> that this category was created with.
        /// </summary>
        private FoundationTarget FoundationTarget
        {
            get;
        }

        /// <summary>
        /// Flag indicating if a foundation that supports attributes properly is currently in use.
        /// </summary>
        private bool AttributesSupported => VersionMajor >= 1 && VersionMinor >= 4;

        /// <summary>
        /// Flag indicating if a foundation that supports the AVP-style 'Snap' recovery is currently in use.
        /// </summary>
        private bool FoundationSnapRecoverySupported => VersionMajor >= 1 && VersionMinor >= 4;

        /// <summary>
        /// Flag indicating if a foundation supports enhanced reel control.
        /// </summary>
        private bool EnhancedReelControlSupported => VersionMajor >= 1 && VersionMinor >= 5;

        #endregion Properties

        #region Events

        /// <inheritdoc/>
        public event EventHandler<ReelStatusEventArgs> ReelStatusChangedEvent
        {
            add
            {
                if(!IsEventHandlerAlreadySubscribed(value, OnReelStatusChanged))
                {
                    OnReelStatusChanged += value;
                }
            }
            remove => OnReelStatusChanged -= value;
        }

        /// <summary>
        /// Event fired when the set of spinning reels' state changes.
        /// </summary>
        public event EventHandler<ReelsSpunEventArgs> ReelsSpunStateChangedEvent
        {
            add
            {
                if(!IsEventHandlerAlreadySubscribed(value, OnReelsSpunStateChanged))
                {
                    OnReelsSpunStateChanged += value;
                }
            }
            remove => OnReelsSpunStateChanged -= value;
        }

        /// <summary>
        /// Post the PostReelsSpunStateChangedEvent event.
        /// </summary>
        /// <param name="evtArgs">Arguments of type <see cref="ReelsSpunEventArgs"/>.</param>
        private void PostReelsSpunStateChangedEvent(ReelsSpunEventArgs evtArgs)
        {
            OnReelsSpunStateChanged?.Invoke(this, evtArgs);
        }

        /// <summary>
        /// Post the PostReelStatusChangedEvent event.
        /// </summary>
        /// <param name="eventArguments">Args of type <see cref="ReelsSpunEventArgs"/>.</param>
        private void PostReelStatusChangedEvent(ReelStatusEventArgs eventArguments)
        {
            OnReelStatusChanged?.Invoke(this, eventArguments);
        }

        /// <summary>
        /// Checks if an event handler delegate is already in an event handler's list.
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

        #endregion Events

        #region Constructors

        /// <summary>
        /// Overloaded constructor to create a specific version of this category.
        /// </summary>
        /// <param name="target">The <see cref="FoundationTarget"/> the game is running against.
        /// </param>
        public MechanicalReelsCategory(FoundationTarget target)
        {
            FoundationTarget = target;
        }

        #endregion Constructors

        #region CategoryBase<CsiReel> Implementation

        /// <inheritdoc/>
        public override Category Category => Category.CsiReel;

        /// <inheritdoc/>
        public override ushort VersionMajor => 1;

        /// <inheritdoc/>
        public override ushort VersionMinor => (ushort)(FoundationTarget.IsEqualOrNewer(FoundationTarget.AscentJSeriesMps) ? 5 : 4);

        /// <inheritdoc/>
        public override void HandleEvent(object message)
        {
            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if(!(message is CsiReel reelMessage))
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiReel), message.GetType()));
            }
            switch(reelMessage.Item)
            {
                case null:
                    throw new InvalidMessageException(
                        "Invalid message body. Message contained no event, request or response.");
                case ReelStatusEvent reelEvent:
                    EnqueueEvent(new ReelStatusEventArgs
                    {
                        FeatureId = reelEvent.FeatureId,
                        ReelNumber = reelEvent.Number,
                        Status = reelEvent.ReelStatus
                    });
                    break;
                default:
                    throw new UnhandledEventException("Event not handled: " + reelMessage.Item.GetType());
            }
        }

        /// <inheritdoc/>
        public override void HandleRequest(object message, ulong requestId)
        {
            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if(!(message is CsiReel reelMessage))
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiReel), message.GetType()));
            }
            if(reelMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Mechanical reel message contained no event, request or response.");
            }

            throw new UnhandledRequestException("Request not handled: " + reelMessage.Item.GetType());
        }

        /// <summary>
        /// Synchronizes reel states based on reel events. Posts events up the chain.
        /// </summary>
        /// <param name="reelStatusEvent">Event args of type <see cref="ReelStatusEventArgs"/>.</param>
        private void SyncAndPostReelStates(ReelStatusEventArgs reelStatusEvent)
        {
            allReelStatuses.TryGetValue(reelStatusEvent.FeatureId, out var currentReelsSpun);

            if(currentReelsSpun != null)
            {
                var stateChange = currentReelsSpun.UpdateStateWithNewEvent(reelStatusEvent);

                // Send the original event up the chain.
                PostReelStatusChangedEvent(reelStatusEvent);

                // Send a state change up the chain.
                if(stateChange)
                {
                    PostReelsSpunStateChangedEvent(new ReelsSpunEventArgs
                    {
                        FeatureId = reelStatusEvent.FeatureId,
                        ReelsSpunState = currentReelsSpun.CurrentReelsSpunState,
                    });
                }
            }
        }

        /// <summary>
        /// En-queue the given event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="ReelStatusEventArgs"/> to en-queue.</param>
        private void EnqueueEvent(ReelStatusEventArgs eventArgs)
        {
            lock(reelEventQueue)
            {
                reelEventQueue.Enqueue(eventArgs);
            }
        }

        #endregion CategoryBase<CsiReel> Implementation

        #region IMechanicalReels Implementation

        /// <inheritdoc/>
        public ICollection<ReelFeatureDescription> GetReelDevices()
        {
            var request = new CsiReel
            {
                Item = new GetReelDevicesRequest()
            };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetReelResponse<GetReelDevicesResponse>();
            CheckResponse(response.ReelResponse);
            reelFeatures.Clear();

            lock(allReelStatuses)
            {
                allReelStatuses.Clear();
            }

            if(response.Features?.Feature != null)
            {
                foreach(var reelFeatureDescription in response.Features.Feature)
                {
                    var featureId = reelFeatureDescription.FeatureId;
                    var reelSubFeature = GetReelSubFeature(reelFeatureDescription.ReelType);
                    var speeds = reelFeatureDescription.Speeds != null?
                                    reelFeatureDescription.Speeds.ToList():
                                    new List<ushort>();
                    var decelerations = reelFeatureDescription.Decelerations != null?
                                    reelFeatureDescription.Decelerations.ToList():
                                    new List<ushort>();
                    var csiAccelDecelerations = reelFeatureDescription.AccelerationDecelerationTimes != null?
                                    reelFeatureDescription.AccelerationDecelerationTimes.ToList():
                                    new List<AccelerationDecelerationTime>();

                    var accelDecelerations = csiAccelDecelerations.Any()?
                                    csiAccelDecelerations.Select(accelDeceleration =>
                                        new ReelAccelerationDecelerationTime
                                            {
                                                AccelerationTimeToNextSpeed = accelDeceleration.AccelerationTimeToNextSpeed,
                                                DecelerationTimeToPreviousSpeed = accelDeceleration.DecelerationTimeToPreviousSpeed
                                            }).ToList():
                                        new List<ReelAccelerationDecelerationTime>();

                    var reelsDescription = reelFeatureDescription.Reels.Select(
                        reelDescription => new ReelDescription(reelDescription.Stops,
                                                               reelDescription.MaxSeekTime)).ToList();

                    var featureDescription = new ReelFeatureDescription(featureId,
                                                                        reelSubFeature,
                                                                        speeds,
                                                                        decelerations,
                                                                        accelDecelerations,
                                                                        reelsDescription);

                    reelFeatures.Add(featureId, featureDescription);

                    // Create the device object for each feature.
                    var device = new ReelsSpunStateManager(reelFeatureDescription.Reels.Count);

                    lock(allReelStatuses)
                    {
                        allReelStatuses.Add(featureId, device);
                    }
                }
            }

            return reelFeatures.Values;
        }

        /// <inheritdoc/>
        public ReelCommandResult RequireDevice(string featureId)
        {
            // Do not verify the feature ID here because requiring a device that is not currently
            // connected is allowed.
            if(String.IsNullOrEmpty(featureId))
            {
                throw new ArgumentException("The feature ID cannot be null or empty.", nameof(featureId));
            }

            var request = new CsiReel
            {
                Item = new SetRequireDeviceRequest1
                {
                    FeatureId = featureId
                }
            };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetReelResponse<SetRequireDeviceResponse1>();
            var result = CheckResponse(response.ReelResponse);

            return result;
        }

        /// <inheritdoc/>
        public ReelCommandResult SetOnlineStatus(string featureId, bool online)
        {
            var result = ReelCommandResult.Success;

            VerifyReelFeatureDescription(featureId);

            if(online)
            {
                result = RegisterInternalEventHandlers(featureId);
            }

            return result;
        }

        /// <inheritdoc/>
        public ReelCommandResult SetRecoveryBehavior(string featureId, RecoveryOrder order, ReelDirection direction)
        {
            VerifyReelFeatureDescription(featureId);

            var request = new CsiReel
            {
                Item = new SetRecoveryRequest
                {
                    Ascending = order == RecoveryOrder.Ascending,
                    Direction = (InternalCsiDirectionEnum)direction,
                    FeatureId = featureId
                }
            };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetReelResponse<SetRecoveryResponse>();
            var result = CheckResponse(response.ReelResponse);

            return result;
        }

        /// <inheritdoc/>
        public ReelCommandResult SetStopOrder(string featureId, ICollection<byte> reels)
        {
            var reelDescription = GetReelFeatureDescription(featureId);
            ReelCommandVerifier.VerifySetStopOrder(reelDescription, reels);

            var request = new CsiReel
            {
                Item = new SetStopOrderRequest
                {
                    FeatureId = featureId,
                    Reels = string.Join(" ", reels.Select(reel => reel.ToString(CultureInfo.InvariantCulture)).ToArray())
                }
            };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetReelResponse<SetStopOrderResponse>();
            var result = CheckResponse(response.ReelResponse);

            return result;
        }

        /// <inheritdoc/>
        public ReelCommandResult SetToPosition(string featureId, ICollection<byte> reelStops, out bool foundationHandlesTiltWhileRecovering)
        {
            ReelCommandResult result;
            var reelDescription = GetReelFeatureDescription(featureId);

            ReelCommandVerifier.VerifyStops(reelDescription, reelStops);

            lock(allReelStatuses)
            {
                allReelStatuses.TryGetValue(featureId, out var reelsSpinningManager);
                reelsSpinningManager?.SetInitialReelStates(reelStops.Count);
            }

            // Foundation CSI versions 2.6.1+ have a dedicated SetToPosition command.
            if(FoundationSnapRecoverySupported)
            {
                foundationHandlesTiltWhileRecovering = true;
                result = SetToPositionByCommand(featureId, reelStops);
            }
            else
            {
                // Else use a regular spin.
                foundationHandlesTiltWhileRecovering = false;
                result = SetToPositionBySpin(featureId, reelStops);
            }

            return result;
        }

        /// <inheritdoc/>
        public ReelCommandResult SetSynchronousStops(string featureId, ICollection<ReelStop> reelStops)
        {
            var result = ReelCommandResult.SetSyncStopsFailedReelsNotSpinning;

            // If the reels are not spinning, then do nothing. This is not a perfect check,
            // as the reels may be on the edge of a state change to a valid state for setting sync-stops, but it should prevent
            // setting the stops while the reels are not moving.
            lock(allReelStatuses)
            {
                allReelStatuses.TryGetValue(featureId, out var reelsSpinningManager);
                if(reelsSpinningManager != null && (reelsSpinningManager.CurrentReelsSpunState == ReelsSpunState.AllStopped ||
                                                    reelsSpinningManager.CurrentReelsSpunState == ReelsSpunState.AllWaitingForCommand))
                {
                    return result;
                }
            }

            var reelDescription = GetReelFeatureDescription(featureId);
            ReelCommandVerifier.VerifyStops(reelDescription, reelStops);

            var spinRequestList = (from reelStop in reelStops
                                   select new SetSynchronousStopRequestReel
                                   {
                                       Number = reelStop.ReelNumber,
                                       Stop = reelStop.Stop
                                   }).ToList();

            var request = new CsiReel
            {
                Item = new SetSynchronousStopRequest
                {
                    FeatureId = featureId,
                    Reels = spinRequestList
                }
            };

            var affectedReels = spinRequestList.Count;

            // Only send the request if at least one reel will be affected.
            if(request.Item != null && affectedReels > 0)
            {
                Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
                var response = GetReelResponse<SetSynchronousStopResponse>();
                result =  CheckResponse(response.ReelResponse);
            }

            return result;
        }

        /// <inheritdoc/>
        public ReelCommandResult Spin(string featureId, ICollection<SpinProfile> spinProfiles)
        {
            var result = ReelCommandResult.SpinFailed;

            var reelDescription = GetReelFeatureDescription(featureId);
            ReelCommandVerifier.VerifySpin(reelDescription, spinProfiles);

            // Note for attributes:
            // - Attributes aren't supported (properly) in Pre-2.6.1 foundation versions.
            // - Cock and bounce are not supported by the reel firmware so they are always cleared here.
            // - Shake should not be set in a spin, only in an ApplyAttributes() command.
            // - By default we turn off all other attributes as they may exist from a previous
            //      ApplyAttributes() or other spin command sent with attributes. Only if
            //      'Hover' attributes are present in spinProfiles do we set them.
            var spinRequestList = new List<SpinRequestReel>();
            var reelsWithAttributes = 0;

            foreach(var spinProfile in spinProfiles)
            {
                var spinRequestReel = new SpinRequestReel
                {
                    Deceleration = (byte)spinProfile.Deceleration,
                    Direction = (InternalCsiDirectionEnum)spinProfile.Direction,
                    Duration = spinProfile.Duration,
                    Number = spinProfile.ReelNumber,
                    Speed = spinProfile.Speed,
                    Stop = spinProfile.Stop,
                    Bounce = false,
                    BounceSpecified = AttributesSupported,
                    Cock = false,
                    CockSpecified = AttributesSupported,
                    Shake = Shake.Off,
                    ShakeSpecified = AttributesSupported,
                    Hover = Hover.Off,
                    HoverSpecified = AttributesSupported
                };

                // Check for a hover attribute present in this command.
                if(AttributesSupported && spinProfile.Attributes != null &&
                   spinProfile.Attributes.Hover.Level != HoverLevel.Off)
                {
                    spinRequestReel.Hover = MapHoverAttribute(spinProfile.Attributes.Hover);
                    spinRequestReel.HoverLimits = MapCustomHoverLimits(spinProfile.Attributes.Hover);
                    reelsWithAttributes++;
                }

                spinRequestList.Add(spinRequestReel);
            }

            var request = new CsiReel
            {
                Item = new SpinRequest
                {
                    FeatureId = featureId,
                    Reels = spinRequestList
                }
            };

            var affectedReels = spinRequestList.Count;

            // Only send the request if at least one reel will be affected.
            if(request.Item != null && affectedReels > 0)
            {
                lock(allReelStatuses)
                {
                    allReelStatuses.TryGetValue(featureId, out var reelsSpinningManager);
                    if(reelsSpinningManager != null)
                    {
                        reelsSpinningManager.SetInitialReelStates(affectedReels);
                        reelsSpinningManager.ReelsSpunWithAttributesCount = reelsWithAttributes;
                    }
                }

                Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
                var response = GetReelResponse<SpinResponse>();
                result = CheckResponse(response.ReelResponse);
            }

            return result;
        }

        /// <inheritdoc/>
        public ReelCommandResult Stop(string featureId, ICollection<ReelStop> reelStops)
        {
            var result = ReelCommandResult.StopFailed;

            var reelDescription = GetReelFeatureDescription(featureId);
            ReelCommandVerifier.VerifyStops(reelDescription, reelStops);

            var request = new CsiReel();
            var stopRequestList = (from reelStop in reelStops
                                   select new StopRequestReel
                                   {
                                       Number = reelStop.ReelNumber,
                                       Stop = reelStop.Stop
                                   }).ToList();

            request.Item = new StopRequest
            {
                FeatureId = featureId,
                Reels = stopRequestList
            };

            var affectedReels = stopRequestList.Count;

            // Only send the request if at least one reel will be affected.
            if(request.Item != null && affectedReels > 0)
            {
                Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
                var response = GetReelResponse<StopResponse>();
                result = CheckResponse(response.ReelResponse);
            }

            return result;
        }

        /// <inheritdoc/>
        public ReelCommandResult SynchronousSpin(string featureId, ushort speedIndex, ICollection<SynchronousSpinProfile> spinProfiles)
        {
            var result = ReelCommandResult.SyncSpinFailedNoReelsAffected;

            var reelDescription = GetReelFeatureDescription(featureId);
            ReelCommandVerifier.VerifySynchronousSpin(reelDescription, speedIndex, spinProfiles);

            // Check and filter out reel numbers not in the correct state.
            var request = new CsiReel();
            var spinRequestList = (from synchSpinProfile in spinProfiles
                                   select new SynchronousSpinRequestReel
                                   {
                                       Direction = (InternalCsiDirectionEnum)synchSpinProfile.Direction,
                                       Duration = synchSpinProfile.Duration,
                                       Number = synchSpinProfile.ReelNumber,
                                       Stop = synchSpinProfile.Stop
                                   }).ToList();

            request.Item = new SynchronousSpinRequest
            {
                SpeedIndex = speedIndex,
                FeatureId = featureId,
                Reels = spinRequestList
            };

            var affectedReels = spinRequestList.Count;

            // Only send the request if at least one reel will be affected.
            if(request.Item != null && affectedReels > 0)
            {
                lock(allReelStatuses)
                {
                    allReelStatuses.TryGetValue(featureId, out var reelsSpinningManager);
                    reelsSpinningManager?.SetInitialReelStates(affectedReels);
                }

                Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
                var response = GetReelResponse<SynchronousSpinResponse>();
                result = CheckResponse(response.ReelResponse);
            }

            return result;
        }

        /// <inheritdoc/>
        public ReelCommandResult SynchronousStop(string featureId, ICollection<byte> reels)
        {
            var result = ReelCommandResult.SyncStopFailedNoReelsAffected;
            var reelDescription = GetReelFeatureDescription(featureId);
            ReelCommandVerifier.VerifySynchronousStop(reelDescription, reels);

            // If the reels are not in a sync-stop state, then do nothing. This is not a perfect check,
            // as the reels may be on the edge of a state change to a valid state for sync-stopping, but it should prevent
            // stopping while the reels are spinning up to speed, which defeats the purpose of a sync-stop.
            lock(allReelStatuses)
            {
                allReelStatuses.TryGetValue(featureId, out var reelsSpinningManager);
                if(reelsSpinningManager != null && !(reelsSpinningManager.CurrentReelsSpunState == ReelsSpunState.AllCompletedSpinUp ||
                                                     reelsSpinningManager.CurrentReelsSpunState == ReelsSpunState.AllSpinningDown))
                {
                    return result;
                }
            }

            // Check and filter out reel numbers not in the correct state.
            var request = new CsiReel();
            var slamStopRequestString = String.Join(" ", (from reel in reels
                                                          select reel.ToString(CultureInfo.InvariantCulture)).ToArray());

            request.Item = new SynchronousStopRequest
            {
                FeatureId = featureId,
                Reels = slamStopRequestString
            };

            var affectedReels = reels.Count;

            // Only send the request if at least one reel will be affected.
            if(request.Item != null && affectedReels > 0)
            {
                Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
                var response = GetReelResponse<SynchronousStopResponse>();
                result = CheckResponse(response.ReelResponse);
            }

            return result;
        }

        /// <inheritdoc/>
        public ReelCommandResult ApplyAttributes(string featureId, IDictionary<byte, SpinAttributes> attributes)
        {
            var result = ReelCommandResult.CommandIgnoredFoundationNotCorrectVersion;
            if(!AttributesSupported)
            {
                return result;
            }

            var request = new CsiReel();

            // This command needs to know how many reels are having attributes applied to them.
            var managedReels =
                attributes.Count(attribute => attribute.Value != null && (attribute.Value.Cock ||
                                                                          attribute.Value.Bounce ||
                                                                          attribute.Value.Shake != ShakeLevel.Off ||
                                                                          attribute.Value.Hover != HoverAttribute.Off));

            // Set cock and bounce to false no matter what was passed in.
            // These attributes are not supported by the firmware yet.
            var attributesRequest = attributes.Select(reelKvp => new ApplyAttributesRequestReel
            {
                Number = reelKvp.Key,
                Cock = false,
                CockSpecified = true,
                Bounce = false,
                BounceSpecified = true,
                Shake = (Shake)reelKvp.Value.Shake,
                ShakeSpecified = true,
                Hover = MapHoverAttribute(reelKvp.Value.Hover),
                HoverLimits = MapCustomHoverLimits(reelKvp.Value.Hover),
                HoverSpecified = true,
            }).ToList();

            request.Item = new ApplyAttributesRequest
            {
                FeatureId = featureId,
                Reels = attributesRequest
            };

            // Only send the request if at least one reel will be affected.
            if(request.Item != null && attributesRequest.Count > 0)
            {
                lock(allReelStatuses)
                {
                    allReelStatuses.TryGetValue(featureId, out var reelsSpinningManager);
                    if(reelsSpinningManager != null)
                    {
                        if(managedReels > 0 &&
                          (reelsSpinningManager.CurrentReelsSpunState == ReelsSpunState.AllStopped ||
                           reelsSpinningManager.CurrentReelsSpunState == ReelsSpunState.AllWaitingForCommand))
                        {
                            reelsSpinningManager.SetInitialReelStates(managedReels);
                            reelsSpinningManager.ReelsSpunWithAttributesCount = managedReels;
                        }
                    }
                }

                Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
                var response = GetReelResponse<ApplyAttributesResponse>();
                result = CheckResponse(response.ReelResponse);
            }

            return result;
        }

        /// <inheritdoc/>
        public ReelCommandResult ChangeSpeed(string featureId, IDictionary<byte, ChangeSpeedProfile> changeSpeedProfiles)
        {
            var result = ReelCommandResult.CommandIgnoredFoundationNotCorrectVersion;
            if(!EnhancedReelControlSupported)
            {
                return result;
            }

            var reelDescription = GetReelFeatureDescription(featureId);
            ReelCommandVerifier.VerifyChangeSpeedProfiles(reelDescription, changeSpeedProfiles);

            // The collection of new velocity profiles for the specified reels.
            var csiVelocityProfiles = (from changeSpeedProfile in changeSpeedProfiles
                                         select new ChangeReelSpeed
                                             {
                                                   Number = changeSpeedProfile.Value.Number,
                                                   SpeedIndex = (byte)changeSpeedProfile.Value.SpeedIndex,
                                                   Period = changeSpeedProfile.Value.Period,
                                                   Direction = (InternalCsiDirectionEnum)changeSpeedProfile.Value.Direction,
                                                   Immediate = changeSpeedProfile.Value.Immediate
                                             }
                                         ).ToList();

            // Create request.
            var request = new CsiReel
                              {
                                  Item = new ChangeSpeedRequest()
                                             {
                                                 FeatureId = featureId,
                                                 Reels = csiVelocityProfiles
                                             }
                              };

            var affectedReels = csiVelocityProfiles.Count;

            // Only send the request if at least one reel will be affected.
            if(request.Item != null && affectedReels > 0)
            {
                Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
                var response = GetReelResponse<MulipleReelResponse>();
                result = CheckResponseList(response.ReelRequestError);
            }

            return result;
        }

        /// <inheritdoc/>
        public void Update()
        {
            ReelStatusEventArgs eventToProcess = null;

            // Don't block if the event queue is locked by the enqueue thread.
            if(Monitor.TryEnter(reelEventQueue))
            {
                try
                {
                    if(reelEventQueue.Count > 0)
                    {
                        eventToProcess = reelEventQueue.Dequeue();
                    }
                }
                finally
                {
                    Monitor.Exit(reelEventQueue);
                }
            }

            if(eventToProcess != null)
            {
                SyncAndPostReelStates(eventToProcess);
            }
        }

        #endregion IMechanicalReels Implementation

        #region Private Methods

        /// <summary>
        /// Maps a <see cref="HoverAttribute"/> to a <see cref="Hover"/>.
        /// </summary>
        /// <param name="hoverAttribute">Incoming attribute.</param>
        /// <returns>Mapped attribute.</returns>
        private Hover MapHoverAttribute(HoverAttribute hoverAttribute)
        {
            Hover result;

            switch(hoverAttribute.Level)
            {
                case HoverLevel.Bottom:
                    result = Hover.Bottom;
                    break;

                case HoverLevel.Center:
                    result = Hover.Center;
                    break;

                case HoverLevel.Top:
                    result = Hover.Top;
                    break;

                case HoverLevel.Custom:
                    result = Hover.Custom;
                    break;

                default:
                    result = Hover.Off;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Maps the 'limits' in a <see cref="HoverAttribute"/> to a <see cref="CSI.Schemas.Internal.HoverLimits"/>.
        /// </summary>
        /// <param name="hoverLimits">Incoming attribute with hover limits set.</param>
        /// <returns>The <see cref="CSI.Schemas.Internal.HoverLimits"/> equivalent.</returns>
        private CSI.Schemas.Internal.HoverLimits MapCustomHoverLimits(HoverAttribute hoverLimits)
        {
            CSI.Schemas.Internal.HoverLimits result = null;

            if(hoverLimits.Level == HoverLevel.Custom)
            {
                result = new CSI.Schemas.Internal.HoverLimits
                {
                    LowerLimit = hoverLimits.Limits.LowerLimit,
                    UpperLimit = hoverLimits.Limits.UpperLimit
                };
            }

            return result;
        }

        /// <summary>
        /// Handles the 2.6.1+ foundation supported 'SetToPositionRequest' command.
        /// </summary>
        /// <param name="featureId">The featureId of the target reel.</param>
        /// <param name="reelStops">An enumerable collection of reel stops as integers.</param>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        private ReelCommandResult SetToPositionByCommand(string featureId, IEnumerable<byte> reelStops)
        {
            var result = ReelCommandResult.SpinFailed;

            var reelSetToPositionStops = reelStops.Select((stop, reelIndex) => new SetToPositionRequestReel
            {
                Number = (byte)reelIndex,
                Stop = stop
            }).ToList();

            var request = new CsiReel
            {
                Item = new SetToPositionRequest
                {
                    FeatureId = featureId,
                    Reels = reelSetToPositionStops
                }
            };

            var affectedReels = reelSetToPositionStops.Count;

            if(request.Item != null && affectedReels > 0)
            {
                Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
                var response = GetReelResponse<SetToPositionResponse>();
                result = CheckResponse(response.ReelResponse);
            }

            return result;
        }

        /// <summary>
        /// Handles the pre-2.6.1 functionality to set reel stops using a 'Spin' command.
        /// </summary>
        /// <param name="featureId">The featureId of the target reel.</param>
        /// <param name="reelStops">An enumerable collection of reel stops as integers.</param>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        private ReelCommandResult SetToPositionBySpin(string featureId, IEnumerable<byte> reelStops)
        {
            var result = ReelCommandResult.SpinFailed;

            var request = new CsiReel();
            var reelSetToPositionStops = reelStops.Select((stop, reelIndex) => new SpinRequestReel
            {
                Speed = 0,
                Stop = stop,
                Deceleration = 0,
                Direction = InternalCsiDirectionEnum.Shortest,
                Duration = 0,
                Number = (byte)reelIndex,
                Bounce = false,
                BounceSpecified = true,
                Cock = false,
                CockSpecified = true,
                Shake = Shake.Off,
                ShakeSpecified = true,
                Hover = Hover.Off,
                HoverSpecified = false,
                HoverLimits = null
            }).ToList();

            request.Item = new SpinRequest
            {
                FeatureId = featureId,
                Reels = reelSetToPositionStops
            };

            var affectedReels = reelSetToPositionStops.Count;

            // Only send the request if at least one reel will be affected.
            if(request.Item != null && affectedReels > 0)
            {
                Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
                var response = GetReelResponse<SpinResponse>();
                result = CheckResponse(response.ReelResponse);
            }

            return result;
        }

        /// <summary>
        /// Wait for a specific response.
        /// </summary>
        /// <typeparam name="TResponse">The type of response to wait for.</typeparam>
        /// <returns>The requested response.</returns>
        /// <exception cref="UnexpectedReplyTypeException">
        /// Thrown when the response is not the type expected.
        /// </exception>
        private TResponse GetReelResponse<TResponse>() where TResponse : class
        {
            var response = GetResponse();
            if(!(response.Item is TResponse innerResponse))
            {
                throw new UnexpectedReplyTypeException("Unexpected response.", typeof(TResponse), response.GetType());
            }

            return innerResponse;
        }

        /// <summary>
        /// Checks a collection of individual reel error responses.
        /// </summary>
        /// <param name="responseList">The list of <see cref="ReelRequestError"/> responses to check.</param>
        /// <returns>If any individual reels returned an error, <see cref="ReelCommandResult"/> is returned,
        /// else <see cref="ReelCommandResult.Success"/> is returned.</returns>
        private static ReelCommandResult CheckResponseList(IList<ReelRequestError> responseList)
        {
            if(responseList?.Any() != true)
            {
                return ReelCommandResult.Success;
            }

            var hasReelErrors = responseList.Any(response => response.ErrorCode != ReelErrorCode.NONE);

            return hasReelErrors?
                ReelCommandResult.ChangeSpeedResultedInOneOrMoreReelErrors:
                ReelCommandResult.Success;
        }

        /// <summary>
        /// Check the response to see if there are any errors.
        /// </summary>
        /// <param name="response">The response to check.</param>
        /// <exception cref="MechanicalReelCategoryException">Thrown if the response indicates that there was an
        /// error not in the expected list of common errors.</exception>
        /// <returns>The appropriate <see cref="ReelCommandResult"/>.</returns>
        private static ReelCommandResult CheckResponse(ReelResponse response)
        {
            var result = ReelCommandResult.Success;
            var shouldThrow = false;

            switch(response.ErrorCode)
            {
                // These are ignored; the two failed codes are sometimes returned because of
                // foundation issues with heavy usage of spin/stop commands.
                case ReelErrorCode.NONE:
                case ReelErrorCode.STOP_SPIN_FAILED:
                case ReelErrorCode.START_SPIN_FAILED:
                    break;

                case ReelErrorCode.REEL_DEVICE_TILTED:
                    result = ReelCommandResult.CommandIgnoredAsReelDeviceIsTilted;
                    break;

                case ReelErrorCode.CLIENT_DOES_NOT_OWN_RESOURCE:
                    result = ReelCommandResult.CommandIgnoredAsReelDeviceIsNotAcquired;
                    break;

                case ReelErrorCode.CHANGE_SPEED_FAILED:
                    if(!string.IsNullOrEmpty(response.ErrorDescription) &&
                        string.Compare(response.ErrorDescription,
                                       ChangeSpeedFirmwareBufferOverflowed,
                                       StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        result = ReelCommandResult.CommandIgnoredAsReelDeviceIsTilted;
                    }
                    else
                    {
                        shouldThrow = true;
                    }
                    break;

                default:
                    shouldThrow = true;
                    break;
            }

            if(shouldThrow)
            {
                throw new MechanicalReelCategoryException(response.ErrorCode.ToString(), response.ErrorDescription);
            }

            return result;
        }

        /// <summary>
        /// Verify that the given feature ID is valid.
        /// </summary>
        /// <param name="featureId">The Feature Id to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if featureId is null.</exception>
        /// <exception cref="InvalidReelException">Thrown if the reel number is not present in the reel device.</exception>
        private void VerifyReelFeatureDescription(string featureId)
        {
            if(String.IsNullOrEmpty(featureId))
            {
                throw new ArgumentNullException(nameof(featureId), "The feature ID may not be null.");
            }

            if(!reelFeatures.ContainsKey(featureId))
            {
                throw new InvalidFeatureIdException(featureId);
            }
        }

        /// <summary>
        /// Return a reel feature description object from the collection of all reel features.
        /// </summary>
        /// <param name="featureId">The Feature Id to get.</param>
        /// <returns>A <see cref="ReelFeatureDescription"/> object matching the Feature Id.</returns>
        private ReelFeatureDescription GetReelFeatureDescription(string featureId)
        {
            VerifyReelFeatureDescription(featureId);
            return reelFeatures[featureId];
        }

        /// <summary>
        /// Get the ReelSubFeature based on the device type.
        /// </summary>
        /// <param name="deviceType">The device type to get the feature for.</param>
        /// <returns>The ReelSubFeature for the device type.</returns>
        private static ReelSubFeature GetReelSubFeature(ReelType deviceType)
        {
            switch(deviceType)
            {
                case ReelType.BonusReels:
                    return ReelSubFeature.BonusReels;

                case ReelType.Dice:
                    return ReelSubFeature.Dice;

                case ReelType.GamePlayReels:
                    return ReelSubFeature.GamePlayReels;

                case ReelType.LinearSlider:
                    return ReelSubFeature.LinearSlider;

                case ReelType.Pointer:
                    return ReelSubFeature.Pointer;

                case ReelType.Prism:
                    return ReelSubFeature.Prism;

                case ReelType.Sphere:
                    return ReelSubFeature.Sphere;

                case ReelType.Wheel:
                    return ReelSubFeature.Wheel;

                default:
                    return ReelSubFeature.Unknown;
            }
        }

        /// <summary>
        /// Handles the internal registration of all required events.
        /// </summary>
        /// <param name="featureId">The Feature Id of the device to register.</param>
        private ReelCommandResult RegisterInternalEventHandlers(string featureId)
        {
            VerifyReelFeatureDescription(featureId);
            var internRegList = new List<ReelStatusEvent>();
            var reelCount = reelFeatures[featureId].ReelDescriptions.Count();

            foreach(var eventEnum in internallyRegisteredEvents)
            {
                internRegList.AddRange(Enumerable.Range(0, reelCount).Select(reelNum =>
                                                                             new ReelStatusEvent
                                                                             {
                                                                                 FeatureId = featureId,
                                                                                 Number = (byte)reelNum,
                                                                                 ReelStatus = eventEnum
                                                                             }).ToList());
            }

            var request = new CsiReel
            {
                Item = new RegisterStatusRequest
                {
                    StatusSubscription = internRegList
                }
            };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetReelResponse<RegisterStatusResponse>();
            var result = CheckResponse(response.ReelResponse);

            return result;
        }

        #endregion Private Methods
    }
}
