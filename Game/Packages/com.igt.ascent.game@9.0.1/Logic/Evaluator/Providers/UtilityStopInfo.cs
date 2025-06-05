//-----------------------------------------------------------------------
// <copyright file = "UtilityStopInfo.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Providers
{
    using System;
    using System.IO;
    using Cloneable;
    using CompactSerialization;

    /// <summary>
    /// This class provides physial stop numbers and virtual ranges which are used to display in Utility.
    /// </summary>
    [Serializable]
    public class UtilityStopInfo : ICompactSerializable, IDeepCloneable
    {
        /// <summary>
        /// Gets the physical stop.
        /// </summary>
        public int DisplayablePhysicalStop { get; private set; }

        /// <summary>
        /// Gets the virtual range.
        /// </summary>
        public VirtualRange DisplayableVirtualRange { get; private set; }

        /// <summary>
        /// Constructs a new default instance.
        /// </summary>
        /// <remarks>
        /// Required for serialization.
        /// </remarks>
        public UtilityStopInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UtilityStopInfo" /> class.
        /// </summary>
        /// <param name="displayablePhysicalStop">The displayable physical stop.</param>
        /// <param name="displayableVirtualRange">The displayable virtual range.</param>
        public UtilityStopInfo(int displayablePhysicalStop, VirtualRange displayableVirtualRange)
        {
            DisplayablePhysicalStop = displayablePhysicalStop;
            DisplayableVirtualRange = displayableVirtualRange;
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, DisplayablePhysicalStop);
            CompactSerializer.Write(stream, DisplayableVirtualRange);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            DisplayablePhysicalStop = CompactSerializer.ReadInt(stream);
            DisplayableVirtualRange = CompactSerializer.ReadSerializable<VirtualRange>(stream);
        }

        #endregion

        #region Implementation of IDeepCloneable

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