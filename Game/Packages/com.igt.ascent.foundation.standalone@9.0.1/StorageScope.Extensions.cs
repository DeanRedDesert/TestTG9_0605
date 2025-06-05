//-----------------------------------------------------------------------
// <copyright file = "StorageScope.Extensions.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;
    using CompactSerialization;

    partial class StorageScope : ICompactSerializable
    {
        #region Fields

        /// <summary>
        /// The current size of the scope. The value is modified as data is added/removed/modified.
        /// The initial value may be calculated based on the existing content of the scope.
        /// </summary>
        /// <remarks>
        /// The size reflected will not be correct if modifications to the scope are made by directly modifying its
        /// contents instead of using the provided methods.
        /// </remarks>
        [XmlIgnore]
        private int size;

        #endregion

        #region Public Properties

        /// <summary>
        /// Get the current size of the scope.
        /// </summary>
        /// <remarks>
        /// Modify the size prior to making changes to the collection. This will ensure the end size is correct if the
        /// current size had not yet been calculated. If the collection is changed, then the size is modified, then
        /// the calculated size would be based on the already modified collection. This is to allow running changes to
        /// the size without having to recalculate.
        /// </remarks>
        [XmlIgnore]
        public int Size
        {
            get
            {
                if(size == 0)
                {
                    CalculateSize();
                }
                return size;
            }

            private set => size = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Remove all the critical data entries from this scope.
        /// </summary>
        public void Clear()
        {
            StorageList?.Clear();
            Size = 0;
        }

        /// <summary>
        /// Check if an item exists in this scope.
        /// </summary>
        /// <param name="path">The path to the item.</param>
        /// <returns>True if the item exists.</returns>
        public bool Contains(string path)
        {
            var contains = false;

            if(StorageList != null)
            {
                var item = (from x in StorageList where x.DataPath == path select x).FirstOrDefault();
                contains = item != null;
            }

            return contains;
        }

        /// <summary>
        /// Read data from the specified path.
        /// </summary>
        /// <param name="path">The path to read the data from.</param>
        /// <returns>
        /// Byte array containing the read data. If the data did not exist, then
        /// null will be returned.
        /// </returns>
        public byte[] ReadData(string path)
        {
            byte[] returnData = null;

            if(StorageList != null)
            {
                returnData =
                    (from storageData in StorageList where storageData.DataPath == path select storageData.Data).
                        FirstOrDefault();
            }

            return returnData;
        }

        /// <summary>
        /// Write data to the specified path.
        /// </summary>
        /// <param name="path">The path to write the data to.</param>
        /// <param name="data">The data to write to the path.</param>
        public void WriteData(string path, byte[] data)
        {
            if(StorageList == null)
            {
                StorageList = new List<StorageData>();
            }

            AddData(path, data);
        }

        /// <summary>
        /// Remove the data from the specified path.
        /// </summary>
        /// <param name="path">The path to remove the data from.</param>
        /// <returns>True if the data was removed.</returns>
        public bool RemoveData(string path)
        {
            if(StorageList != null)
            {
                return DeleteData(path);
            }

            return false;
        }

        /// <summary>
        /// Make a deep copy of the contents of the given scope and discard the current contents of this scope.
        /// </summary>
        /// <param name="sourceScope">The source scope to clone.</param>
        public void CloneScope(StorageScope sourceScope)
        {
            StorageList = new List<StorageData>();

            if(sourceScope.StorageList != null)
            {
                foreach(var storageData in sourceScope.StorageList)
                {
                    var dataCopy = new StorageData {DataPath = storageData.DataPath};
                    AssignData(dataCopy, storageData.Data);

                    StorageList.Add(dataCopy);
                }
            }

            CalculateSize();
        }

        /// <summary>
        /// Assign the storage list for this scope.
        /// </summary>
        /// <param name="storageList"></param>
        /// <remarks>This function should be used instead of the property in the schema generated class.</remarks>
        public void SetStorageList(List<StorageData> storageList)
        {
            StorageList = storageList;
            CalculateSize();
        }

        /// <summary>
        /// Get a list of the paths contained in this scope.
        /// </summary>
        /// <returns>List of the paths of items in this scope.</returns>
        public ICollection<string> GetManifest()
        {
            var manifest = new List<string>();

            if(StorageList != null)
            {
                manifest = StorageList.Select(storageData => storageData.DataPath).ToList();
            }

            return manifest;
        }

        /// <summary>
        /// Check if the specified path is a directory.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>True if the path is a directory.</returns>
        public bool IsDirectory(string path)
        {
            //Format the path as a directory to perform the check.
            if(!path.EndsWith("/"))
            {
                path = path + "/";
            }

            return StorageList?.Any(storageData => storageData.DataPath.StartsWith(path)) == true;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Add the given data with specified path to the storage list.
        /// If data already exists at the path it will be updated, otherwise
        /// a new entry for the data will be created.
        /// </summary>
        /// <param name="path">Unique identifier to assign the data within the list.</param>
        /// <param name="data">The data to add to the list.</param>
        private void AddData(string path, byte[] data)
        {
            //If the path already exists, then alter the data instead
            //of adding it.
            if(!UpdateData(path, data))
            {
                var storageData = new StorageData { DataPath = path };
                AssignData(storageData, data);
                StorageList.Add(storageData);
            }
        }


        /// <summary>
        /// Assign data to the Data member of a StorageData object.
        /// This method is used so that a copy of the data is made
        /// and no reference to the original byte array is maintained.
        /// </summary>
        /// <param name="storageData">The StorageData object to add the data to.</param>
        /// <param name="data">The data to add to the StorageData object.</param>
        private void AssignData(StorageData storageData, byte[] data)
        {
            //If there is existing data, then remove its size from the running size.
            if(storageData.Data != null)
            {
                Size -= storageData.Data.Length;
            }

            Size += data.Length;
            storageData.Data = new byte[data.Length];
            
            //Being as the bytes are value types we can
            //use this to make a deep copy of the byte array.
            data.CopyTo(storageData.Data, 0);
        }

        /// <summary>
        /// Update data within the storage list if it exists.
        /// </summary>
        /// <param name="path">Path to the data.</param>
        /// <param name="data">The data to update.</param>
        /// <returns>False if the data was not present.</returns>
        private bool UpdateData(string path, byte[] data)
        {
            var updated = false;
            foreach(var storageData in StorageList)
            {
                if(storageData.DataPath == path)
                {
                    AssignData(storageData, data);
                    updated = true;

                    //Don't look at further paths as there should only be 1.
                    break;
                }
            }

            return updated;
        }

        /// <summary>
        /// Remove the data from the storage list.
        /// </summary>
        /// <param name="path">Path to the data.</param>
        /// <returns>False if the data was not present.</returns>
        private bool DeleteData(string path)
        {
            var result = false;
            foreach(var storageData in StorageList)
            {
                if(storageData.DataPath == path)
                {
                    Size -= storageData.Data.Length;
                    StorageList.Remove(storageData);

                    result = true;

                    //Don't look at further paths as there should only be 1.
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Calculate the current size of the scope and assign it to the runningSize.
        /// </summary>
        private void CalculateSize()
        {
            //If there is a storage list then calculate its size, otherwise set the size to 0.
            Size = StorageList != null ? (from item in StorageList select item.Data.Length).Sum() : 0;
        }

        #endregion

        /// <inheritdoc />
        void ICompactSerializable.Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, StorageList);
            CompactSerializer.Write(stream, Name);
        }

        /// <inheritdoc />
        void ICompactSerializable.Deserialize(Stream stream)
        {
            StorageList = CompactSerializer.ReadListSerializable<StorageData>(stream);
            Name = CompactSerializer.ReadString(stream);
        }
    }
}
