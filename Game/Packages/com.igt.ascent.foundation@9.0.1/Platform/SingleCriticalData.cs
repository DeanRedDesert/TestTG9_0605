// -----------------------------------------------------------------------
// <copyright file = "SingleCriticalData.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform
{
    using System;
    using System.Collections.Generic;
    using Interfaces;

    /// <summary>
    /// Implementation of <see cref="ICriticalDataBlock"/> that contains
    /// a single piece of critical data.
    /// </summary>
    public class SingleCriticalData<TData> : CriticalDataBlockBase, ICriticalDataBlock
    {
        #region Private Fields

        private TData dataValue;
        private byte[] serializedBytes;
        private IList<CriticalDataName> nameList;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the data name.
        /// </summary>
        public CriticalDataName Name { get; }

        /// <summary>
        /// Sets or gets the data value.
        /// </summary>
        public TData Data
        {
            get => serializedBytes != null ? dataValue : default;
            set
            {
                dataValue = value;
                serializedBytes = SerializeCriticalData(dataValue);
            }
        }

        /// <summary>
        /// Sets or gets the serialized data.
        /// </summary>
        public byte[] SerializedData
        {
            get => GetSerializedData(Name);
            set => SetSerializedData(Name, value);
        }

        /// <summary>
        /// Gets the string that represents the data.
        /// </summary>
        internal string RepString => PlatformSettings.SafeStorageRepStringsEnabled && serializedBytes != null ? dataValue.ToString() : null;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="SingleCriticalData{TData}"/> with a name of the critical data.
        /// </summary>
        /// <param name="dataName">
        /// The name of the critical data.
        /// </param>
        /// <param name="isDataCompressed">
        /// The flag indicating whether it is compressed critical data block.
        /// This parameter is optional.  If not specified, it defaults to false.
        /// </param>
        public SingleCriticalData(CriticalDataName dataName, bool isDataCompressed = false) : base(isDataCompressed)
        {
            Name = dataName;
        }

        #endregion

        #region Internal Methods

        /// <inheritdoc />
        internal override string GetRepString(CriticalDataName name)
        {
            ValidateName(name);

            return RepString;
        }

        #endregion

        #region ICriticalDataBlock Implementation

        /// <inheritdoc/>
        /// <remarks>
        /// This is a less optimized implementation.  Please use <see cref="Data"/> whenever possible.
        /// </remarks>
        public T GetCriticalData<T>(CriticalDataName name)
        {
            ValidateName(name);
            ValidateType(typeof(T));

            return DeserializeCriticalData<T>(serializedBytes);
        }

        /// <inheritdoc/>
        public byte[] GetSerializedData(CriticalDataName name)
        {
            ValidateName(name);

            return serializedBytes;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This is a less optimized implementation.  Please use <see cref="Data"/> whenever possible.
        /// </remarks>
        public void SetCriticalData<T>(CriticalDataName name, T data)
        {
            ValidateName(name);
            ValidateType(typeof(T));

            serializedBytes = SerializeCriticalData(data);
            dataValue = DeserializeCriticalData<TData>(serializedBytes);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This is a less optimized implementation.  Please use <see cref="Data"/> whenever possible.
        /// </remarks>
        public void SetSerializedData(CriticalDataName name, byte[] data)
        {
            ValidateName(name);

            serializedBytes = data;
            dataValue = DeserializeCriticalData<TData>(serializedBytes);
        }

        /// <inheritdoc/>
        public void DeleteCriticalData(CriticalDataName name)
        {
            ValidateName(name);

            throw new NotSupportedException("Cannot delete the critical data from a SingleCriticalData.");
        }

        /// <inheritdoc/>
        public bool Contains(CriticalDataName name)
        {
            return name == Name;
        }

        /// <inheritdoc/>
        public IList<CriticalDataName> GetNameList()
        {
            return nameList ?? (nameList = new List<CriticalDataName> { Name });
        }

        /// <inheritdoc/>
        public bool HasData => true;

        #endregion

        #region Private Methods

        private void ValidateName(CriticalDataName name)
        {
            if(name != Name)
            {
                throw new NotSupportedException(
                    $"This single critical data block only supports data name of {Name}.  Data name {name} is not supported.");
            }
        }

        private void ValidateType(Type type)
        {
            if(type != typeof(TData))
            {
                throw new NotSupportedException(
                    $"This single critical data block only supports data type of {typeof(TData)}.  Data type {type} is not supported.");
            }
        }

        #endregion
    }
}