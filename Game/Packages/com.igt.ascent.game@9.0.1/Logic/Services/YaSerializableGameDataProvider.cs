// -----------------------------------------------------------------------
// <copyright file = "YaSerializableGameDataProvider.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Services
{
    using System;
    using System.IO;
    using Ascent.Communication.Platform;
    using Ascent.Communication.Platform.Interfaces;
    using CompactSerialization;

    /// <summary>
    /// Yet another serializable game data provider that uses <see cref="ICompactSerializable"/>
    /// to save/load all fields of the provider to/from an <see cref="ICriticalDataStore"/> object.
    /// Works better as the base class of providers that support multiple pieces of data.
    /// </summary>
    /// <remarks>
    /// This provider works with the <see cref="ICriticalDataStore"/> interface that was
    /// first introduced for concurrent gaming.
    /// </remarks>
    public abstract class YaSerializableGameDataProvider : NamedProviderBase, IYaGameDataProvider, ICompactSerializable
    {
        #region Private Fields

        private readonly ICriticalDataStore criticalDataStore;
        private readonly CriticalDataName criticalDataPath;
        private ICriticalDataBlock criticalDataBlock;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="YaSerializableGameDataProvider"/>.
        /// </summary>
        /// <param name="criticalDataStore">
        /// </param>
        /// <param name="criticalDataPath">
        /// </param>
        /// <param name="providerName">
        /// </param>
        protected YaSerializableGameDataProvider(ICriticalDataStore criticalDataStore,
                                                 CriticalDataName criticalDataPath,
                                                 string providerName = null)
            : base(providerName)
        {
            this.criticalDataStore = criticalDataStore ?? throw new ArgumentNullException(nameof(criticalDataStore));

            this.criticalDataPath = criticalDataPath;
        }

        #endregion

        #region IYaGameDataProvider Members

        /// <inheritdoc/>
        public bool IsLoaded => criticalDataBlock != null;

        /// <inheritdoc/>
        public void Save()
        {
            if(criticalDataBlock == null)
            {
                criticalDataBlock = new CriticalDataBlock();
            }

            using(var stream = new MemoryStream())
            {
                Serialize(stream);
                criticalDataBlock.SetSerializedData(criticalDataPath, stream.ToArray());

                criticalDataStore.Write(criticalDataBlock);
            }
        }

        /// <inheritdoc/>
        public void Load(bool allowsOverwrite = false)
        {
            if(criticalDataBlock != null && !allowsOverwrite)
            {
                throw new InvalidOperationException($"Provider {Name} has already been loaded." +
                                                    " A provider should be loaded only once per lifetime for better performances." +
                                                    " Use Load(true) if you really want to load it multiple times.");
            }

            criticalDataBlock = criticalDataStore.Read(criticalDataPath);

            var serializedData = criticalDataBlock.GetSerializedData(criticalDataPath);

            if(serializedData != null)
            {
                using(var stream = new MemoryStream(serializedData))
                {
                    Deserialize(stream);
                }
            }
        }

        #endregion

        #region Implementation of ICompactSerializable

        /// <inheritdoc />
        /// <remarks>
        /// This method is NOT for public use.  It is to be privately used by the provider only.
        /// </remarks>
        public abstract void Serialize(Stream stream);

        /// <inheritdoc />
        /// <remarks>
        /// This method is NOT for public use.  It is to be privately used by the provider only.
        /// </remarks>
        public abstract void Deserialize(Stream stream);

        #endregion
    }
}