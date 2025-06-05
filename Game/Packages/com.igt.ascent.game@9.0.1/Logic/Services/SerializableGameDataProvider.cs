//-----------------------------------------------------------------------
// <copyright file = "SerializableGameDataProvider.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Services
{
    using System;
    using System.IO;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Communication.Foundation;
    using CompactSerialization;

    /// <summary>
    /// Basic provider implementation that reads and writes all provider data fields to critical data 
    /// using <see cref="ICompactSerializable"/>.
    /// </summary>
    /// <remarks>
    /// This class is intended to be used with multiple data fields.
    /// </remarks>
    public abstract class SerializableGameDataProvider : NamedProviderBase, IGameDataProvider, ICompactSerializable
    {
        /// <summary>
        /// The scope used to read and write the data.
        /// </summary>
        protected readonly CriticalDataScope Scope;

        /// <summary>
        /// The path used to read and write the data.
        /// </summary>
        protected readonly string CriticalDataPath;

        /// <summary>
        /// Create an instance of the provider and initialize all provider data.
        /// </summary>
        /// <param name="scope">
        /// The scope used to read and write the data.
        /// </param>
        /// <param name="path">
        /// The path used to read and write the data.
        /// </param>
        /// <param name="name">
        /// The provider name. If not specified, <see cref="NamedProviderBase.DefaultName"/> will be used as Name.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="name"/> is null or empty.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="path"/> is not a valid name for critical data.
        /// </exception>
        protected SerializableGameDataProvider(CriticalDataScope scope, string path, string name = null) : base(name)
        {
            if(!Utility.ValidateCriticalDataName(path))
            {
                throw new ArgumentException($"The critical data path {path} is not a valid name for critical data.", nameof(path));
            }

            Scope = scope;
            CriticalDataPath = path;
        }

        #region IGameDataProvider Implementation

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="gameCriticalData"/> is null.
        /// </exception>
        public void Save(IGameCriticalData gameCriticalData)
        {
            if(gameCriticalData == null)
            {
                throw new ArgumentNullException(
                    nameof(gameCriticalData), $"Error occurred while trying to Save {Name}. The critical data access point cannot be null.");
            }

            using(var memoryStream = new MemoryStream())
            {
                Serialize(memoryStream);
                gameCriticalData.WriteCriticalData(Scope, CriticalDataPath, memoryStream.ToArray());
            }
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="gameCriticalData"/> is null.
        /// </exception>
        public void Load(IGameCriticalData gameCriticalData)
        {
            if(gameCriticalData == null)
            {
                throw new ArgumentNullException(
                    nameof(gameCriticalData), $"Error occurred while trying to Load {Name}. The critical data access point cannot be null.");
            }

            var serializedData = gameCriticalData.ReadCriticalData<byte[]>(Scope, CriticalDataPath);
            if(serializedData != null)
            {
                using(var memoryStream = new MemoryStream(serializedData))
                {
                    Deserialize(memoryStream);
                }
            }
        }

        #endregion

        #region ICompactSerializable Implementation

        /// <inheritdoc />
        public abstract void Serialize(Stream stream);

        /// <inheritdoc />
        public abstract void Deserialize(Stream stream);

        #endregion
    }
}