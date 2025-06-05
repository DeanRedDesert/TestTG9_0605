// -----------------------------------------------------------------------
// <copyright file = "CriticalDataBlock.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces;

    /// <summary>
    /// Implementation of <see cref="ICriticalDataBlock"/> to provide the APIs for
    /// accessing one piece of critical data at a time.
    /// </summary>
    /// <remarks>
    /// It maintains a collection of multiple pieces of critical data in a key-value store
    /// to support accessing a specified piece of data as needed.
    /// </remarks>
    public class CriticalDataBlock : CriticalDataBlockBase, ICriticalDataBlock
    {
        #region Private Fields

        /// <summary>
        /// The table of critical data items with serialized data values.
        /// </summary>
        private readonly IDictionary<CriticalDataName, byte[]> keyValueStoreItems;

        /// <summary>
        /// The table of representative strings of critical data items.
        /// </summary>
        private Dictionary<CriticalDataName, string> itemRepStrings;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates an empty instance of <see cref="CriticalDataBlock"/>.
        /// </summary>
        /// <param name="isDataCompressed">
        /// The flag indicating whether it is compressed critical data block.
        /// This parameter is optional.  If not specified, it defaults to false.
        /// </param>
        public CriticalDataBlock(bool isDataCompressed = false) : base(isDataCompressed)
        {
            keyValueStoreItems = new Dictionary<CriticalDataName, byte[]>();
        }

        /// <summary>
        /// Instantiates an instance of <see cref="CriticalDataBlock"/> that has an entry for each name in
        /// <paramref name="criticalDataNames"/>, and each entry has null data bytes.
        /// </summary>
        /// <param name="criticalDataNames">
        /// A list of critical data names to create entries for.
        /// </param>
        /// <param name="isDataCompressed">
        /// The flag indicating whether it is compressed critical data block. By default, if not specified, will be false
        /// which means it is not compressed critical data block.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="criticalDataNames"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="criticalDataNames"/> contains invalid critical data names.
        /// </exception>
        public CriticalDataBlock(IEnumerable<string> criticalDataNames, bool isDataCompressed = false) : base(isDataCompressed)
        {
            if(criticalDataNames == null)
            {
                throw new ArgumentNullException(nameof(criticalDataNames));
            }

            keyValueStoreItems = criticalDataNames.ToDictionary(name => new CriticalDataName(name), name => (byte[])null);
        }


        /// <summary>
        /// Instantiates an instance of <see cref="CriticalDataBlock"/> that has an entry for each name in
        /// <paramref name="criticalDataNames"/>, and each entry has null data bytes.
        /// </summary>
        /// <param name="criticalDataNames">
        /// A list of critical data names to create entries for.
        /// </param>
        /// <param name="isDataCompressed">
        /// The flag indicating whether it is compressed critical data block. By default, if not specified, will be false
        /// which means it is not compressed critical data block.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="criticalDataNames"/> is null.
        /// </exception>
        public CriticalDataBlock(IEnumerable<CriticalDataName> criticalDataNames, bool isDataCompressed = false) : base(isDataCompressed)
        {
            if(criticalDataNames == null)
            {
                throw new ArgumentNullException(nameof(criticalDataNames));
            }

            keyValueStoreItems = criticalDataNames.ToDictionary(name => name, name => (byte[])null);
        }

        /// <summary>
        /// Instantiates an instance of <see cref="CriticalDataBlock"/> by the given critical data items.
        /// </summary>
        /// <param name="keyValueStoreItems">
        /// The critical data items.
        /// </param>
        /// <param name="isDataCompressed">
        /// The flag indicating whether it is compressed critical data block. By default, if not specified, will be false
        /// which means it is not compressed critical data block.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="keyValueStoreItems"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="keyValueStoreItems"/> contains invalid critical data names.
        /// </exception>
        public CriticalDataBlock(IDictionary<string, byte[]> keyValueStoreItems, bool isDataCompressed = false) : base(isDataCompressed)
        {
            if(keyValueStoreItems == null)
            {
                throw new ArgumentNullException(nameof(keyValueStoreItems));
            }

            this.keyValueStoreItems = keyValueStoreItems.ToDictionary(entry => new CriticalDataName(entry.Key), entry => entry.Value);
        }

        /// <summary>
        /// Instantiates an instance of <see cref="CriticalDataBlock"/> by the given critical data items.
        /// </summary>
        /// <param name="keyValueStoreItems">
        /// The critical data items.
        /// </param>
        /// <param name="isDataCompressed">
        /// The flag indicating whether it is compressed critical data block. By default, if not specified, will be false
        /// which means it is not compressed critical data block.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="keyValueStoreItems"/> is null.
        /// </exception>
        public CriticalDataBlock(IDictionary<CriticalDataName, byte[]> keyValueStoreItems, bool isDataCompressed = false) : base(isDataCompressed)
        {
            this.keyValueStoreItems = keyValueStoreItems ?? throw new ArgumentNullException(nameof(keyValueStoreItems));
        }

        #endregion

        #region Internal Methods

        /// <inheritdoc />
        internal override string GetRepString(CriticalDataName name)
        {
            return itemRepStrings?.ContainsKey(name) == true
                       ? itemRepStrings[name]
                       : null;
        }

        #endregion

        #region ICriticalDataBlock Members

        /// <inheritdoc />
        public virtual T GetCriticalData<T>(CriticalDataName name)
        {
            if(!keyValueStoreItems.TryGetValue(name, out var value))
            {
                throw new ArgumentException(
                    $"There is no critical data by the name of {name} present in the critical data block.");
            }

            var result = DeserializeCriticalData<T>(value);

            // In case the data was only set as serialized data before.
            SaveRepString(name, result, false);

            return result;
        }

        /// <inheritdoc />
        public virtual byte[] GetSerializedData(CriticalDataName name)
        {
            if(!keyValueStoreItems.TryGetValue(name, out var value))
            {
                throw new ArgumentException(
                    $"There is no critical data by the name of {name} present in the critical data block.");
            }

            return value;
        }

        /// <inheritdoc />
        public virtual void SetCriticalData<T>(CriticalDataName name, T data)
        {
            keyValueStoreItems[name] = SerializeCriticalData(data);

            SaveRepString(name, data, true);
        }

        /// <inheritdoc />
        public virtual void SetSerializedData(CriticalDataName name, byte[] data)
        {
            keyValueStoreItems[name] = data;

            // We can't update rep string based on serialized data.
            // Remove it for now.  It will be added when the data is read.
            DeleteRepString(name);
        }

        /// <inheritdoc />
        public virtual void DeleteCriticalData(CriticalDataName name)
        {
            if(!keyValueStoreItems.ContainsKey(name))
            {
                throw new ArgumentException(
                    $"There is no critical data by the name of {name} present in the critical data block.");
            }

            keyValueStoreItems.Remove(name);

            DeleteRepString(name);
        }

        /// <inheritdoc />
        public virtual bool Contains(CriticalDataName name)
        {
            return keyValueStoreItems.ContainsKey(name);
        }

        /// <inheritdoc />
        public virtual IList<CriticalDataName> GetNameList()
        {
            return keyValueStoreItems.Keys.ToList();
        }

        /// <inheritdoc />
        public bool HasData => keyValueStoreItems.Any();

        #endregion

        #region Private Methods

        private void SaveRepString(CriticalDataName name, object data, bool forceWrite)
        {
            if(!PlatformSettings.SafeStorageRepStringsEnabled)
            {
                return;
            }

            if(itemRepStrings == null)
            {
                itemRepStrings = new Dictionary<CriticalDataName, string>();
            }

            if(!itemRepStrings.ContainsKey(name) || forceWrite)
            {
                itemRepStrings[name] = data?.ToString();
            }
        }

        private void DeleteRepString(CriticalDataName name)
        {
            if(!PlatformSettings.SafeStorageRepStringsEnabled)
            {
                return;
            }

            itemRepStrings?.Remove(name);
        }

        #endregion
    }
}