// -----------------------------------------------------------------------
// <copyright file = "LegacyGameMeterAccessor.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameMeter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// This class implements the <see cref="IGameMeterAccessor"/> that provides
    /// access to the meters in the critical data by using legacy F2L critical
    /// data interface.
    /// </summary>
    public class LegacyGameMeterAccessor : IGameMeterAccessor
    {
        #region Private Fields

        /// <summary>
        /// The game critical data interface.
        /// </summary>
        private readonly IGameCriticalData criticalDataInterface;

        /// <summary>
        /// The critical data scope for accessing through the game critical data interface.
        /// </summary>
        private readonly CriticalDataScope criticalDataScope;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="LegacyGameMeterAccessor"/> with the game critical
        /// data interface.
        /// </summary>
        /// <param name="criticalDataInterface">
        /// The game critical data interface.
        /// </param>
        /// <param name="criticalDataScope">
        /// The critical data scope for accessing through the game critical data interface.
        /// </param>
        /// <param name="writeEnabled">
        /// The flag indicating if the legacy game meter accessor is write-enabled or not.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="criticalDataInterface"/> is null.
        /// </exception>
        public LegacyGameMeterAccessor(IGameCriticalData criticalDataInterface,
                                       CriticalDataScope criticalDataScope,
                                       bool writeEnabled)
        {
            this.criticalDataInterface = criticalDataInterface ?? throw new ArgumentNullException(nameof(criticalDataInterface));
            this.criticalDataScope = criticalDataScope;

            WriteEnabled = writeEnabled;
        }

        #endregion

        #region IGameMeterAccessor Implementation

        /// <inheritdoc />
        public bool WriteEnabled { get; }

        /// <inheritdoc />
        public void SetMeter(string meterPath, long value)
        {
            // Make sure that the game meter accessor is write-enabled.
            MustBeWriteEnabled();

            // Write the value to the critical data.
            criticalDataInterface.WriteCriticalData(criticalDataScope, meterPath, value);
        }

        /// <inheritdoc />
        public void IncrementMeter(string meterPath, long value)
        {
            // Make sure that the game meter accessor is write-enabled.
            MustBeWriteEnabled();

            // Reads the current value of the meter.
            var curValue = criticalDataInterface.ReadCriticalData<long>(criticalDataScope, meterPath);

            // Increases by value and writes it back to critical data.
            criticalDataInterface.WriteCriticalData(criticalDataScope, meterPath, curValue + value);
        }

        /// <inheritdoc />
        public void IncrementMeter(string meterPath)
        {
            IncrementMeter(meterPath, 1);
        }

        /// <inheritdoc />
        public void IncrementMeters(IEnumerable<string> meterPaths)
        {
            meterPaths.ToList().ForEach(IncrementMeter);
        }

        /// <inheritdoc />
        public long GetMeter(string meterPath)
        {
            return criticalDataInterface.ReadCriticalData<long>(criticalDataScope, meterPath);
        }

        /// <inheritdoc />
        public IEnumerable<long> GetMeters(IEnumerable<string> meterPaths)
        {
            if(meterPaths == null)
            {
                return null;
            }
            var paths = meterPaths.ToList();
            if(!paths.Any())
            {
                return null;
            }

            var meters = new List<long>();
            // ReSharper disable once ConvertClosureToMethodGroup
            meters.AddRange(paths.Select(path => GetMeter(path)));

            return meters.Any() ? meters : null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Makes sure that the game meter accessor is write-enabled.
        /// </summary>
        private void MustBeWriteEnabled()
        {
            if(!WriteEnabled)
            {
                throw new InvalidOperationException(
                    "Operation is not supported since the game meter accessor is not write-enabled!");
            }
        }

        #endregion
    }
}
