//-----------------------------------------------------------------------
// <copyright file = "ICompactSerializable.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.CompactSerialization
{
    using System.IO;

    /// <summary>
    /// This interface represents a data type that
    /// can be serialized by <see cref="CompactSerializer"/>.
    /// </summary>
    /// <remarks>
    /// Types implementing this interface must have a
    /// public parameter-less constructor in order for
    /// the <see cref="CompactSerializer"/> to create
    /// an instance of the type upon which the Deserialize
    /// method can be called.
    /// </remarks>
    public interface ICompactSerializable
    {
        /// <summary>
        /// Serialize the instance data to the given stream.
        /// </summary>
        /// <param name="stream">
        /// The stream where the instance data is serialized to.
        /// </param>
        void Serialize(Stream stream);

        /// <summary>
        /// Populate the instance data with information
        /// deserialized from the given stream.
        /// </summary>
        /// <param name="stream">
        /// The stream where the instance data is read from.
        /// </param>
        void Deserialize(Stream stream);
    }
}
