// -----------------------------------------------------------------------
// <copyright file = "BonusSoaaSettings.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;
    using System.IO;
    using Game.Core.Cloneable;
    using Game.Core.CompactSerialization;

    /// <summary>
    /// The settings for the Single Option Auto Advance (SOAA) feature in a Bonus/Game Feature.
    ///
    /// The settings are applicable only if there is a single button/player option presented in a bonus, such as
    /// a button saying “Press Start to enter bonus”.
    /// 
    /// The settings are not applicable if there is no player interaction is needed, or there are multiple choices
    /// for the player to pick.  Examples are a free spin bonus playing all free spins without player input, or
    /// a pick bonus asking the player to pick a prize.
    /// </summary>
    /// <remarks>
    /// When the game has a single button/option presented in a bonus...
    /// If <see cref="IsAllowed"/> is false, the game must wait indefinitely until the player presses the button.
    /// If <see cref="IsAllowed"/> is true, and the player has not pressed the button in <see cref="MinDelaySeconds"/>,
    /// then the game can either move on as if the button had been pressed, or keep waiting for a longer time or
    /// till the player presses the button.
    /// </remarks>
    public sealed class BonusSoaaSettings : ICompactSerializable, IDeepCloneable
    {
        #region Properties

        /// <summary>
        /// Gets the flag indicating whether the game is allowed to auto advance
        /// a single player option presented in a bonus.
        /// </summary>
        public bool IsAllowed { get; private set; }

        /// <summary>
        /// Gets the minimum time (in seconds) the game has to wait for a player interaction
        /// before auto advancing a single option presented in a bonus.
        /// Has valid value only when <see cref="IsAllowed"/> is true.
        /// Will be null if <see cref="IsAllowed"/> is false.
        /// </summary>
        public uint? MinDelaySeconds { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor for <see cref="ICompactSerializable"/>.
        /// </summary>
        public BonusSoaaSettings()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BonusSoaaSettings"/>.
        /// </summary>
        /// <param name="isAllowed">
        /// Whether SOAA in a bonus is allowed.
        /// </param>
        /// <param name="minDelaySeconds">
        /// The minimum seconds the game has to wait before SOAA.
        /// </param>
        public BonusSoaaSettings(bool isAllowed, uint? minDelaySeconds = null)
        {
            switch(isAllowed)
            {
                case true when minDelaySeconds == null:
                {
                    throw new ArgumentException($"{nameof(minDelaySeconds)} cannot be null when {nameof(isAllowed)} is true.");
                }
                case false when minDelaySeconds != null:
                {
                    throw new ArgumentException($"{nameof(minDelaySeconds)} must be null when {nameof(isAllowed)} is false.");
                }
                default:
                {
                    IsAllowed = isAllowed;
                    MinDelaySeconds = minDelaySeconds;
                    break;
                }
            }
        }

        #endregion

        #region Implementation of ICompactSerializable

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, IsAllowed);
            CompactSerializer.Write(stream, MinDelaySeconds);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            IsAllowed = CompactSerializer.ReadBool(stream);
            MinDelaySeconds = CompactSerializer.ReadNullable<uint>(stream);
        }

        #endregion

        #region Implementation of IDeepCloneable

        /// <inheritdoc />
        public object DeepClone()
        {
            return new BonusSoaaSettings(IsAllowed, MinDelaySeconds);
        }

        #endregion
    }
}