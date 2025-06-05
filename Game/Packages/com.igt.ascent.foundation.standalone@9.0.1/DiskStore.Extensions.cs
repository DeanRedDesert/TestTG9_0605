//-----------------------------------------------------------------------
// <copyright file = "DiskStore.Extensions.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;
    using Ascent.Communication.Platform.Interfaces;
    using CompactSerialization;

    /// <summary>
    /// This class contains methods which extend the functionality
    /// of the automatically generated type DiskStore. It encapsulates
    /// the ability to read and write DiskStore XML files and adds
    /// convenience methods for reading and writing data from a store.
    /// </summary>
    public partial class DiskStore : ICompactSerializable
    {
        #region Fields

        /// <summary>
        /// Section map to improve performance of section lookup.
        /// </summary>
        private Dictionary<DiskStoreSection, StorageSection> sectionMap =
            new Dictionary<DiskStoreSection, StorageSection>();

        /// <summary>
        /// Scope map to improve the performance of scope lookup.
        /// </summary>
        private Dictionary<DiskStoreSection, Dictionary<int, StorageScope>> scopeMap =
            new Dictionary<DiskStoreSection, Dictionary<int, StorageScope>>();

        #endregion
        
        #region File Operations

        /// <summary>
        /// The Load method is used to load a DiskStore from a TextReader object.
        /// This method can be used to load a DiskStore from a file by making
        /// a TextReader object backed by a StreamReader for a file.
        /// </summary>
        /// <param name="reader">TextReader to read the DiskStore from.</param>
        /// <returns>The DiskStore object.</returns>
        public static DiskStore Load(TextReader reader)
        {
            var serializer = new XmlSerializer(typeof(DiskStore));
            return serializer.Deserialize(reader) as DiskStore;
        }

        /// <summary>
        /// The Save method is used to save a DiskStore object to a TextWriter.
        /// This method can be used to save a DiskStore to a file by making a
        /// TextWriter object backed by a StreamWriter for a file.
        /// </summary>
        /// <param name="writer">TextWriter object to write the DiskStore to.</param>
        /// <param name="diskStore">DiskStore object to be written.</param>
        public static void Save(TextWriter writer, DiskStore diskStore)
        {
            var serializer = new XmlSerializer(typeof(DiskStore));
            serializer.Serialize(writer, diskStore);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Remove all the critical data entries from the specified scope.
        /// </summary>
        /// <param name="section">The section to remove the data from.</param>
        /// <param name="scope">The scope to clear in the specified section.</param>
        public void ClearScope(DiskStoreSection section, int scope)
        {
            var storageScope = GetStorageScope(section, scope, false);
            storageScope?.Clear();
        }

        /// <summary>
        /// Check if an item already exists in the disk store.
        /// </summary>
        /// <param name="section">The section to read the data from.</param>
        /// <param name="scope">The scope to read the data from.</param>
        /// <param name="path">The path to read the data from.</param>
        /// <returns>True if the item already exists in the disk store.  False otherwise.</returns>
        public bool Contains(DiskStoreSection section, int scope, string path)
        {
            var storageScope = GetStorageScope(section, scope, false);
            return storageScope?.Contains(path) == true;
        }

        /// <summary>
        /// Read data from the specified disk store section, scope and path.
        /// </summary>
        /// <param name="section">The section to read the data from.</param>
        /// <param name="scope">The scope to read the data from.</param>
        /// <param name="path">The path to read the data from.</param>
        /// <returns>
        /// Byte array containing the read data. If the data did not exist, then
        /// null will be returned.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the path passed in is null.
        /// </exception>
        public byte[] ReadData(DiskStoreSection section, int scope, string path)
        {
            if(path == null)
            {
                throw new ArgumentNullException(nameof(path), "Path argument may not be null.");
            }

            var storageScope = GetStorageScope(section, scope, false);
            return storageScope?.ReadData(path);
        }

        /// <summary>
        /// Write data to the specified scope and path.
        /// </summary>
        /// <param name="section">The section to write the data.</param>
        /// <param name="scope">The scope to write the data.</param>
        /// <param name="path">The path to write the data.</param>
        /// <param name="data">The data to write.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the path passed in is null.
        /// </exception>
        public void WriteData(DiskStoreSection section, int scope, string path, byte[] data)
        {
            if(path == null)
            {
                throw new ArgumentNullException(nameof(path), "Path argument may not be null.");
            }

            var storageScope = GetStorageScope(section, scope, true);
            storageScope.WriteData(path, data);
        }

        /// <summary>
        /// Remove data from the specified scope and path.
        /// </summary>
        /// <param name="section">The section to write the data.</param>
        /// <param name="scope">The scope to write the data.</param>
        /// <param name="path">The path to write the data.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the path passed in is null.
        /// </exception>
        /// <returns>True if the data was removed.</returns>
        public bool RemoveData(DiskStoreSection section, int scope, string path)
        {
            if(path == null)
            {
                throw new ArgumentNullException(nameof(path), "Path argument may not be null.");
            }

            var storageScope = GetStorageScope(section, scope, false);
            return storageScope?.RemoveData(path) == true;
        }

        /// <summary>
        /// Swap the storage lists of two safe storage scopes.
        /// </summary>
        /// <param name="section1">The section of the first storage list.</param>
        /// <param name="scope1">The scope of the first storage list.</param>
        /// <param name="section2">The section of the second storage list.</param>
        /// <param name="scope2">The scope of the second storage list.</param>
        public void SwapScopes(DiskStoreSection section1, int scope1, DiskStoreSection section2, int scope2)
        {
            // Call with "isWrite" flag set to create the scope if it is not created yet.
            var storageScope1 = GetStorageScope(section1, scope1, true);
            var storageScope2 = GetStorageScope(section2, scope2, true);

            var tempList = storageScope1.StorageList;
            storageScope1.SetStorageList(storageScope2.StorageList);
            storageScope2.SetStorageList(tempList);
        }

        /// <summary>
        /// Make a deep copy of the source storage list, and assign to the
        /// destination storage list.  The original content of the destination
        /// storage list will be discarded.
        /// </summary>
        /// <param name="sourceSection">The section of the source storage list.</param>
        /// <param name="sourceScope">The scope of the source storage list.</param>
        /// <param name="destinationSection">The section of the destination storage list.</param>
        /// <param name="destinationScope">The scope of the destination storage list.</param>
        public void CopyScope(DiskStoreSection sourceSection, int sourceScope,
                              DiskStoreSection destinationSection, int destinationScope)
        {
            // Call with "isWrite" flag set to create the scope if it is not created yet.
            var source = GetStorageScope(sourceSection, sourceScope, true);
            var destination = GetStorageScope(destinationSection, destinationScope, true);

            destination.CloneScope(source);
        }

        /// <summary>
        /// Get the name list of items stored in a given data storage scope.
        /// </summary>
        /// <param name="section">The section to read from.</param>
        /// <param name="scope">The scope for which the manifest is requested.</param>
        /// <returns>
        /// Name list of the items stored the given scope, null if the specified scope doesn't exist.
        /// </returns>
        public ICollection<string> GetManifest(DiskStoreSection section, int scope)
        {
            var storageScope = GetStorageScope(section, scope, false);
            return storageScope?.GetManifest();
        }

        /// <summary>
        /// Get the current usage of the specified scope.
        /// </summary>
        /// <param name="section">The section containing the scope.</param>
        /// <param name="scope">The scope to get the usage for.</param>
        /// <returns>The current usage of the specified scope in bytes.</returns>
        public int GetScopeUsage(DiskStoreSection section, int scope)
        {
            var storageScope = GetStorageScope(section, scope, false);
            return storageScope?.Size ?? 0;
        }

        /// <summary>
        /// Check if the given path is a directory within the specified section and scope.
        /// </summary>
        /// <param name="section">The section to check the path in.</param>
        /// <param name="scope">The scope to check the path in.</param>
        /// <param name="path">The path to check.</param>
        /// <returns>True if the specified path is a directory.</returns>
        public bool IsDirectory(DiskStoreSection section, int scope, string path)
        {
            var storageScope = GetStorageScope(section, scope, false);
            return storageScope?.IsDirectory(path) == true;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Get the storage scope in the disk store where the target data
        /// locates, depending on the specified section and scope.
        /// </summary>
        /// <param name="section">The disk store section of the target data.</param>
        /// <param name="scope">The scope within the disk store section.</param>
        /// <param name="isWrite">Flag indicating if this is a write operation  If yes, the
        ///                       corresponding storage scope will be allocated if not yet.</param>
        /// <returns>
        /// The storage scope found/created.  Null if the storage scope does not
        /// exist and it is a read operation.
        /// </returns>
        private StorageScope GetStorageScope(DiskStoreSection section, int scope, bool isWrite)
        {
            if(scopeMap.ContainsKey(section) && scopeMap[section].ContainsKey(scope))
            {
                return scopeMap[section][scope];
            }

            StorageScope foundScope = null;
            var foundSection = GetStorageSection(section, isWrite);

            // With a valid Disk Store Section...
            if(foundSection != null)
            {
                // Check if the Scopes list is allocated.  Allocate one
                // if not yet and it is a write operation.
                if(isWrite && foundSection.Scopes == null)
                {
                    foundSection.Scopes = new List<StorageScope>();
                }

                // With a valid Scopes list...
                if(foundSection.Scopes != null)
                {
                    var scopeName = GetScopeName(section, scope);

                    // Search for the Storage Scope.  Create one if none exists
                    // and it is a write operation.
                    foundScope = (from c in foundSection.Scopes where c.Name == scopeName select c).FirstOrDefault();

                    if(isWrite && foundScope == null)
                    {
                        var newScope = new StorageScope {Name = scopeName};
                        foundSection.Scopes.Add(newScope);
                        foundScope = newScope;
                    }
                }
            }

            if(foundScope != null)
            {
                if(!scopeMap.ContainsKey(section))
                {
                    scopeMap[section] = new Dictionary<int, StorageScope>();
                }

                scopeMap[section][scope] = foundScope;
            }

            return foundScope;
        }

        /// <summary>
        /// Get the storage section in the disk store where the target data is
        /// located, depending on the specified section and scope.
        /// </summary>
        /// <param name="section">The disk store section of the target data.</param>
        /// <param name="isWrite">Flag indicating if this is a write operation  If yes, the
        ///                       corresponding storage section will be allocated if not yet.</param>
        /// <returns>
        /// The storage section found/created.  Null if the storage section does not
        /// exist and it is a read operation.
        /// </returns>
        private StorageSection GetStorageSection(DiskStoreSection section, bool isWrite)
        {
            StorageSection foundSection = null;
            
            if(sectionMap.ContainsKey(section))
            {
                return sectionMap[section];
            }
            
            var sectionString = section.ToString();
            
            // Check if the root Sections list is allocated.  Allocate one if
            // not yet and it is a write operation.
            if(isWrite && Sections == null)
            {
                Sections = new List<StorageSection>();
            }

            // With a valid Sections list...
            if(Sections != null)
            {
                // Search for the Disk Store Section.  Create one if none exists
                // and it is a write operation.
                foundSection = (from s in Sections where s.Name == sectionString select s).FirstOrDefault();

                if(isWrite && foundSection == null)
                {
                    var newSection = new StorageSection { Name = sectionString };
                    Sections.Add(newSection);
                    foundSection = newSection;
                }
            }

            if(foundSection != null)
            {
                sectionMap[section] = foundSection;
            }

            return foundSection;
        }

        /// <summary>
        /// Convert the scope enum to scope name string, depending on the
        /// enum type.
        /// </summary>
        /// <param name="section">The disk store section.</param>
        /// <param name="scope">The scope within the disk store section.</param>
        /// <returns></returns>
        private string GetScopeName(DiskStoreSection section, int scope)
        {
            string scopeName;

            // Convert scope enum to string.
            switch(section)
            {
                case DiskStoreSection.CriticalData:
                    scopeName = ((CriticalDataScope)scope).ToString();
                    break;

                case DiskStoreSection.Meters:
                    scopeName = ((MeterScope)scope).ToString();
                    break;

                case DiskStoreSection.FoundationData:
                    scopeName = ((FoundationDataScope)scope).ToString();
                    break;

                case DiskStoreSection.History:
                    scopeName = "History Record " + scope;
                    break;

                case DiskStoreSection.ThemeCriticalData:
                    scopeName = "Critical Data Of Theme " + scope;
                    break;

                case DiskStoreSection.ThemeAnalyticsCriticalData:
                    scopeName = "Critical Data Of Theme Analytics " + scope;
                    break;

                case DiskStoreSection.ThemePersistentCriticalData:
                    scopeName = "Critical Data Of Theme Persistent " + scope;
                    break;

                case DiskStoreSection.ThemeConfigurations:
                    scopeName = "Configurations Of Theme " + scope;
                    break;

                case DiskStoreSection.ThemeConfigurationProfiles:
                    scopeName = "Configuration Profiles Of Theme " + scope;
                    break;

                case DiskStoreSection.PayvarCriticalData:
                    scopeName = "Critical Data Of Payvar " + scope;
                    break;

                case DiskStoreSection.PayvarAnalyticsCriticalData:
                    scopeName = "Critical Data Of Payvar Analytics " + scope;
                    break;

                case DiskStoreSection.PayvarPersistentCriticalData:
                    scopeName = "Critical Data Of Payvar Persistent " + scope;
                    break;

                case DiskStoreSection.PayvarConfigurations:
                    scopeName = "Configurations Of Payvar " + scope;
                    break;

                case DiskStoreSection.PayvarConfigurationProfiles:
                    scopeName = "Configuration Profiles Of Payvar " + scope;
                    break;

                case DiskStoreSection.ExtensionConfigurationProfiles:
                    scopeName = "Configuration Profiles Of Extension " + scope;
                    break;

                case DiskStoreSection.ExtensionConfigurations:
                    scopeName = "Configuration Of Extension " + scope;
                    break;

                default:
                    throw new InvalidOperationException(
                        $"{section} section is not supported.");
            }

            return scopeName;
        }

        #endregion
        
        /// <inheritdoc />
        void ICompactSerializable.Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, Sections);
        }

        /// <inheritdoc />
        void ICompactSerializable.Deserialize(Stream stream)
        {
            Sections = CompactSerializer.ReadListSerializable<StorageSection>(stream);
        }
    }
}
