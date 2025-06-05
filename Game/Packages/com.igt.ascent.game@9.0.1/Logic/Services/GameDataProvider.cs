//-----------------------------------------------------------------------
// <copyright file = "GameDataProvider.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Services
{
    using System;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Communication.Foundation;
    using CompactSerialization;

    /// <summary>
    /// Basic provider implementation that adds one generic data field and implements <see cref="Save"/> 
    /// and <see cref="Load"/> methods for convenience.
    /// </summary>
    /// <typeparam name="T">Generic provider data type.</typeparam>
    /// <remarks>
    /// The class is intended to be used with a data type that contains other data to be added as
    /// game services. This generic data type must implement <see cref="ICompactSerializable"/>
    /// If you want to expose <typeparamref name="T"/> as a <see cref="GameServiceAttribute"/> itself,
    /// it should implement <see cref="Cloneable.IDeepCloneable"/>. Structs may also be used as the
    /// generic type.
    /// </remarks>
    public abstract class GameDataProvider<T> : NamedProviderBase, IGameDataProvider
        where T : ICompactSerializable, new()
    {
        /// <summary>
        /// The generic data field.
        /// </summary>
        protected T Data;

        /// <summary>
        /// The scope used to read and write the data.
        /// </summary>
        protected readonly CriticalDataScope Scope;

        /// <summary>
        /// The path used to read and write the data.
        /// </summary>
        protected readonly string CriticalDataPath;

        /// <summary>
        /// The flag determines if a data object should be instantiated automatically when none is provided 
        /// in the constructor or loaded from critical data. When true, a new instance of type T will be 
        /// created using its parameter-less constructor. Otherwise, <see cref="Data"/> will be set to the 
        /// default value of type T. This flag is more useful for reference types than value types.
        /// </summary>
        protected readonly bool AutoCreateData;

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
        /// <param name="autoCreateData">
        /// Defines if <see cref="Data"/> should be set to default instead of null when <see cref="Load"/> 
        /// fails, or if the data is null in <see cref="GameDataProvider{T}"/>. This parameter is optional. 
        /// If not specified, it is defaulted to true.
        /// </param>
        /// <param name="data">
        /// The initial data state. This parameter is optional. 
        /// If not specified, it will be set to the type's default value.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="name"/> is null or empty, or if <paramref name="data"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="path"/> is not a valid name for critical data.
        /// </exception>
        /// <remarks>
        /// If <paramref name="autoCreateData"/> is set to false, then <see cref="Data"/> must 
        /// be checked for nullness as needed.
        /// </remarks>
        protected GameDataProvider(CriticalDataScope scope, string path, string name = null,
                                   bool autoCreateData = true, T data = default)
            : base(name)
        {
            if(!Utility.ValidateCriticalDataName(path))
            {
                throw new ArgumentException($"The critical data path {path} is not a valid name for critical data.", nameof(path));
            }

            // ReSharper disable once CompareNonConstrainedGenericWithNull
            if(data == null && autoCreateData)
            {
                Data = new T();
            }
            else
            {
                Data = data;
            }

            Scope = scope;
            CriticalDataPath = path;
            AutoCreateData = autoCreateData;
        }

        #region IGameDataProvider Implementation

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="gameCriticalData"/> is null.
        /// </exception>
        public virtual void Save(IGameCriticalData gameCriticalData)
        {
            if(gameCriticalData == null)
            {
                throw new ArgumentNullException(
                    nameof(gameCriticalData), $"Error occurred while trying to Save {Name}. The critical data access point cannot be null.");
            }

            gameCriticalData.WriteCriticalData(Scope, CriticalDataPath, Data);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="gameCriticalData"/> is null.
        /// </exception>
        public virtual void Load(IGameCriticalData gameCriticalData)
        {
            if(gameCriticalData == null)
            {
                throw new ArgumentNullException(
                    nameof(gameCriticalData), $"Error occurred while trying to Load {Name}. The critical data access point cannot be null.");
            }

            if(!gameCriticalData.TryReadCriticalData(ref Data, Scope, CriticalDataPath))
            {
                Data = AutoCreateData ? new T() : default;
            }
        }

        #endregion
    }
}