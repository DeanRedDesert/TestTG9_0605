//-----------------------------------------------------------------------
// <copyright file = "SpinProfile.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    using System;
    using System.Text;

    /// <summary>
    /// Class which encapsulates the setting for a spin.
    /// </summary>
    [Serializable]
    public class SpinProfile
    {
        /// <summary>
        /// Constant which indicates that there is no stop and the reels are to spin indefinitely.
        /// </summary>
        public const byte NoStop = 0xFF;

        /// <summary>
        /// Create a default profile.
        /// </summary>
        public SpinProfile()
        {
            Stop = NoStop;
        }

        /// <summary>
        /// Construct a profile with the given parameters. This profile will cause the specified reel to spin
        /// indefinitely.
        /// </summary>
        /// <param name="reelNumber">The number of the reel this profile is for.</param>
        /// <param name="reelDirection">The direction to spin.</param>
        public SpinProfile(byte reelNumber, ReelDirection reelDirection)
        {
            ReelNumber = reelNumber;
            Stop = NoStop;
            Direction = reelDirection;
        }

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
        public SpinProfile(byte reelNumber, byte reelStop, ushort duration, ReelDirection reelDirection)
        {
            ReelNumber = reelNumber;
            Stop = reelStop;
            Duration = duration;
            Direction = reelDirection;
        }

        /// <summary>
        /// Construct a profile with the given parameters. This constructor is for a normal spin with a known stop
        /// and duration.
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
        /// <param name="deceleration">
        /// The deceleration index. Each device supports a list of different deceleration times. This is an index into
        /// the list of supported times. The supported times are contained in the reel feature description.
        /// </param>
        /// <param name="speed">
        /// The speed index. Each device supports a list of different speeds. This is an index into the list of
        /// supported speeds. The supported speeds are contained in the reel feature description.
        /// </param>
        /// <param name="attributes">
        /// Attributes to apply to the spin. If null then the default attributes will be used.
        /// </param>
        public SpinProfile(byte reelNumber, byte reelStop, ushort duration, ReelDirection reelDirection,
                           ushort deceleration, byte speed, SpinAttributes attributes)
        {
            ReelNumber = reelNumber;
            Stop = reelStop;
            Duration = duration;
            Direction = reelDirection;
            Deceleration = deceleration;
            Speed = speed;
            Attributes = attributes;
        }

        /// <summary>
        /// The number of the reel this profile is for.
        /// </summary>
        public byte ReelNumber { get; set; }

        /// <summary>
        /// Gets/Sets the deceleration index. Each device supports a list of
        /// different deceleration times. This is an index into the list of 
        /// supported times. The supported times are contained in the reel
        /// feature description.
        /// </summary>
        public ushort Deceleration { get; set; }

        /// <summary>
        /// Gets/Sets the speed index. Each device supports a list of different speeds.
        /// This is an index into the list of supported speeds. The supported speeds are
        /// contained in the reel feature description.	
        /// </summary>
        public byte Speed { get; set; }

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
        /// Attributes to apply to the spin. If null then the default attributes will be used.
        /// </summary>
        public SpinAttributes Attributes { get; set; }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("Spin Profile -");
            builder.AppendLine("\t Reel Number = " + ReelNumber);
            builder.AppendLine("\t Speed = " + Speed);
            builder.AppendLine("\t Deceleration = " + Deceleration);
            builder.AppendLine("\t Direction = " + Direction);
            builder.AppendLine("\t Duration = " + Duration);
            builder.AppendLine("\t Stop = " + Stop);
            if(Attributes != null)
            {
                builder.AppendLine("\t Attributes = " + Attributes);
            }

            return builder.ToString();
        }
    }
}
