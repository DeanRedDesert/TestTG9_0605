// -----------------------------------------------------------------------
// <copyright file = "KeyFrameTable.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Streaming
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using CompactSerialization;

    /// <summary>
    /// The collection of information on key frames.
    /// </summary>
    internal class KeyFrameTable : Collection<KeyFrameEntry>, ICompactSerializable, IEquatable<KeyFrameTable>
    {
        /// <summary>
        /// Determines if there is an entry for a given frame index.
        /// </summary>
        /// <param name="frameIndex">The frame index to check for.</param>
        /// <returns>True if found.</returns>
        public bool ContainsKeyFrameAtIndex(int frameIndex)
        {
            return Items.Any(item => item.FrameIndex == frameIndex);
        }

        /// <summary>
        /// Removes the entry with the specified frame index if it exists in the collection.
        /// </summary>
        /// <param name="frameIndex">The frame index in the entry to remove.</param>
        /// <returns>True if the item was removed.</returns>
        public bool Remove(int frameIndex)
        {
            var item = Items.FirstOrDefault(entry => entry.FrameIndex == frameIndex);

            return item != null && Remove(item);
        }

        /// <inheritdoc />
        public bool Equals(KeyFrameTable other)
        {
            if(other != null)
            {
                if(ReferenceEquals(this, other))
                {
                    return true;
                }

                return Count == other.Count && this.SequenceEqual(other);
            }

            return false;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as KeyFrameTable);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
            return base.GetHashCode();
        }

        #region ICompactSerializable

        /// <inheritdoc />
        void ICompactSerializable.Deserialize(Stream stream)
        {
            Clear();
            var count = CompactSerializer.ReadInt(stream);
            while(count > 0)
            {
                var item = CompactSerializer.ReadSerializable<KeyFrameEntry>(stream);
                Add(item);
                count--;
            }
        }

        /// <inheritdoc />
        void ICompactSerializable.Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, Items.Count);
            foreach(var item in Items)
            {
                CompactSerializer.Write(stream, item);
            }
        }

        #endregion
    }
}