// -----------------------------------------------------------------------
// <copyright file = "CriticalDataAccessor.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Implementation of <see cref="ICriticalDataAccessor"/>.
    /// </summary>
    /// <remarks>
    /// Both non-transactional reading critical data and transactional reading and writing critical data for standalone
    /// are not supported yet at present.
    /// </remarks>
    internal class CriticalDataAccessor : ICriticalDataAccessor
    {
        #region Private Fields

        /// <summary>
        /// The object used to read configuration data from the safe storage.
        /// </summary>
        private readonly IDiskStoreManager diskStoreManager;

        /// <summary>
        /// The flag that indicates whether is reading compressed critical data.
        /// </summary>
        private readonly bool isDataCompressed;

        /// <summary>
        /// The interface used for validating the access to critical data.
        /// </summary>
        private readonly ICriticalDataAccessValidation criticalDataAccessValidation;

        /// <summary>
        /// The object used to find the location in disk store for reading and writing data.
        /// </summary>
        private readonly DiskStoreSectionIndexer diskStoreSectionIndexer;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="CriticalDataAccessor"/>.
        /// </summary>
        /// <param name="diskStoreManager">
        /// The object managing the standalone safe storage.
        /// </param>
        /// <param name="isDataCompressed">
        /// The flag that indicates whether the data is compressed.
        /// </param>
        /// <param name="diskStoreSectionIndexer">
        /// The object used to find where in the disk store to read and write data.
        /// </param>
        /// <param name="criticalDataAccessValidation">
        /// The interface used for validating the access to critical data.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the reference arguments is null.
        /// </exception>
        public CriticalDataAccessor(IDiskStoreManager diskStoreManager,
                                    bool isDataCompressed,
                                    DiskStoreSectionIndexer diskStoreSectionIndexer,
                                    ICriticalDataAccessValidation criticalDataAccessValidation)
        {
            this.diskStoreManager = diskStoreManager ?? throw new ArgumentNullException(nameof(diskStoreManager));
            this.isDataCompressed = isDataCompressed;
            this.diskStoreSectionIndexer = diskStoreSectionIndexer ?? throw new ArgumentNullException(nameof(diskStoreSectionIndexer));
            this.criticalDataAccessValidation = criticalDataAccessValidation ?? throw new ArgumentNullException(nameof(criticalDataAccessValidation));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Reads critical data from disk store manager.
        /// </summary>
        /// <param name="criticalDataSelectors">
        /// A set of scope identifiers and paths for reading critical data.
        /// </param>
        /// <returns>The critical data chunk instance.</returns>
        private ICriticalDataChunk ReadCriticalDataChunk(IList<CriticalDataSelector> criticalDataSelectors)
        {
            if(criticalDataSelectors == null)
            {
                throw new ArgumentNullException(nameof(criticalDataSelectors));
            }

            if(!criticalDataSelectors.Any())
            {
                throw new ArgumentException("Critical data selectors should not be empty.");
            }

            criticalDataAccessValidation.ValidateCriticalDataAccess(criticalDataSelectors, DataAccessing.Read);

            var criticalData = criticalDataSelectors.ToDictionary(
                selector => selector,
                selector =>
                    {
                        var (section, index) = diskStoreSectionIndexer.GetCriticalDataLocation(selector.Scope, selector.ScopeIdentifier);
                        return diskStoreManager.ReadRaw(section, index, selector.Path);
                    });

            return new CriticalDataChunk(criticalData, isDataCompressed);
        }

        #endregion

        #region ICriticalDataAccessor Implementation

        /// <inheritdoc />
        public ICriticalDataChunk ReadNonTransactionalCriticalData(IList<CriticalDataSelector> criticalDataSelectors)
        {
            return ReadCriticalDataChunk(criticalDataSelectors);
        }

        /// <inheritdoc />
        public ICriticalDataChunk ReadTransactionalCriticalData(IList<CriticalDataSelector> criticalDataSelectors)
        {
            return ReadCriticalDataChunk(criticalDataSelectors);
        }

        /// <inheritdoc />
        public void WriteTransactionalCriticalData(IDictionary<CriticalDataSelector, object> criticalDataItems)
        {
            if(criticalDataItems == null)
            {
                throw new ArgumentNullException(nameof(criticalDataItems));
            }

            if(!criticalDataItems.Any())
            {
                throw new ArgumentException("Critical data objects should not be empty.", nameof(criticalDataItems));
            }

            criticalDataAccessValidation.ValidateCriticalDataAccess(criticalDataItems.Keys.ToList(), 
                                                                    DataAccessing.Write);
            // Do nothing for standalone at this moment.
        }

        /// <inheritdoc />
        public void RemoveTransactionalCriticalData(IList<CriticalDataSelector> criticalDataSelectors)
        {
            if(criticalDataSelectors == null)
            {
                throw new ArgumentNullException(nameof(criticalDataSelectors));
            }

            if(!criticalDataSelectors.Any())
            {
                throw new ArgumentException("Selectors should not be empty.", nameof(criticalDataSelectors));
            }

            criticalDataAccessValidation.ValidateCriticalDataAccess(criticalDataSelectors, DataAccessing.Remove);
            // Do nothing for standalone at this moment.
        }

        #endregion

    }
}