//-----------------------------------------------------------------------
// <copyright file = "TimeSpanWatch.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Timing
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// This class provides the system time stamp.
    /// </summary>
    /// <remarks>
    /// This class is backed by Win32 API QueryPerformanceCounter().
    /// We didn't use Stopwatch.GetTimestamp() to implement the timestamps in this class because mono implements it different than Microsoft .Net library.
    /// Because Mono starts the counter only when the API mono_100ns_ticks() is firstly called, we wouldn't be able to compare the timestamp returned from
    /// Stopwatch.GetTimestamp() with other Unity and .Net processes.
    /// As a result, we have to use Win32 API QueryPerformanceCounter() to implement the functions in this class instead.
    /// </remarks>
    public static class TimeSpanWatch
    {
        #region PInvoke Methods

        // Reference https://docs.microsoft.com/en-us/windows/win32/sysinfo/acquiring-high-resolution-time-stamps
        [DllImport("kernel32.dll")]
        [SuppressUnmanagedCodeSecurity]
        private static extern bool QueryPerformanceCounter(out long value);
        
        [DllImport("kernel32.dll")]
        [SuppressUnmanagedCodeSecurity]
        private static extern bool QueryPerformanceFrequency(out long value);
        
        #endregion

        /// <summary>
        /// Static constructor to initialize the frequency of the performance counter.
        /// </summary>
        static TimeSpanWatch()
        {
            QueryPerformanceFrequency(out var frequency);
            Frequency = frequency;
        }

        /// <summary>
        /// The frequency of the performance counter. 
        /// </summary>
        private static readonly long Frequency;

        /// <summary>
        /// Gets the performance counter value.
        /// </summary>
        private static long GetTimestamp()
        {
            QueryPerformanceCounter(out var counter);
            return counter;
        }

        /// <summary>
        /// Gets the system time stamp in the form of <see cref="TimeSpan"/>.
        /// </summary>
        public static TimeSpan Now
        {
            get
            {
                var timestampInSeconds = (double)GetTimestamp() / Frequency;
                return TimeSpan.FromTicks(unchecked ((long)(timestampInSeconds * TimeSpan.TicksPerSecond)));
            }
        }

        /// <summary>
        /// Gets the system time stamp in millisecond resolution.
        /// </summary>
        public static long TimeStampMilliseconds => GetTimestamp() * 1000L / Frequency;
    }
}
