//-----------------------------------------------------------------------
// <copyright file = "ValueRange.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System;

    /// <summary>
    /// Define the value range type where all values are within the range from min to max value.
    /// </summary>
    /// <typeparam name="T">The type of value in the value range.</typeparam>
    public class ValueRange<T> : IValuePool<T> where T: IComparable
    {
        /// <summary>
        /// The minimum value of this range.
        /// </summary>
        public T Min { get; }
        /// <summary>
        /// The maximum value of this range.
        /// </summary>
        public T Max { get; }

        /// <summary>
        /// Constructor with specified range of values.
        /// </summary>
        /// <param name="min">The minimum value of the range.</param>
        /// <param name="max">The maximum value of the range.</param>
        public ValueRange(T min, T max)
        {
            Min = min;
            Max = max;
        }

        /// <inheritDoc />
        public bool Contains(T value)
        {
            return Min.CompareTo(value) <= 0 && Max.CompareTo(value) >= 0;
        }
    }
}
