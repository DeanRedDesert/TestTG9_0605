// -----------------------------------------------------------------------
// <copyright file = "DataItemsCopyProcessor.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.LogicPresentationBridge
{
    using System;
    using System.Collections.Generic;
    using Cloneable;
    using CommunicationLib;
    using Tracing;

    /// <summary>
    /// This processor is used to deep copy <see cref="DataItems"/> data.
    /// </summary>
    /// <remarks>
    /// We didn't implement <see cref="IDeepCloneable"/> directly on DataItems, because it's very hard to record the GL2P metrics
    /// in that class.
    /// </remarks>
    internal sealed class DataItemsCopyProcessor : IDataCopyProcessor
    {
        #region Fields

        /// <summary>
        /// This processor is used to copy the service data from the data items.
        /// </summary>
        private readonly Gl2PDataCopyProcessor elementCopyProcessor;

        #endregion

        #region Constructor

        /// <summary>
        /// Construct the instance with the given service data copy processor.
        /// </summary>
        /// <param name="elementCopyProcessor">The processor used to copy the service data.</param>
        /// <exception cref="ArgumentNullException">Thrown when <see cref="elementCopyProcessor"/> is null.</exception>
        public DataItemsCopyProcessor(Gl2PDataCopyProcessor elementCopyProcessor)
        {
            if(elementCopyProcessor == null)
            {
                throw new ArgumentNullException("elementCopyProcessor");
            }

            this.elementCopyProcessor = elementCopyProcessor;
        }

        #endregion

        /// <inheritdoc />
        public object DeepCopy(object source)
        {
            if(source == null)
            {
                return null;
            }

            var dataItems = source as DataItems;
            if(dataItems == null)
            {
                throw new ArgumentException("The source type is not supported.", "source");
            }

            var clone = new DataItems();
            foreach(var entry in dataItems)
            {
                var innerDictionary = entry.Value == null ? null : new Dictionary<int, object>(entry.Value.Count);
                clone.Add(entry.Key, innerDictionary);
                if(innerDictionary != null)
                {
                    foreach(var innerEntry in entry.Value)
                    {
                        long currentSize = 0;
                        if(elementCopyProcessor.MetricsRecorder != null)
                        {
                            currentSize = elementCopyProcessor.MetricsRecorder.GetSerializationSize();
                        }
                        var innerValue = innerEntry.Value == null
                            ? null
                            : elementCopyProcessor.DeepCopy(innerEntry.Value);
                        if(elementCopyProcessor.MetricsRecorder != null)
                        {
                            var serializationSize =
                                elementCopyProcessor.MetricsRecorder.GetSerializationSize() - currentSize;
                            Gl2PTracing.Log
                                .PerServiceSerializationSize(entry.Key, innerEntry.Key, (int)serializationSize);
                        }
                        innerDictionary.Add(innerEntry.Key, innerValue);
                    }
                }
            }
            return clone;
        }
    }
}