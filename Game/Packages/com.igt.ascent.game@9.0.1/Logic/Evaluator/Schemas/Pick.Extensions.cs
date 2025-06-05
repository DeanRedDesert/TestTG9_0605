//-----------------------------------------------------------------------
// <copyright file = "Pick.Extensions.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the schema generated Pick class.
    /// </summary>
    public partial class Pick : ICompactSerializable
    {
        /// <summary>
        /// Create an empty pick.
        /// </summary>
        public Pick() {}

        /// <summary>
        /// Construct a pick with the contents of another.
        /// </summary>
        /// <param name="other">Pick to copy.</param>
        public Pick(Pick other)
        {
            CopyMembers(other);
        }

        /// <summary>
        /// Copy the contents of a pick into this pick.
        /// </summary>
        /// <param name="other">Pick to copy.</param>
        private void CopyMembers(Pick other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other", "Other pick may not be null.");
            }

            name = other.name;
            weight = other.weight;
            if (other.Win != null)
            {
                Win = new PickWin(other.Win);
            }
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, name);
            CompactSerializer.Write(stream, weight);
            CompactSerializer.Write(stream, Win);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            name = CompactSerializer.ReadString(stream);
            weight = CompactSerializer.ReadUint(stream);
            Win = CompactSerializer.ReadSerializable<PickWin>(stream);
        }

        #endregion
    }
}
