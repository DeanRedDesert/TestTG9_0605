// -----------------------------------------------------------------------
// <copyright file = "GameMeterAccessor.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameMeter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// This class implements the <see cref="IGameMeterAccessor"/> that provides
    /// access to the meters in the critical data by using F2X critical data
    /// interface.
    /// </summary>
    public class GameMeterAccessor : IGameMeterAccessor
    {
        #region Private Fields

        /// <summary>
        /// The critical data accessor interface.
        /// </summary>
        private readonly ICriticalDataAccessor criticalDataAccessor;

        /// <summary>
        /// The critical data scope for accessing through the critical data accessor interface.
        /// </summary>
        private readonly CriticalDataScope criticalDataScope;

        /// <summary>
        /// The scope identifier to access the critical data through the critical data accessor interface.
        /// </summary>
        private readonly string scopeIdentifier;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="GameMeterAccessor"/> with the critical data accessor interface
        /// and the flag to indicate if it is enabled to write.
        /// </summary>
        /// <param name="criticalDataAccessor">
        /// The critical data accessor interface.
        /// </param>
        /// <param name="criticalDataScope">
        /// The critical data scope for accessing through the critical data accessor interface.
        /// </param>
        /// <param name="scopeIdentifier">
        /// The scope identifier to access the critical data through the critical data accessor interface.
        /// </param>
        /// <param name="writeEnabled">
        /// The flag indicating if the game meter accessor is write-enabled or not.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="criticalDataAccessor"/> is null, or <paramref name="scopeIdentifier"/>
        /// is null or empty.
        /// </exception>
        public GameMeterAccessor(ICriticalDataAccessor criticalDataAccessor,
                                 CriticalDataScope criticalDataScope,
                                 string scopeIdentifier,
                                 bool writeEnabled = false)
        {
            if(string.IsNullOrEmpty(scopeIdentifier))
            {
                throw new ArgumentNullException(nameof(scopeIdentifier));
            }

            this.criticalDataAccessor = criticalDataAccessor ?? throw new ArgumentNullException(nameof(criticalDataAccessor));
            this.criticalDataScope = criticalDataScope;
            this.scopeIdentifier = scopeIdentifier;

            WriteEnabled = writeEnabled;
        }

        #endregion

        #region IGameMeterAccessor Implementation

        /// <inheritdoc />
        public bool WriteEnabled { get; }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// Thrown when this interface gets called as the game meter accessor is readonly.
        /// </exception>
        public void SetMeter(string meterPath, long value)
        {
            // Make sure that the game meter accessor must be write-enabled.
            MustBeWriteEnabled();

            // Write the meter value to the critical data.
            criticalDataAccessor.WriteTransactionalCriticalData(
                new Dictionary<CriticalDataSelector, object>
                {
                    { new CriticalDataSelector(criticalDataScope, scopeIdentifier, meterPath), value }
                });
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// Thrown when this interface gets called as the game meter accessor is readonly.
        /// </exception>
        public void IncrementMeter(string meterPath, long value)
        {
            // Make sure that the game meter accessor must be write-enabled.
            MustBeWriteEnabled();

            // Read the meter value from the critical data.
            var selector = new CriticalDataSelector(criticalDataScope, scopeIdentifier, meterPath);
            var chunk = criticalDataAccessor.ReadTransactionalCriticalData(new List<CriticalDataSelector> { selector });
            var meterValue = chunk.RetrieveCriticalData<long>(selector);

            // Increment the value.
            meterValue += value;

            // Write the meter value to the critical data.
            criticalDataAccessor.WriteTransactionalCriticalData(
                new Dictionary<CriticalDataSelector, object> { { selector, meterValue } });
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// Thrown when this interface gets called as the game meter accessor is readonly.
        /// </exception>
        public void IncrementMeter(string meterPath)
        {
            IncrementMeter(meterPath, 1);
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// Thrown when this interface gets called as the game meter accessor is readonly.
        /// </exception>
        public void IncrementMeters(IEnumerable<string> meterPaths)
        {
            // Make sure that the game meter accessor must be write-enabled.
            MustBeWriteEnabled();

            var selectors = meterPaths.ToList()
                .ConvertAll(path => new CriticalDataSelector(criticalDataScope, scopeIdentifier, path));

            var chunk = criticalDataAccessor.ReadTransactionalCriticalData(selectors);
            var dataItems = selectors.ToDictionary(
                selector => selector,
                selector => chunk.RetrieveCriticalData<long>(selector) + 1 as object);
            criticalDataAccessor.WriteTransactionalCriticalData(dataItems);
        }

        /// <inheritdoc />
        public long GetMeter(string meterPath)
        {
            var selectors = new List<CriticalDataSelector>
                            {
                                new CriticalDataSelector(
                                        criticalDataScope,
                                        scopeIdentifier,
                                        meterPath),
                            };
            return criticalDataAccessor.ReadTransactionalCriticalData(selectors)
                                        .RetrieveCriticalData<long>(selectors[0]);
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
            // Read all the meter values from critical data at one time.
            var selectors = paths.Select(path => new CriticalDataSelector(
                                                            criticalDataScope,
                                                            scopeIdentifier,
                                                            path)).ToList();
            var chunk = criticalDataAccessor.ReadTransactionalCriticalData(selectors);
            selectors.ForEach(x => meters.Add(chunk.RetrieveCriticalData<long>(x)));

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
