//-----------------------------------------------------------------------
// <copyright file = "SynchronousSpinProfile.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    using System;
    using System.IO;
    using System.Text;
    using CompactSerialization;

    /// <summary>
    /// Class which encapsulates the settings for a synchronous spin.
    /// </summary>
    [Serializable]
    public class SynchronousSpinProfile : ICompactSerializable
    {
        /// <summary>
        /// The number of the reel this profile is for.
        /// </summary>
        public byte ReelNumber { get; set; }

        /// <summary>
        /// Gets/Sets the direction the reel is to spin.
        /// </summary>
        public ReelDirection Direction { get; set; }

        /// <summary>
        /// Gets/Sets the spin duration in milliseconds. This time needs to include
        /// the acceleration, constant speed, and deceleration time.
        /// </summary>
        public ushort Duration { get; set; }

        /// <summary>
        /// Gets/Sets the desired stop for the spin. If the stop is specified as
        /// 0xFF then the reel will spin indefinitely. This will override
        /// the Duration portion of the profile.
        /// </summary>
        public byte Stop { get; set; }

        /// <summary>
        /// Construct a profile with the given parameters.
        /// </summary>
        /// <param name="reelNumber">The number of the reel this profile is for.</param>
        /// <param name="reelStop">
        /// The desired stop for the spin. If the stop is specified as 0xFF then the reel will spin indefinitely.
        /// This will override the Duration portion of the profile.
        /// </param>
        /// <param name="duration">
        /// The spin duration in milliseconds. This time needs to include the acceleration, constant speed, and
        /// deceleration time.
        /// </param>
        /// <param name="reelDirection">The direction to spin.</param>
        public SynchronousSpinProfile(byte reelNumber, byte reelStop, ushort duration, ReelDirection reelDirection)
        {
            ReelNumber = reelNumber;
            Stop = reelStop;
            Duration = duration;
            Direction = reelDirection;
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("Synchronous Spin Profile -");
            builder.AppendLine("\t Reel Number = " + ReelNumber);
            builder.AppendLine("\t Direction = " + Direction);
            builder.AppendLine("\t Duration = " + Duration);
            builder.AppendLine("\t Stop = " + Stop);

            return builder.ToString();
        }

        #region ICompactSerializable Members

        /// <summary>
        /// This parameter-less constructor is required by ICompactSerializable
        /// interface, and should not be used for any purpose other than
        /// deserialization.
        /// </summary>
        public SynchronousSpinProfile()
        {
        }

        /// <inheritdoc />
        void ICompactSerializable.Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, ReelNumber);
            CompactSerializer.Write(stream, Stop);
            CompactSerializer.Write(stream, (byte)Direction);
            CompactSerializer.Write(stream, Duration);
        }

        /// <inheritdoc />
        void ICompactSerializable.Deserialize(Stream stream)
        {
            ReelNumber = CompactSerializer.ReadByte(stream);
            Stop = CompactSerializer.ReadByte(stream);
            Direction = (ReelDirection)CompactSerializer.ReadByte(stream);
            Duration = CompactSerializer.ReadUshort(stream);
        }

        #endregion
    }
}
