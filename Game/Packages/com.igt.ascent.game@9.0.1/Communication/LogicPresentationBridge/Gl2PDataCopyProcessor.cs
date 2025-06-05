// -----------------------------------------------------------------------
// <copyright file = "Gl2PDataCopyProcessor.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.LogicPresentationBridge
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Cloneable;
    using CommunicationLib;
    using CompactSerialization;
    using Tracing;

    /// <summary>
    /// This processor is used to deep copy GL2P data.
    /// </summary>
    internal class Gl2PDataCopyProcessor : IDataCopyProcessor
    {
        #region Fields

        /// <summary>
        /// The deep copy processors indexed by the data type.
        /// </summary>
        private readonly IDictionary copyProcessors;

        /// <summary>
        /// The binary formatter used to serialize the GL2P object in certain circumstances.
        /// </summary>
        private BinaryFormatter formatter;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the GL2P data copy metrics recorder which is used to record specified metrics of the data copy process.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public Gl2PDataCopyMetricsRecorder MetricsRecorder { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// The default constructor.
        /// </summary>
        public Gl2PDataCopyProcessor()
        {
            var genericListCopyProcessor = new GenericListCopyProcessor(this);
            var genericDictionaryCopyProcessor = new GenericDictionaryCopyProcessor(this, this);
            var genericReadOnlyCollectionCopyProcessor = new GenericReadOnlyCollectionCopyProcessor(this);
            var keyValuePairCopyProcessor = new KeyValuePairCopyProcessor(this, this);

            copyProcessors = new ListDictionary
                             {
                                 {typeof(DataItems), new DataItemsCopyProcessor(this)},
                                 {typeof(List<>), genericListCopyProcessor},
                                 {typeof(Dictionary<,>), genericDictionaryCopyProcessor},
                                 {typeof(ReadOnlyCollection<>), genericReadOnlyCollectionCopyProcessor},
                                 {typeof(KeyValuePair<,>), keyValuePairCopyProcessor}
                             };
        }

        #endregion

        #region Implementation of IDeepCopyProcessor

        /// <inheritdoc />
        public object DeepCopy(object source)
        {
            if(source == null)
            {
                return null;
            }

            var realType = source.GetType();
            // Pass through primitive and enum types.
            if(realType.IsPrimitive || realType.IsEnum)
            {
                return source;
            }

            if(realType == typeof(string))
            {
                return source;
            }

            if(copyProcessors.Contains(realType))
            {
                return ((IDataCopyProcessor)copyProcessors[realType]).DeepCopy(source);
            }

            // In case the custom struct implements IDeepCloneable, we need to handle it before passing it through directly.
            // This check also goes before the generic check, in case the generic custom type implements IDeepCloneable.
            if(source is IDeepCloneable deepCloneable)
            {
                return deepCloneable.DeepClone();
            }

            // Generic check goes before struct check in order to copy KeyValuePair<,> properly.
            if(realType.IsGenericType)
            {
                var genericDefinition = realType.GetGenericTypeDefinition();
                
                if(copyProcessors.Contains(genericDefinition))
                {
                    return ((IDataCopyProcessor)copyProcessors[genericDefinition]).DeepCopy(source);
                }
            }

            // Pass through other struct types besides primitives and enums, which does not implement IDeepCloneable.
            // If a custom struct contains reference type fields, it's the custom struct's obligation
            // to implement IDeepCloneable for a custom deep clone.
            if(realType.IsValueType)
            {
                return source;
            }

            Gl2PTracing.Log.MissingIDeepCloneable(realType.FullName);

            return SerialCopy(source, realType);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Serial copy the object with the given type.
        /// </summary>
        /// <param name="source">The source to copy from.</param>
        /// <param name="realType">The type of the source object.</param>
        /// <returns>The copied object.</returns>
        private object SerialCopy(object source, Type realType)
        {
            object result;
            using(var buffer = new MemoryStream())
            {
                if(CompactSerializer.Supports(realType))
                {
                    CompactSerializer.Serialize(buffer, source);
                    buffer.Seek(0, SeekOrigin.Begin);
                    result = CompactSerializer.Deserialize(buffer, realType);
                }
                else
                {
                    Gl2PTracing.Log.MissingICompactSerializable(realType.FullName);

                    if(formatter == null)
                    {
                        formatter = new BinaryFormatter();
                    }
                    formatter.Serialize(buffer, source);
                    buffer.Seek(0, SeekOrigin.Begin);
                    result = formatter.Deserialize(buffer);
                }

                MetricsRecorder?.IncreaseSerializationSize(buffer.Length);
            }

            return result;
        }

        #endregion
    }
}