// -----------------------------------------------------------------------
// <copyright file = "CoplayerInfo.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using System.IO;
    using System.Text;
    using Game.Core.Cloneable;
    using Game.Core.CompactSerialization;
    using Platform.Interfaces;

    /// <summary>
    /// This class holds the information on a coplayer.
    /// </summary>
    [Serializable]
    public sealed class CoplayerInfo : ICompactSerializable, IDeepCloneable
    {
        #region Properties

        /// <summary>
        /// Gets the ID of the coplayer.
        /// </summary>
        public int CoplayerId { get; private set; }

        /// <summary>
        /// Gets the denomination the coplayer runs with.
        /// 0 if no theme is loaded in the coplayer.
        /// </summary>
        public long Denomination { get; private set; }

        /// <summary>
        /// Gets the theme identifier loaded in the coplayer.
        /// Null if no theme is loaded in the coplayer.
        /// </summary>
        public IdentifierToken ThemeIdentifier { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for <see cref="ICompactSerializable"/>.
        /// </summary>
        public CoplayerInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CoplayerInfo"/>.
        /// </summary>
        /// <param name="coplayerId">
        /// The ID of the coplayer.
        /// </param>
        /// <param name="themeIdentifier">
        /// The theme identifier loaded in the coplayer.  Null if no theme is loaded.
        /// </param>
        /// <param name="denomination">
        /// The denomination the coplayer runs with.  0 if no theme is loaded in the coplayer.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="coplayerId"/> is less than 0.
        /// </exception>
        public CoplayerInfo(int coplayerId, IdentifierToken themeIdentifier, long denomination)
        {
            if(coplayerId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(coplayerId), "Coplayer ID cannot be less than 0.");
            }

            CoplayerId = coplayerId;
            ThemeIdentifier = themeIdentifier;
            Denomination = denomination < 0 ? 0 : denomination;
        }

        #endregion

        #region Implementation of ICompactSerializable

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, CoplayerId);
            CompactSerializer.Write(stream, ThemeIdentifier);
            CompactSerializer.Write(stream, Denomination);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            CoplayerId = CompactSerializer.ReadInt(stream);
            ThemeIdentifier = CompactSerializer.ReadSerializable<IdentifierToken>(stream);
            Denomination = CompactSerializer.ReadLong(stream);
        }

        #endregion

        #region Implementation of ICompactSerializable

        /// <inheritdoc />
        public object DeepClone()
        {
            return new CoplayerInfo(CoplayerId, ThemeIdentifier, Denomination);
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("CoplayerInfo -");
            builder.AppendLine("\t CoplayerId: " + CoplayerId);
            builder.AppendLine("\t ThemeIdentifier Hash: " + ThemeIdentifier.GetHashCode());
            builder.AppendLine("\t Denomination: " + Denomination);

            return builder.ToString();
        }

        #endregion
    }
}