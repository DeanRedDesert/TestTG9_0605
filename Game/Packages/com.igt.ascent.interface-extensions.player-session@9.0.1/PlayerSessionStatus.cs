// -----------------------------------------------------------------------
// <copyright file = "PlayerSessionStatus.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSession
{
    using System;
    using System.IO;
    using System.Text;
    using CompactSerialization;

    /// <summary>
    /// This class contains information of the current player session.
    /// </summary>
    [Serializable]
    public class PlayerSessionStatus : ICompactSerializable
    {
        /// <summary>
        /// Gets the flag indicating whether there is a player session active.
        /// True means there is an active session. False means no active session.
        /// </summary>
        public bool SessionActive { get; private set; }

        /// <summary>
        /// Gets the session start time of DateTime type.
        /// </summary>
        /// <remarks>
        /// This value is valid only when <see cref="SessionActive"/> is true.
        /// </remarks>
        public DateTime SessionStartTime { get; private set; }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>
        /// A string describing the object.
        /// </returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("Player Session Status -");
            builder.AppendLine($"\t SessionActive({SessionActive})");
            if(SessionActive)
            {
                builder.AppendLine($"\t SessionStartTime({SessionStartTime})");
            }

            return builder.ToString();
        }

        #region Constructors

        /// <summary>
        /// Constructs an instance of the <see cref="PlayerSessionStatus"/>.
        /// </summary>
        /// <remarks>
        /// Types implementing ICompactSerializable must have a public parameter-less constructor.
        /// </remarks>
        public PlayerSessionStatus()
            : this(false, new DateTime())
        {
        }

        /// <summary>
        /// Constructs an instance of the <see cref="PlayerSessionStatus"/>.
        /// </summary>
        /// <param name="sessionActive">
        /// The flag indicating whether there is a player session active or not.
        /// </param>
        /// <param name="sessionStartTime">
        /// The session start time.
        /// </param>
        public PlayerSessionStatus(bool sessionActive, DateTime sessionStartTime)
        {
            SessionActive = sessionActive;
            SessionStartTime = sessionStartTime;
        }

        #endregion

        #region ICompactSerializable Implementation

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Serialize(stream, SessionActive);
            CompactSerializer.Serialize(stream, SessionStartTime.Ticks);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            SessionActive = CompactSerializer.ReadBool(stream);
            SessionStartTime = new DateTime(CompactSerializer.ReadLong(stream));
        }

        #endregion
    }
}
