//-----------------------------------------------------------------------
// <copyright file = "ReelState.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System;
    using System.Collections.Generic;
    using CSI.Schemas;
    using MechanicalReels;

    /// <summary>
    /// Class used to track the state of a simulated reel.
    /// </summary>
    internal class ReelState
    {
        #region Nested Types

        /// <summary>
        /// Class which contains simulated reel timing characteristics.
        /// </summary>
        private class ReelTimingCharacteristics
        {
            /// <summary>
            /// The delay between being issued a spin command and accelerating.
            /// </summary>
            public int StartDelay { set; get; }

            /// <summary>
            /// The amount of time needed to accelerate.
            /// </summary>
            public int AccelerationDelay { set; get; }

            /// <summary>
            /// The amount of time required to decelerate.
            /// </summary>
            public int DecelerationDelay { set; get; }

            /// <summary>
            /// Total time taken by the timing characteristics.
            /// </summary>
            public int Total => StartDelay + AccelerationDelay + DecelerationDelay;
        }

        /// <summary>
        /// Internal states to the reels.
        /// </summary>
        public enum ReelStates
        {
            /// <summary>
            /// The reel is accelerating.
            /// </summary>
            Accelerating,

            /// <summary>
            /// The reel is at a constant speed.
            /// </summary>
            StartConstantSpeed,

            /// <summary>
            /// The reel is waiting with a constant speed until a stop is specified. If a stop is
            /// already specified, then it will transition to the next state after the specified interval.
            /// </summary>
            HoldingConstant,

            /// <summary>
            /// The reel is decelerating.
            /// </summary>
            Decelerating,

            /// <summary>
            /// The reel is stopped.
            /// </summary>
            Stopped
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// The last profile used to initiate actions on the reel.
        /// </summary>
        private SpinProfile requestedProfile = new SpinProfile();

        /// <summary>
        /// The feature this reel is a part of.
        /// </summary>
        private readonly string featureId;

        /// <summary>
        /// The reel number for this reel.
        /// </summary>
        private readonly byte reelNumber;

        /// <summary>
        /// The timing characteristics for this reel.
        /// </summary>
        private readonly ReelTimingCharacteristics timingCharacteristics;

        /// <summary>
        /// Table of handlers for the reel states.
        /// </summary>
        private readonly Dictionary<ReelStates, Func<int>> stateHandlers;

        /// <summary>
        /// Duration override for stop order.
        /// </summary>
        private int overrideDuration;

        #endregion

        #region Public Properties

        /// <summary>
        /// Flag which indicates if this reel is active and needs to be processed.
        /// </summary>
        public bool Active { private set; get; }

        /// <summary>
        /// The currently executing reel state.
        /// </summary>
        public ReelStates CurrentState { private set; get; }

        /// <summary>
        /// The next reel state to execute.
        /// </summary>
        public ReelStates NextState { private set; get; }

        /// <summary>
        /// The direction the reel is spinning. The direction can only be changed when the reel is stopped.
        /// </summary>
        public ReelDirection Direction { private set; get; }

        /// <summary>
        /// Get the spin duration.
        /// </summary>
        public int Duration => overrideDuration == 0 ? requestedProfile.Duration : overrideDuration;

        /// <summary>
        /// Gets a flag indicating if the current spin is synchronous.
        /// </summary>
        public bool Synchronous { private set; get; }

        /// <summary>
        /// The total timing delay caused by the timing characteristics.
        /// </summary>
        public int TimingDelay => timingCharacteristics.Total;

        #endregion

        #region Events

        /// <summary>
        /// Event used to convey changes in reel status.
        /// </summary>
        public event EventHandler<ReelStatusEventArgs> ReelStatusChanged;

        #endregion

        #region Constructor

        /// <summary>
        /// Construct a reel state for the given reel description.
        /// </summary>
        /// <param name="featureId">The feature this reel is port of.</param>
        /// <param name="reelNumber">The number of this reel.</param>
        public ReelState(string featureId, byte reelNumber)
        {
            this.featureId = featureId;
            this.reelNumber = reelNumber;
            CurrentState = ReelStates.Stopped;
            NextState = ReelStates.Stopped;

            timingCharacteristics = new ReelTimingCharacteristics
            {
                AccelerationDelay = 250,
                DecelerationDelay = 250,
                StartDelay = 10
            };

            stateHandlers = new Dictionary<ReelStates, Func<int>>
                                {
                                    {ReelStates.Stopped, HandleStopped},
                                    {ReelStates.Accelerating, HandleAccelerating},
                                    {ReelStates.StartConstantSpeed, HandleStartConstantSpeed},
                                    {ReelStates.HoldingConstant, HandleHoldConstantSpeed},
                                    {ReelStates.Decelerating, HandleDecelerating}
                                };
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Spin the reel.
        /// </summary>
        /// <param name="spinProfile">Profile to use for spinning the reel.</param>
        /// <exception cref="ArgumentNullException">Thrown if spinProfile is null.</exception>
        /// <returns>Time interval to call process.</returns>
        public int Spin(SpinProfile spinProfile)
        {
            if(spinProfile == null)
            {
                throw new ArgumentNullException(nameof(spinProfile), "Parameter may not be null.");
            }

            Synchronous = false;
            return SpinInternal(spinProfile);
        }

        /// <summary>
        /// Stop the reel.
        /// </summary>
        /// <param name="reelStop">The reel stop information.</param>
        /// <exception cref="ArgumentNullException">Thrown if reelStop is null.</exception>
        /// <returns>Time interval to call process.</returns>
        public int Stop(ReelStop reelStop)
        {
            if(reelStop == null)
            {
                throw new ArgumentNullException(nameof(reelStop), "Parameter may not be null.");
            }

            Synchronous = false;
            return StopInternal(reelStop);
        }

        /// <summary>
        /// Set the synchronous stop for the reel.
        /// </summary>
        /// <param name="reelStop">The stop to set.</param>
        /// <exception cref="ArgumentNullException">Thrown if reelStop is null.</exception>
        /// <returns>Time interval to call process.</returns>
        public void SetSynchronousStop(ReelStop reelStop)
        {
            if(reelStop == null)
            {
                throw new ArgumentNullException(nameof(reelStop), "Parameter may not be null.");
            }

            if(!Synchronous || (Synchronous && requestedProfile.Stop == SpinProfile.NoStop))
            {
                Synchronous = true;
                requestedProfile.Stop = reelStop.Stop;
            }
        }

        /// <summary>
        /// Synchronously stop the reel using the set synchronous stop.
        /// </summary>
        /// <returns>Time interval to call process.</returns>
        /// <remarks>
        /// If the reel is spinning and a synchronous stop has not been set, then the request to stop will be ignored
        /// and the reel will spin indefinitely.
        /// </remarks>
        public int SynchronousStop()
        {
            if(Synchronous && requestedProfile.Stop != SpinProfile.NoStop)
            {
                return StopInternal(new ReelStop { ReelNumber = reelNumber, Stop = requestedProfile.Stop });
            }

            if(CurrentState != ReelStates.Stopped && CurrentState != ReelStates.Decelerating)
            {
                //Need to spin forever.
                requestedProfile.Stop = SpinProfile.NoStop;

                if(CurrentState == ReelStates.HoldingConstant)
                {
                    NextState = ReelStates.HoldingConstant;
                }
            }

            return 0;
        }

        /// <summary>
        /// Synchronously spin the reel.
        /// </summary>
        /// <param name="spinProfile">Profile to use for spinning the reel.</param>
        /// <exception cref="ArgumentNullException">Thrown if spinProfile is null.</exception>
        /// <returns>Time interval to call process.</returns>
        public int SynchronousSpin(SynchronousSpinProfile spinProfile)
        {
            if(spinProfile == null)
            {
                throw new ArgumentNullException(nameof(spinProfile), "Parameter may not be null.");
            }

            var regularSpinProfile = new SpinProfile(spinProfile.ReelNumber, spinProfile.Stop, spinProfile.Duration,
                                                     spinProfile.Direction);

            Synchronous = true;

            return SpinInternal(regularSpinProfile);
        }

        /// <summary>
        /// Process the reel.
        /// </summary>
        /// <returns>Time interval to call process again.</returns>
        public int Process()
        {
            CurrentState = NextState;
            int result = stateHandlers[CurrentState]();
            return result;
        }

        /// <summary>
        /// Override the duration for stop order.
        /// </summary>
        /// <param name="duration">The override duration. 0 to use the profile duration.</param>
        public void SetOverrideDuration(int duration)
        {
            overrideDuration = duration;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Post the specified reel status.
        /// </summary>
        /// <param name="status">The status to post.</param>
        private void PostStatus(ReelStatus status)
        {
            ReelStatusChanged?.Invoke(this,
                new ReelStatusEventArgs
                {
                    FeatureId = featureId,
                    ReelNumber = reelNumber,
                    Status = status
                });
        }

        /// <summary>
        /// Spin the reel. Internal implementation shared by synchronous and asynchronous spins.
        /// </summary>
        /// <param name="spinProfile">Profile to use for spinning the reel.</param>
        /// <exception cref="ArgumentNullException">Thrown if passed a null spinProfile.</exception>
        /// <returns>0 always.</returns>
        private int SpinInternal(SpinProfile spinProfile)
        {
            requestedProfile = spinProfile ?? throw new ArgumentNullException(nameof(spinProfile), "Parameter may not be null.");
            Active = true;

            switch(CurrentState)
            {
                case ReelStates.Stopped:
                    Direction = requestedProfile.Direction;
                    NextState = ReelStates.Accelerating;
                    return timingCharacteristics.StartDelay;
                case ReelStates.HoldingConstant:
                    NextState = ReelStates.HoldingConstant;
                    break;
            }

            //Not sure what should happen when decelerating. It isn't a good idea to issue a command
            //while the reel is decelerating.

            return 0;
        }

        /// <summary>
        /// Stop the reel. Internal implementation shared by synchronous and asynchronous spins.
        /// </summary>
        /// <param name="reelStop">The <see cref="ReelStop"/> object with reel stop info.</param>
        /// <exception cref="ArgumentNullException">Thrown if reelStop is null.</exception>
        /// <returns>0 always.</returns>
        private int StopInternal(ReelStop reelStop)
        {
            if(reelStop == null)
            {
                throw new ArgumentNullException(nameof(reelStop), "Parameter may not be null.");
            }

            requestedProfile.Stop = reelStop.Stop;

            if(CurrentState != ReelStates.Stopped && CurrentState != ReelStates.Decelerating)
            {
                NextState = ReelStates.Decelerating;
            }

            return 0;
        }

        #endregion

        #region Reel State Handlers

        /// <summary>
        /// Handler for the stop state.
        /// </summary>
        /// <returns>Delay before next state should be executed.</returns>
        private int HandleStopped()
        {
            Active = false;
            Synchronous = false;
            requestedProfile = new SpinProfile();

            PostStatus(ReelStatus.Stopped);
            NextState = ReelStates.Stopped;

            return 0;
        }

        /// <summary>
        /// Handler for the accelerating state.
        /// </summary>
        /// <returns>Delay before next state should be executed.</returns>
        private int HandleAccelerating()
        {
            PostStatus(ReelStatus.Accelerating);
            NextState = ReelStates.StartConstantSpeed;
            return timingCharacteristics.AccelerationDelay;
        }

        /// <summary>
        /// Handler for the start constant speed state.
        /// </summary>
        /// <returns>Delay before next state should be executed.</returns>
        private int HandleStartConstantSpeed()
        {
            PostStatus(ReelStatus.ConstantSpeed);
            NextState = ReelStates.HoldingConstant;
            return 0;
        }

        /// <summary>
        /// Handler for the hold constant speed state.
        /// </summary>
        /// <returns>Delay before next state should be executed.</returns>
        private int HandleHoldConstantSpeed()
        {
            // If a stop is specified then we will spin for the given duration.
            // Otherwise continue to hold until a new command is issued.
            if(requestedProfile.Stop != SpinProfile.NoStop)
            {
                NextState = ReelStates.Decelerating;

                var delay = Duration - (timingCharacteristics.AccelerationDelay +
                                        timingCharacteristics.DecelerationDelay);

                delay = delay < 0 ? 0 : delay;

                return delay;
            }

            NextState = ReelStates.HoldingConstant;
            Active = false;
            return 0;
        }

        /// <summary>
        /// Handler for the decelerating state.
        /// </summary>
        /// <returns>Delay before next state should be executed.</returns>
        private int HandleDecelerating()
        {
            PostStatus(ReelStatus.Decelerating);
            NextState = ReelStates.Stopped;
            return timingCharacteristics.DecelerationDelay;
        }

        #endregion
    }
}
