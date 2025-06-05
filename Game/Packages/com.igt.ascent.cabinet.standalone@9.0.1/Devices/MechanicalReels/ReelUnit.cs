//-----------------------------------------------------------------------
// <copyright file = "ReelUnit.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.MechanicalReels
{
    using System;
    using System.Collections.Generic;
    using Cabinet.MechanicalReels;
    using CSI.Schemas;

    /// <summary>
    /// Class which encapsulates the settings and status of a reel.
    /// </summary>
    internal class ReelUnit
    {
        #region Properties

        /// <summary>
        /// Get the index of the reel.
        /// </summary>
        public byte ReelNumber { get; }

        /// <summary>
        /// Get the descriptor of the reel received from the driver.
        /// </summary>
        public ReelDescriptor Descriptor { get; }

        /// <summary>
        /// Get the current spin profile of the reel.
        /// </summary>
        public SpinProfile SpinProfile { get; }

        /// <summary>
        /// Get the status registrations for the reel.
        /// </summary>
        public HashSet<ReelStatus> StatusRegistrations { get; }

        /// <summary>
        /// Get or set the status code of the reel.
        /// </summary>
        public ReelStatusCode StatusCode { get; set; }

        /// <summary>
        /// Get or set the status of the reel.
        /// </summary>
        public ReelStatus Status { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initialize an instance of <see cref="ReelUnit"/>.
        /// </summary>
        /// <param name="reelNumber">The reel number for the reel unit.</param>
        /// <param name="reelDescriptor">The descriptor of the reel.</param>
        public ReelUnit(byte reelNumber, ReelDescriptor reelDescriptor)
        {
            ReelNumber = reelNumber;
            Descriptor = reelDescriptor ?? throw new ArgumentNullException(nameof(reelDescriptor));

            SpinProfile = new SpinProfile
                          {
                              ReelNumber = reelNumber,

                              // The driver always reset the stop to 0xFF
                              // after the reels have spun.  So no point
                              // for us to remember it.
                              Stop = SpinProfile.NoStop,

                              // To avoid null check in the future.
                              Attributes = new SpinAttributes()
                          };

            StatusRegistrations = new HashSet<ReelStatus>();

            // Initialize status to Stopped.
            Status = ReelStatus.Stopped;
        }

        #endregion
    }
}
