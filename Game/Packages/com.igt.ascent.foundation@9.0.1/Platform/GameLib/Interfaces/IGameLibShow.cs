//-----------------------------------------------------------------------
// <copyright file = "IGameLibShow.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;

    /// <summary>
    /// Interface for show functionality in the GameLib.
    /// </summary>
    public interface IGameLibShow
    {
        /// <summary>
        /// Get if the GameLib is currently in show mode.
        /// If the GameLib is not in show mode, then the methods in this interface should not be called.
        /// </summary>
        bool ShowMode { get; }

        /// <summary>
        /// Add money amount to Player Bank Meter. Only valid when in show mode.
        /// </summary>
        /// <param name="value">Amount to add, in units of the denomination passed in.</param>
        /// <param name="denomination">The denomination for the value to add.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when value is less than 0, or denomination is less than 1.
        /// </exception>
        /// <exeption cref="ShowFunctionException">
        /// Thrown when the function is called while the GameLib is not in show mode.
        /// </exeption>
        void InsertMoney(long value, long denomination);

        /// <summary>
        /// Returns the Show Environment.
        /// </summary>
        /// <returns>The current show environment of the Foundation. Either development, show or invalid.</returns>
        ShowEnvironment GetShowEnvironment();
    }
}

