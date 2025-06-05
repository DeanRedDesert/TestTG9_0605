//-----------------------------------------------------------------------
// <copyright file = "TimeStampedEventArgs.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Timing
{
    using System;

    /// <summary>
    /// Event arguments that include a millisecond time stamp member.
    /// </summary>
    public class TimeStampedEventArgs : EventArgs
    {
        /// <summary>
        /// A millisecond time stamp based on the performance counter or system timer.
        /// </summary>
        /// <remarks>
        /// If the hardware and operating system support the use of the HPET, that counter should be used. Otherwise, the system
        /// timer should be used.
        /// </remarks>
        public long TimeStamp { get; }

        /// <summary>
        /// Construct a <see cref="TimeStampedEventArgs"/> using the current millisecond system time stamp.
        /// </summary>
        public TimeStampedEventArgs()
        {
            TimeStamp = TimeSpanWatch.TimeStampMilliseconds;
        }

        /// <summary>
        /// Construct a <see cref="TimeStampedEventArgs"/> with the given time stamp value.
        /// </summary>
        /// <param name="timeStamp">The millisecond time stamp associated with this event. See <see cref="TimeStamp"/>.</param>
        public TimeStampedEventArgs(long timeStamp)
        {
            TimeStamp = timeStamp;
        }
    }
}
