// -----------------------------------------------------------------------
// <copyright file = "CriticalDataAccessor.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using Ascent.Communication.Platform.Interfaces;
    using CompactSerialization;
    using F2X;
    using F2X.Schemas.Internal.TransactionalCritDataWrite;
    using F2XCallbacks;

    /// <summary>
    /// Implementation of <see cref="ICriticalDataAccessor"/> that
    /// provides access to critical data maintained by the Foundation.
    /// </summary>
    internal class CriticalDataAccessor : ICriticalDataAccessor
    {
        #region Private Fields

        /// <summary>
        /// The category used for reading critical data from the Foundation in a
        /// non-transactional manner.
        /// </summary>
        private INonTransactionalCritDataReadCategory nonTransactionalCritDataReadCategory;

        /// <summary>
        /// The category used for reading critical data from the Foundation in a
        /// transactional manner.
        /// </summary>
        private ITransactionalCritDataReadCategory transactionalCritDataReadCategory;

        /// <summary>
        /// The category used for writing critical data to the Foundation in a
        /// transactional manner.
        /// </summary>
        private ITransactionalCritDataWriteCategory transactionalCritDataWriteCategory;

        /// <summary>
        /// The interface used for validating the accessings to critical data.
        /// </summary>
        private readonly ICriticalDataAccessValidation criticalDataAccessValidation;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="CriticalDataAccessor"/> for accessing to
        /// the critical data through F2X category.
        /// </summary>
        /// <param name="criticalDataAccessValidation">
        /// The interface used for validating the accessings to critical data.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="criticalDataAccessValidation"/> is null.
        /// </exception>
        public CriticalDataAccessor(ICriticalDataAccessValidation criticalDataAccessValidation)
        {
            if(criticalDataAccessValidation == null)
            {
                throw new ArgumentNullException("criticalDataAccessValidation");
            }

            this.criticalDataAccessValidation = criticalDataAccessValidation;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes class members whose values become available after construction,
        /// e.g. when a connection is established with the Foundation.
        /// </summary>
        /// <param name="nonTransactionalCriticalDataReadHandler">
        /// The interface of category for reading critical data non-transactionally.
        /// </param>
        /// <param name="transactionalCriticalDataReadHandler">
        /// The interface of category for reading critical data transactionally.
        /// </param>
        /// <param name="transactionalCritDataWriteHandler">
        /// The interface of category for writing critical data transactionally.
        /// </param>
        public void Initialize(INonTransactionalCritDataReadCategory nonTransactionalCriticalDataReadHandler,
                               ITransactionalCritDataReadCategory transactionalCriticalDataReadHandler,
                               ITransactionalCritDataWriteCategory transactionalCritDataWriteHandler)
        {
            nonTransactionalCritDataReadCategory = nonTransactionalCriticalDataReadHandler;
            transactionalCritDataReadCategory = transactionalCriticalDataReadHandler;
            transactionalCritDataWriteCategory = transactionalCritDataWriteHandler;
        }

        #endregion

        #region ICriticalDataAccessor Implementation

        /// <inheritdoc/>
        public ICriticalDataChunk ReadNonTransactionalCriticalData(IList<CriticalDataSelector> criticalDataSelectors)
        {
            if(criticalDataSelectors == null)
            {
                throw new ArgumentNullException("criticalDataSelectors");
            }

            if(!criticalDataSelectors.Any())
            {
                throw new ArgumentException("Selectors should not be empty.");
            }

            CheckNonTransactionalReadInitialization();
            criticalDataAccessValidation.ValidateCriticalDataAccess(criticalDataSelectors, DataAccessing.Read);

            var criticalDataItemSelectors =
                criticalDataSelectors.Select(
                    criticalDataSelector => criticalDataSelector.ToNonTransactionalCriticalDataItemSelector());

            // Request for critical data.
            var readData = nonTransactionalCritDataReadCategory.ReadCritData(criticalDataItemSelectors);

            var criticalData = readData.ToDictionary(
                data => data.CriticalDataItemSelector.ToCriticalDataSelector(),
                data => data.Data);

            return new CriticalDataChunk(criticalData);
        }

        /// <inheritdoc/>
        public ICriticalDataChunk ReadTransactionalCriticalData(IList<CriticalDataSelector> criticalDataSelectors)
        {
            if(criticalDataSelectors == null)
            {
                throw new ArgumentNullException("criticalDataSelectors");
            }

            if(!criticalDataSelectors.Any())
            {
                throw new ArgumentException("Selectors should not be empty.");
            }

            CheckTransactionalReadInitialization();
            criticalDataAccessValidation.ValidateCriticalDataAccess(criticalDataSelectors, DataAccessing.Read);

            var criticalDataItemSelectors =
                criticalDataSelectors.Select(selector => selector.ToTransactionalCriticalDataReadItemSelector());

            // Request for critical data.
            var readData = transactionalCritDataReadCategory.ReadCritData(criticalDataItemSelectors);

            var criticalData = readData.ToDictionary(
                data => data.CriticalDataItemSelector.ToCriticalDataSelector(),
                data => data.Data);

            return new CriticalDataChunk(criticalData);
        }

        /// <inheritdoc/>
        public void RemoveTransactionalCriticalData(IList<CriticalDataSelector> criticalDataSelectors)
        {
            if(criticalDataSelectors == null)
            {
                throw new ArgumentNullException("criticalDataSelectors");
            }

            if(!criticalDataSelectors.Any())
            {
                throw new ArgumentException("Selectors should not be empty.");
            }

            CheckTransactionalWriteInitialization();
            criticalDataAccessValidation.ValidateCriticalDataAccess(criticalDataSelectors, DataAccessing.Remove);

            var criticalDataItemSelectors =
                criticalDataSelectors.Select(selector => selector.ToTransactionalCriticalDataWriteItemSelector());

            transactionalCritDataWriteCategory.RemoveCritData(criticalDataItemSelectors);
        }

        /// <inheritdoc/>
        public void WriteTransactionalCriticalData(IDictionary<CriticalDataSelector, object> criticalDataObjects)
        {
            if(criticalDataObjects == null)
            {
                throw new ArgumentNullException("criticalDataObjects");
            }

            if(!criticalDataObjects.Any())
            {
                throw new ArgumentException("Critical data objects should not be empty.");
            }

            CheckTransactionalWriteInitialization();
            criticalDataAccessValidation.ValidateCriticalDataAccess(criticalDataObjects.Keys.ToList(), DataAccessing.Write);

            var criticalDataItems = new List<CriticalDataItem>();
            foreach(var objectItem in criticalDataObjects)
            {
                using(var memoryStream = new MemoryStream())
                {
                    if(CompactSerializer.Supports(objectItem.Value.GetType()))
                    {
                        CompactSerializer.Serialize(memoryStream, objectItem.Value);
                    }
                    else
                    {
                        try
                        {
                            var formatter = new BinaryFormatter();
                            formatter.Serialize(memoryStream, objectItem.Value);
                        }
                        catch(SerializationException serializationException)
                        {
                            throw new InvalidSafeStorageTypeException(
                                "Type must be serializable to be written to critical data.", serializationException);
                        }
                    }

                    criticalDataItems.Add(
                        new CriticalDataItem
                        {
                            CriticalDataItemSelector = objectItem.Key.ToTransactionalCriticalDataWriteItemSelector(),
                            Data = memoryStream.ToArray(),
                        });
                }
            }
            transactionalCritDataWriteCategory.WriteCritData(
                new WriteCritDataSendContent
                {
                    CriticalDataItems = criticalDataItems
                });
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks if the handler for reading critical data non-transactionally has been
        /// initialized correctly before being used.
        /// </summary>
        /// <exception cref="CommunicationInterfaceUninitializedException">
        /// Thrown when reading critical data non-transactionally is not supported.
        /// </exception>
        private void CheckNonTransactionalReadInitialization()
        {
            if(nonTransactionalCritDataReadCategory == null)
            {
                throw new CommunicationInterfaceUninitializedException(
                    "Reading critical data non-transactionally is not supported on the" +
                    " Foundation target specified by the build parameters.");
            }
        }

        /// <summary>
        /// Checks if the handler for reading critical data transactionally has been
        /// initialized correctly before being used.
        /// </summary>
        /// <exception cref="CommunicationInterfaceUninitializedException">
        /// Thrown when reading critical data transactionally is not supported.
        /// </exception>
        private void CheckTransactionalReadInitialization()
        {
            if(transactionalCritDataReadCategory == null)
            {
                throw new CommunicationInterfaceUninitializedException(
                    "Reading critical data transactionally is not supported on the" +
                    " Foundation target specified by the build parameters.");
            }
        }

        /// <summary>
        /// Checks if the handler for writing critical data transactionally has been
        /// initialized correctly before being used.
        /// </summary>
        /// <exception cref="CommunicationInterfaceUninitializedException">
        /// Thrown when writing critical data transactionally is not supported.
        /// </exception>
        private void CheckTransactionalWriteInitialization()
        {
            if(transactionalCritDataWriteCategory == null)
            {
                throw new CommunicationInterfaceUninitializedException(
                    "Writing critical data transactionally is not supported on the" +
                    " Foundation target specified by the build parameters.");
            }
        }

        #endregion
    }
}