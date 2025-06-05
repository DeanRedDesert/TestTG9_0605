// -----------------------------------------------------------------------
// <copyright file = "YaGameDataProvider.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Services
{
    using System;
    using Ascent.Communication.Platform;
    using Ascent.Communication.Platform.Interfaces;
    using CompactSerialization;

    /// <summary>
    /// Yet another game data provider that has one generic data field and implements a Save and a Load method
    /// to conveniently save/load the generic data to/from an <see cref="ICriticalDataStore"/> object.
    /// Works better as the base class of providers that support a single piece of data.
    /// </summary>
    /// <typeparam name="TData">The data type supported by the provider.</typeparam>
    /// <remarks>
    /// The generic data type <typeparamref name="TData"/> must be a class or a struct that implements
    /// <see cref="ICompactSerializable"/> for better performance of saving into critical data.
    /// If it needs to be exposed as as a <see cref="GameServiceAttribute"/>, it should implement
    /// <see cref="Cloneable.IDeepCloneable"/> as well.
    /// 
    /// This provider works with the <see cref="ICriticalDataStore"/> interface that was
    /// first introduced for concurrent gaming.
    /// </remarks>
    public abstract class YaGameDataProvider<TData> : NamedProviderBase, IYaGameDataProvider
        where TData : ICompactSerializable, new()
    {
        #region Fields

        private readonly ICriticalDataStore criticalDataStore;
        private readonly CriticalDataName criticalDataPath;
        private ICriticalDataBlock criticalDataBlock;

        /// <summary>
        /// The flag determines if a data object should be instantiated automatically when none is provided 
        /// in the constructor or loaded from critical data. When true, a new instance of type T will be 
        /// created using its parameter-less constructor. Otherwise, <see cref="Data"/> will be set to the 
        /// default value of type T. This flag is more useful for reference types than value types.
        /// </summary>
        protected readonly bool AutoCreateData;

        /// <summary>
        /// The data supported by the provider.
        /// </summary>
        protected TData Data;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="YaGameDataProvider{TData}"/>.
        /// </summary>
        protected YaGameDataProvider(ICriticalDataStore criticalDataStore,
                                    CriticalDataName criticalDataPath,
                                    string providerName = null,
                                    bool autoCreateData = true,
                                    TData data = default)
            : base(providerName)
        {
            this.criticalDataStore = criticalDataStore ?? throw new ArgumentNullException(nameof(criticalDataStore));
            this.criticalDataPath = criticalDataPath;

            if(data == null && autoCreateData)
            {
                Data = new TData();
            }
            else
            {
                Data = data;
            }

            AutoCreateData = autoCreateData;
        }

        #endregion

        #region IYaGameDataProvider Members

        /// <inheritdoc/>
        public bool IsLoaded => criticalDataBlock != null;

        /// <inheritdoc/>
        // ReSharper disable once VirtualMemberNeverOverridden.Global
        public virtual void Save()
        {
            if(criticalDataBlock == null)
            {
                criticalDataBlock = new CriticalDataBlock();
            }

            criticalDataBlock.SetCriticalData(criticalDataPath, Data);

            criticalDataStore.Write(criticalDataBlock);
        }

        /// <inheritdoc/>
        // ReSharper disable once VirtualMemberNeverOverridden.Global
        public virtual void Load(bool allowsOverwrite = false)
        {
            if(criticalDataBlock != null && !allowsOverwrite)
            {
                throw new InvalidOperationException($"Provider {Name} has already been loaded." +
                                                    " A provider should be loaded only once per lifetime for better performances." +
                                                    " Use Load(true) if you really want to load it multiple times.");
            }

            criticalDataBlock = criticalDataStore.Read(criticalDataPath);

            Data = criticalDataBlock.GetCriticalData<TData>(criticalDataPath);

            if(Data == null && AutoCreateData)
            {
                Data = new TData();
            }
        }

        #endregion
    }
}