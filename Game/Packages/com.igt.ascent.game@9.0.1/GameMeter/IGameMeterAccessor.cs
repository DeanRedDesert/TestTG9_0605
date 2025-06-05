// -----------------------------------------------------------------------
// <copyright file = "IGameMeterAccessor.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameMeter
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This interface defines APIs for accessing game meters in safe storage whose values are of type long.
    /// This interface assumes that all meters are located in the same critical data scope which is known to
    /// the interface implementation class.
    /// </summary>
    public interface IGameMeterAccessor
    {
        /// <summary>
        /// Gets the flag indicating whether or not the game meter accessor is write-enabled.
        /// </summary>
        bool WriteEnabled { get; }

        /// <summary>
        /// Sets the value to the specific meter in the critical data.
        /// </summary>
        /// <param name="meterPath">
        /// The specified meter path to set. It must be a valid path for critical data.
        /// <list type="number">
        /// <item>
        /// The character set for the name is limited to a subset of
        /// ASCII characters that include numeric, alphabetic and the
        /// characters '/', '.', '_', and '-'.
        /// </item>
        /// <item>
        /// The name cannot start with '/'.
        /// </item>
        /// </list>
        /// </param>
        /// <param name="value">The value to set.</param>
        /// <remarks>
        /// <para>
        /// This method can be called only from within a method that provides an open transaction.
        /// </para>
        /// <para>
        /// Invoking this method shall create one meter in the critical data if the specified
        /// meter doesn't exist, and the new meter's value is set to <paramref name="value"/>.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when there are illegal characters in the meter path which is being validated by
        /// <see cref="IGT.Game.Core.Communication.Foundation.Utility.ValidateCriticalDataName"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the game meter accessor is not write-enabled.
        /// </exception>
        void SetMeter(string meterPath, long value);

        /// <summary>
        /// Increments the specific meter by <paramref name="value"/> with the specified meter
        /// path in critical data.
        /// </summary>
        /// <param name="meterPath">
        /// The specified meter path to increment. It must be a valid path for critical data.
        /// <list type="number">
        /// <item>
        /// The character set for the name is limited to a subset of
        /// ASCII characters that include numeric, alphabetic and the
        /// characters '/', '.', '_', and '-'.
        /// </item>
        /// <item>
        /// The name cannot start with '/'.
        /// </item>
        /// </list>
        /// </param>
        /// <param name="value">
        /// The value to increment the meter by.
        /// </param>
        /// <remarks>
        /// <para>
        /// This method can be called only from within a method that provides an open transaction.
        /// </para>
        /// <para>
        /// Invoking this method shall create one meter in the critical data if the specified
        /// meter doesn't exist, and the new meter's value is set to <paramref name="value"/>.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when there are illegal characters in the meter path which is being validated by
        /// <see cref="IGT.Game.Core.Communication.Foundation.Utility.ValidateCriticalDataName"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the game meter accessor is not write-enabled.
        /// </exception>
        void IncrementMeter(string meterPath, long value);

        /// <summary>
        /// Increments the specific meter by 1 with the specified meter path in critical data.
        /// </summary>
        /// <param name="meterPath">
        /// The specified meter path to increment. It must be a valid path for critical data.
        /// <list type="number">
        /// <item>
        /// The character set for the name is limited to a subset of
        /// ASCII characters that include numeric, alphabetic and the
        /// characters '/', '.', '_', and '-'.
        /// </item>
        /// <item>
        /// The name cannot start with '/'.
        /// </item>
        /// </list>
        /// </param>
        /// <remarks>
        /// <para>
        /// This method can be called only from within a method that provides an open transaction.
        /// </para>
        /// <para>
        /// Invoking this method shall create one meter in the critical data if the specified
        /// meter doesn't exist, and the new meter's value is set to 1.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when there are illegal characters in the meter path which is being validated by
        /// <see cref="IGT.Game.Core.Communication.Foundation.Utility.ValidateCriticalDataName"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the game meter accessor is not write-enabled.
        /// </exception>
        void IncrementMeter(string meterPath);

        /// <summary>
        /// Increments multiple meters by 1 with the specified meter paths in the critical data.
        /// </summary>
        /// <param name="meterPaths">
        /// The list of specified meter paths. All meter paths must be valid paths for critical data.
        /// <list type="number">
        /// <item>
        /// The character set for the name is limited to a subset of
        /// ASCII characters that include numeric, alphabetic and the
        /// characters '/', '.', '_', and '-'.
        /// </item>
        /// <item>
        /// The name cannot start with '/'.
        /// </item>
        /// </list>
        /// </param>
        /// <remarks>
        /// <para>
        /// This method can be called only from within a method that provides an open transaction.
        /// </para>
        /// <para>
        /// Invoking this method shall create meters in the critical data if any of the specified
        /// meters do not exist, and the new meters' values are set to 1.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when there are illegal characters in the meter paths which are being validated by
        /// <see cref="IGT.Game.Core.Communication.Foundation.Utility.ValidateCriticalDataName"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the game meter accessor is not write-enabled.
        /// </exception>
        void IncrementMeters(IEnumerable<string> meterPaths);

        /// <summary>
        /// Gets the single meter value out of the critical data.
        /// </summary>
        /// <param name="meterPath">
        /// The specified meter path to get. It must be a valid path for critical data.
        /// <list type="number">
        /// <item>
        /// The character set for the name is limited to a subset of
        /// ASCII characters that include numeric, alphabetic and the
        /// characters '/', '.', '_', and '-'.
        /// </item>
        /// <item>
        /// The name cannot start with '/'.
        /// </item>
        /// </list>
        /// </param>
        /// <returns>The value read from the critical data.</returns>
        /// <remarks>
        /// This method can be called only from within a method that provides an open transaction.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when there are illegal characters in the meter path which is being validated by
        /// <see cref="IGT.Game.Core.Communication.Foundation.Utility.ValidateCriticalDataName"/>.
        /// </exception>
        long GetMeter(string meterPath);

        /// <summary>
        /// Gets the values of multiple meters out of the critical data.
        /// </summary>
        /// <param name="meterPaths">
        /// The list of multiple meter paths to get. All meter paths must be valid paths for critical data.
        /// <list type="number">
        /// <item>
        /// The character set for the name is limited to a subset of
        /// ASCII characters that include numeric, alphabetic and the
        /// characters '/', '.', '_', and '-'.
        /// </item>
        /// <item>
        /// The name cannot start with '/'.
        /// </item>
        /// </list>
        /// </param>
        /// <returns>The list of values for multiple meters.</returns>
        /// <remarks>
        /// This method can be called only from within a method that provides an open transaction.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when there are illegal characters in the meter paths which are being validated by
        /// <see cref="IGT.Game.Core.Communication.Foundation.Utility.ValidateCriticalDataName"/>.
        /// </exception>
        IEnumerable<long> GetMeters(IEnumerable<string> meterPaths);
    }
}
