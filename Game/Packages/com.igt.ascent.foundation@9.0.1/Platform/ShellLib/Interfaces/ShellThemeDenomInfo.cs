// -----------------------------------------------------------------------
// <copyright file = "ShellThemeDenomInfo.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using System.IO;
    using Game.Core.Cloneable;
    using Game.Core.CompactSerialization;

    /// <summary>
    /// Contains information about a shell's theme denomination.
    /// </summary>
    [Serializable]
    public class ShellThemeDenomInfo : IDeepCloneable, ICompactSerializable
    {
        /// <summary>
        /// The value of the denom in base units.
        /// </summary>
        public long Denom { get; private set; }

        /// <summary>
        /// Specifies if the denom belongs to a progressive.
        /// </summary>
        public bool IsProgressiveDenom { get; private set; }

        #region Constructor

        /// <summary>
        /// Default constructor for <see cref="ICompactSerializable"/>.
        /// </summary>
        public ShellThemeDenomInfo()
        {
            Denom = 1;
        }

        /// <summary>
        /// Constructor for creating a <see cref="ShellThemeDenomInfo"/>.
        /// </summary>
        /// <param name="denom">Value of the denom in base units.</param>
        /// <param name="isProgressiveDenom">If it belongs to a progressive.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="denom"/> is less than 1.
        /// </exception>
        public ShellThemeDenomInfo(long denom, bool isProgressiveDenom)
        {
            if(denom < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(denom), "Denomination cannot be less than 1.");
            }

            Denom = denom;
            IsProgressiveDenom = isProgressiveDenom;
        }

        #endregion

        #region IDeepClonable Implementation

        /// <inheritdoc />
        public virtual object DeepClone()
        {
            return new ShellThemeDenomInfo(Denom, IsProgressiveDenom);
        }

        #endregion

        #region ICompactSerializable Implementation

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, Denom);
            CompactSerializer.Write(stream, IsProgressiveDenom);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            Denom = CompactSerializer.ReadLong(stream);
            IsProgressiveDenom = CompactSerializer.ReadBool(stream);
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            return Denom + (IsProgressiveDenom ? "(P)" : string.Empty);
        }

        #endregion
    }
}