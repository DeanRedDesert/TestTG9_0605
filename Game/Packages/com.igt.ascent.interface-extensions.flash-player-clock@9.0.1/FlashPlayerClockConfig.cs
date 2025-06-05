// -----------------------------------------------------------------------
// <copyright file = "FlashPlayerClockConfig.cs" company = "IGT">
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
    /// Container class for Flash Player Clock config items.
    /// </summary>
    [Serializable]
    public class FlashPlayerClockConfig : ICompactSerializable, IEquatable<FlashPlayerClockConfig>, IDeepCloneable
    {
        #region Public Properties

        /// <summary>
        /// True if the feature that is to flash the player-clock is enabled.
        /// If enabled the clock is to flash at the start of the player-clock session
        /// and then at least every <see cref="MinutesBetweenSequences"/> while the player-clock session
        /// is active (except when the player UI is suspended). The player-clock is NOT to
        /// flash when the UI is suspended. If the player-clock session is active when the
        /// player facing display is to be displayed normally (i.e. NOT suspended, AKA DisplayAsNormal),
        /// the client is to immediately execute a flashing sequence. The client is responsible for
        /// timing such that a flash sequence occurs at least every <see cref="MinutesBetweenSequences"/>
        /// thereafter (i.e. while the UI not suspended).
        /// </summary>
        public bool FlashPlayerClockEnabled { get; private set; }

        /// <summary>
        /// The number of times the clock display is to be flashed per sequence.
        /// For example, if this value is 5 then the clock should be highlighted 5 times over
        /// <see cref="FlashSequenceLengthMilliseconds"/>.
        /// </summary>
        public uint NumberOfFlashesPerSequence { get; private set; }

        /// <summary>
        /// The length of time the clock display is to flash.
        /// For example, if this value is 3000 then the clock display should be flashed
        /// <see cref="NumberOfFlashesPerSequence"/> over a total of 3 seconds.
        /// </summary>
        public uint FlashSequenceLengthMilliseconds { get; private set; }

        /// <summary>
        /// The minimum number of minutes between executing a flash sequence while in a player-clock session.
        /// </summary>
        public uint MinutesBetweenSequences { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new <see cref="FlashPlayerClockConfig"/>. Used for CompactSerialization.
        /// </summary>
        public FlashPlayerClockConfig()
        {
        }

        /// <summary>
        /// Instantiates a new <see cref="FlashPlayerClockConfig"/>.
        /// </summary>
        /// <param name="flashPlayerClockEnabled">Flag indicating of the flash player clock is enabled.</param>
        /// <param name="numberOfFlashesPerSequence">The number of flashes per sequence.</param>
        /// <param name="flashSequenceLengthMilliseconds">The length of time between flash sequence, in milliseconds.</param>
        /// <param name="minutesBetweenSequences">Minutes between each flash sequence.</param>
        public FlashPlayerClockConfig(bool flashPlayerClockEnabled, uint numberOfFlashesPerSequence, 
            uint flashSequenceLengthMilliseconds, uint minutesBetweenSequences)
        {
            FlashPlayerClockEnabled = flashPlayerClockEnabled;
            NumberOfFlashesPerSequence = numberOfFlashesPerSequence;
            FlashSequenceLengthMilliseconds = flashSequenceLengthMilliseconds;
            MinutesBetweenSequences = minutesBetweenSequences;
        }

        #endregion

        #region ICompactSerializable

        /// <inheritDoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, FlashPlayerClockEnabled);
            CompactSerializer.Write(stream, NumberOfFlashesPerSequence);
            CompactSerializer.Write(stream, FlashSequenceLengthMilliseconds);
            CompactSerializer.Write(stream, MinutesBetweenSequences);
        }

        /// <inheritDoc/>
        public void Deserialize(Stream stream)
        {
            FlashPlayerClockEnabled = CompactSerializer.ReadBool(stream);
            NumberOfFlashesPerSequence = CompactSerializer.ReadUint(stream);
            FlashSequenceLengthMilliseconds = CompactSerializer.ReadUint(stream);
            MinutesBetweenSequences = CompactSerializer.ReadUint(stream);
        }

        #endregion

        #region IEquatable<FlashPlayerClockConfig>

        /// <inheritDoc/>
        public bool Equals(FlashPlayerClockConfig other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }

            if(ReferenceEquals(this, other))
            {
                return true;
            }

            return FlashPlayerClockEnabled == other.FlashPlayerClockEnabled &&
                   NumberOfFlashesPerSequence == other.NumberOfFlashesPerSequence &&
                   FlashSequenceLengthMilliseconds == other.FlashSequenceLengthMilliseconds &&
                   MinutesBetweenSequences == other.MinutesBetweenSequences;
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

            return Equals((FlashPlayerClockConfig)obj);
        }

        /// <inheritDoc/>
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = FlashPlayerClockEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ NumberOfFlashesPerSequence.GetHashCode();
                hashCode = (hashCode * 397) ^ FlashSequenceLengthMilliseconds.GetHashCode();
                hashCode = (hashCode * 397) ^ MinutesBetweenSequences.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritDoc/>
        public static bool operator ==(FlashPlayerClockConfig left, FlashPlayerClockConfig right)
        {
            return Equals(left, right);
        }

        /// <inheritDoc/>
        public static bool operator !=(FlashPlayerClockConfig left, FlashPlayerClockConfig right)
        {
            return !Equals(left, right);
        }

        #endregion

        #region IDeepCloneable

        /// <inheritDoc/>
        public object DeepClone()
        {
            return new FlashPlayerClockConfig(FlashPlayerClockEnabled, NumberOfFlashesPerSequence,
                FlashSequenceLengthMilliseconds, MinutesBetweenSequences);
        }

        #endregion
    }
}