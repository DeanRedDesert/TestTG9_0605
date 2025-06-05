// -----------------------------------------------------------------------
// <copyright file = "CriticalDataStoreBase.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Game.Core.Communication.Foundation.F2X;
    using Game.Core.Communication.Foundation.F2X.Schemas.Internal.KVSTypes;
    using Interfaces;

    /// <summary>
    /// This generic base class encapsulates the basic operations on accessing the key-value store.
    /// It is for purpose of creating the concrete class for accessing the specific key-value store,
    /// such as ShellStore, ThemeStore, PayvarStore and so on.
    /// </summary>
    /// <typeparam name="TCategory">
    /// The generic interface type of key-value store category.
    /// </typeparam>
    internal abstract class CriticalDataStoreBase<TCategory> : ICriticalDataStore where TCategory : IKeyValueStoreCategory
    {
        #region Private Fields

        /// <summary>
        /// The cached interface of the key-value store category.
        /// </summary>
        private readonly CategoryInitializer<IKeyValueStoreCategory> keyValueStoreCategory;

        /// <summary>
        /// The interface used for validating the accessing to key-value store of critical data.
        /// </summary>
        private readonly ICriticalDataStoreAccessValidator storeAccessValidator;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor of the generic base class.
        /// </summary>
        /// <param name="storeAccessValidator">
        /// The interface used for validating the accessing to key-value store of critical data.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="storeAccessValidator"/> is null.
        /// </exception>
        protected CriticalDataStoreBase(ICriticalDataStoreAccessValidator storeAccessValidator)
        {
            this.storeAccessValidator = storeAccessValidator ?? throw new ArgumentNullException(nameof(storeAccessValidator));

            keyValueStoreCategory = new CategoryInitializer<IKeyValueStoreCategory>();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Initializes the critical data store.
        /// </summary>
        /// <param name="category">
        /// The interface of the key-value store category.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="category"/> is null.
        /// </exception>
        public void Initialize(TCategory category)
        {
            if(category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            keyValueStoreCategory.Initialize(category);
        }

        #endregion

        #region ICriticalDataStore Implementation

        /// <inheritdoc/>
        public ICriticalDataBlock Read(IList<CriticalDataName> nameList)
        {
            if(nameList?.Any() != true)
            {
                throw new ArgumentException("The name list can not be null or empty.");
            }

            storeAccessValidator.Validate(DataAccess.Read, StoreName);

            return DoRead(nameList);
        }

        /// <inheritdoc />
        public ICriticalDataBlock Read(CriticalDataName name)
        {
            return Read(new List<CriticalDataName> { name });
        }

        /// <inheritdoc />
        public void Fill(ICriticalDataBlock criticalDataBlock)
        {
            if(criticalDataBlock == null)
            {
                throw new ArgumentNullException(nameof(criticalDataBlock));
            }

            var nameList = criticalDataBlock.GetNameList();

            if(nameList?.Any() != true)
            {
                throw new ArgumentException("The name list specified by the given critical data block can not be null or empty.");
            }

            storeAccessValidator.Validate(DataAccess.Read, StoreName);

            DoFill(nameList, criticalDataBlock);
        }

        /// <inheritdoc/>
        public void Remove(IList<CriticalDataName> nameList)
        {
            if(nameList?.Any() != true)
            {
                throw new ArgumentException("The name list can not be null or empty.");
            }

            storeAccessValidator.Validate(DataAccess.Remove, StoreName);

            DoRemove(nameList);
        }

        /// <inheritdoc />
        public void Remove(CriticalDataName name)
        {
            Remove(new List<CriticalDataName> { name });
        }

        /// <inheritdoc/>
        public void Write(ICriticalDataBlock criticalDataBlock)
        {
            if(criticalDataBlock == null)
            {
                throw new ArgumentNullException(nameof(criticalDataBlock));
            }

            var nameList = criticalDataBlock.GetNameList();
            if(nameList?.Any() != true)
            {
                throw new ArgumentException("Invalid critical data block: The name list can not be empty.");
            }

            storeAccessValidator.Validate(DataAccess.Write, StoreName);

            DoWrite(criticalDataBlock);
        }

        #endregion

        #region Abstract Properties

        /// <summary>
        /// Gets the name of the critical data store.
        /// </summary>
        protected abstract string StoreName { get; }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Executes the action of reading.
        /// </summary>
        /// <param name="nameList">
        /// List of names of the critical data to read from the store.
        /// </param>
        /// <returns>
        /// A critical data block which contains all the critical data having been read.
        /// </returns>
        protected virtual ICriticalDataBlock DoRead(IList<CriticalDataName> nameList)
        {
            return ReadFromFoundation(nameList);
        }

        /// <summary>
        /// Executes the action of filling.
        /// </summary>
        /// <param name="nameList">
        /// List of names of the critical data to read from the store.
        /// </param>
        /// <param name="criticalDataBlock">
        /// The critical data block to hold the reading result.
        /// </param>
        protected virtual void DoFill(IList<CriticalDataName> nameList,
                                      ICriticalDataBlock criticalDataBlock)
        {
            FillFromFoundation(nameList, criticalDataBlock);
        }

        /// <summary>
        /// Executes the action of removing.
        /// </summary>
        /// <param name="nameList">
        /// List of names of the critical data to remove from the store.
        /// </param>
        protected virtual void DoRemove(IList<CriticalDataName> nameList)
        {
            RemoveFromFoundation(nameList);
        }

        /// <summary>
        /// Executes the action of writing.
        /// </summary>
        /// <param name="criticalDataBlock">
        /// A critical data block which contains all the critical data having been read.
        /// </param>
        protected virtual void DoWrite(ICriticalDataBlock criticalDataBlock)
        {
            WriteToFoundation(criticalDataBlock);
        }

        /// <summary>
        /// Communicates with Foundation to read.
        /// </summary>
        /// <param name="nameList">
        /// List of names of the critical data to read from the store.
        /// </param>
        /// <returns>
        /// A critical data block which contains all the critical data having been read.
        /// </returns>
        protected ICriticalDataBlock ReadFromFoundation(IList<CriticalDataName> nameList)
        {
            var names = nameList.Select(name => (string)name);
            var contentItems = keyValueStoreCategory.Instance.Read(names);

            return new CriticalDataBlock(contentItems.ToDictionary(contentItem => contentItem.Key,
                                                                   contentItem => contentItem.Value));
        }

        /// <summary>
        /// Communicates with Foundation to read and fill a critical data block.
        /// </summary>
        /// <param name="nameList">
        /// List of names of the critical data to read from the store.
        /// </param>
        /// <param name="criticalDataBlock">
        /// The critical data block to hold the reading result.
        /// </param>
        protected void FillFromFoundation(IList<CriticalDataName> nameList,
                                          ICriticalDataBlock criticalDataBlock)
        {
            var names = nameList.Select(name => (string)name);
            var contentItems = keyValueStoreCategory.Instance.Read(names);

            foreach(var contentItem in contentItems)
            {
                criticalDataBlock.SetSerializedData(contentItem.Key, contentItem.Value);
            }
        }

        /// <summary>
        /// Communicates with Foundation to remove.
        /// </summary>
        /// <param name="nameList">
        /// List of names of the critical data to remove from the store.
        /// </param>
        protected void RemoveFromFoundation(IList<CriticalDataName> nameList)
        {
            var names = nameList.Select(name => (string)name);
            keyValueStoreCategory.Instance.Remove(names);
        }

        /// <summary>
        /// Communicates with Foundation to write.
        /// </summary>
        /// <param name="criticalDataBlock">
        /// A critical data block which contains all the critical data having been read.
        /// </param>
        protected void WriteToFoundation(ICriticalDataBlock criticalDataBlock)
        {
            // nameList has been validated so far.
            var nameList = criticalDataBlock.GetNameList();

            var itemsToWrite = nameList.Select(name => new WriteSendItem
                                                           {
                                                               Key = name,
                                                               Value = criticalDataBlock.GetSerializedData(name)
                                                           });
            keyValueStoreCategory.Instance.Write(itemsToWrite);
        }

        #endregion
    }
}
