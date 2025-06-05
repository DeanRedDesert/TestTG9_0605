//-----------------------------------------------------------------------
// <copyright file = "HistoryStopInfo.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Providers
{
    using System;
    using System.IO;
    using CompactSerialization;
    using Cloneable;

    /// <summary>
    /// This class provides physial stop numbers and virtual stop numbers which are used to display in history.
    /// </summary>
    [Serializable]
    public class HistoryStopInfo : ICompactSerializable, IDeepCloneable
    {
        /// <summary>
        /// Gets the displayable physical stop.
        /// </summary>
        public int DisplayablePhysicalStop { get; private set; }

        /// <summary>
        /// Gets the displayable virtual stop.
        /// </summary>
        public int DisplayableVirtualStop { get; private set; }

        /// <summary>
        /// Construct a new default instance.
        /// </summary>
        /// <remarks>
        /// Required for serialization.
        /// </remarks>
        public HistoryStopInfo()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryStopInfo" /> class.
        /// </summary>
        /// <param name="displayablePhysicalStop">The displayable physical stop.</param>
        /// <param name="displayableVirtualStop">The displayable virtual stop.</param>
        public HistoryStopInfo(int displayablePhysicalStop, int displayableVirtualStop)
        {
            DisplayablePhysicalStop = displayablePhysicalStop;
            DisplayableVirtualStop = displayableVirtualStop;
        }

        #region Implementation of ICompactSerializable

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, DisplayablePhysicalStop);
            CompactSerializer.Write(stream, DisplayableVirtualStop);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            DisplayablePhysicalStop = CompactSerializer.ReadInt(stream);
            DisplayableVirtualStop = CompactSerializer.ReadInt(stream);
        }

        #endregion

        #region Implementation of IDeepClonable

        /// <inheritdoc />
        public object DeepClone()
        {
            // This type is supposed to be immutable. However, the invoking to Deserialize() of an existing instance could 
            // corrupt its immutibility. Thus, we must disallow such invoking for an instance already in use.
            return this;
        }

        #endregion
    }
}
