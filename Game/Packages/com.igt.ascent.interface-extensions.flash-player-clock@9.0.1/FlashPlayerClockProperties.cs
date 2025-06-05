// -----------------------------------------------------------------------
// <copyright file = "FlashPlayerClockProperties.cs" company = "IGT">
//     Copyright (c) 2022 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.FlashPlayerClock
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using Cloneable;
    using CompactSerialization;

    /// <summary>
    /// Container class for Flash Player Clock properties.
    /// </summary>
    [Serializable]
    public class FlashPlayerClockProperties : ICompactSerializable, IEquatable<FlashPlayerClockProperties>, IDeepCloneable
    {
        #region Public Properties

        /// <summary>
        /// Get if the Player Clock Session is active.
        /// </summary>
        public bool PlayerClockSessionActive { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new <see cref="FlashPlayerClockProperties"/>. Used for CompactSerialization.
        /// </summary>
        public FlashPlayerClockProperties()
        {
        }

        /// <summary>
        /// Instantiates a new <see cref="FlashPlayerClockProperties"/>.
        /// </summary>
        /// <param name="playerClockSessionActive">Flag indicating of the Player Clock Session is active.</param>
        public FlashPlayerClockProperties(bool playerClockSessionActive)
        {
            PlayerClockSessionActive = playerClockSessionActive;
        }

        #endregion

        #region ICompactSerializable

        /// <inheritDoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, PlayerClockSessionActive);
        }

        /// <inheritDoc/>
        public void Deserialize(Stream stream)
        {
            PlayerClockSessionActive = CompactSerializer.ReadBool(stream);
        }

        #endregion

        #region IEquatable<FlashPlayerClockProperties>

        /// <inheritDoc/>
        public bool Equals(FlashPlayerClockProperties other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }

            if(ReferenceEquals(this, other))
            {
                return true;
            }

            return PlayerClockSessionActive == other.PlayerClockSessionActive;
        }

        /// <inheritDoc/>
        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
            {
                return false;
            }

            if(ReferenceEquals(this, obj))
            {
                return true;
            }

            if(obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((FlashPlayerClockProperties)obj);
        }

        /// <inheritDoc/>
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            return PlayerClockSessionActive.GetHashCode();
        }

        /// <inheritDoc/>
        public static bool operator ==(FlashPlayerClockProperties left, FlashPlayerClockProperties right)
        {
            return Equals(left, right);
        }

        /// <inheritDoc/>
        public static bool operator !=(FlashPlayerClockProperties left, FlashPlayerClockProperties right)
        {
            return !Equals(left, right);
        }

        #endregion

        #region IDeepCloneable

        /// <inheritDoc/>
        public object DeepClone()
        {
            return new FlashPlayerClockProperties(PlayerClockSessionActive);
        }

        #endregion
    }
}