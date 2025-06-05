//-----------------------------------------------------------------------
// <copyright file = "PickWin.Extensions.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// Additions to the schema generated PickWin class.
    /// </summary>
    public partial class PickWin : ICompactSerializable
    {
        /// <summary>
        /// Construct an empty pick win.
        /// </summary>
        public PickWin()
        {
        }

        /// <summary>
        /// Create a PickWin with the contents of another.
        /// </summary>
        /// <param name="other">A pick win to copy.</param>
        public PickWin(PickWin other)
        {
            CopyMembers(other);
        }

        /// <summary>
        /// Copy the members of another pick win to this one.
        /// </summary>
        /// <param name="other">The pick win to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown if the pick win to copy is null.</exception>
        private void CopyMembers(PickWin other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other", "Other pick win may not be null.");
            }

            valueFieldSpecified = other.valueFieldSpecified;
            value = other.value;
            winLevelIndex = other.winLevelIndex;
            ProgressiveLevel.AddRange(other.ProgressiveLevel);
            Trigger.AddRangeCopy(other.Trigger);
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, valueFieldSpecified);
            CompactSerializer.Write(stream, value);
            CompactSerializer.Write(stream, winLevelIndex);
            CompactSerializer.WriteList(stream, ProgressiveLevel);
            CompactSerializer.WriteList(stream, Trigger);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            valueFieldSpecified = CompactSerializer.ReadBool(stream);
            value = CompactSerializer.ReadLong(stream);
            winLevelIndex = CompactSerializer.ReadUint(stream);
            ProgressiveLevel = CompactSerializer.ReadListString(stream);
            Trigger = CompactSerializer.ReadListSerializable<Trigger>(stream);
        }

        #endregion
    }
}
