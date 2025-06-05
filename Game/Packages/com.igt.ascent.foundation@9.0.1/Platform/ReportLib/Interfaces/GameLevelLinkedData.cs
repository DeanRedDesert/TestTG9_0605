// -----------------------------------------------------------------------
// <copyright file = "GameLevelLinkedData.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ReportLib.Interfaces
{
    using System;
    using System.IO;
    using System.Text;
    using Game.Core.CompactSerialization;

    /// <summary>
    /// This class is used to specify the associated value of game-level.
    /// </summary>
    [Serializable]
    public class GameLevelLinkedData : ICompactSerializable
    {
        #region Read-Only Properties

        /// <summary>
        /// Gets the zero based game-level index.
        /// </summary>
        public uint GameLevelIndex { get; private set; }

        /// <summary>
        /// Gets the value of a game-level when its linked status is link-up.
        /// </summary>
        public GameLevelLinkUpValue GameLevelLinkUpValue { get; private set; }

        /// <summary>
        /// Gets the value of a game-level when its linked status is link-down.
        /// </summary>
        public GameLevelLinkDownValue GameLevelLinkDownValue { get; private set; }

        /// <summary>
        /// Gets the status of whether the <see cref="GameLevelIndex"/> is linked or not.
        /// </summary>
        public bool IsLinked => GameLevelLinkUpValue != null || GameLevelLinkDownValue != null;

        /// <summary>
        /// Gets the linked status.
        /// </summary>
        public bool IsLinkUp => GameLevelLinkUpValue != null;

        #endregion

        #region Contructors

        /// <summary>
        /// Constructs an instance of <see cref="GameLevelLinkedData"/>.
        /// </summary>
        /// <remarks>
        /// Types implementing <see cref="ICompactSerializable"/> must have a public
        /// parameter-less constructor.
        /// </remarks>
        public GameLevelLinkedData() : this(0)
        {
        }

        /// <summary>
        /// Constructs an instance of <see cref="GameLevelLinkedData"/> for a specific game level that is not linked.
        /// </summary>
        /// <param name="gameLevelIndex">The zero-based game-level index that is not linked.</param>
        /// <remarks>
        /// If the game-level is unlinked, both the <see cref="GameLevelLinkUpValue"/>
        /// and the <see cref="GameLevelLinkDownValue"/> are null.
        /// </remarks>
        public GameLevelLinkedData(uint gameLevelIndex)
        {
            GameLevelIndex = gameLevelIndex;
            GameLevelLinkUpValue = null;
            GameLevelLinkDownValue = null;
        }

        /// <summary>
        /// Constructs an instance of <see cref="GameLevelLinkedData"/> for a specific
        /// game level that is linked up.
        /// </summary>
        /// <param name="gameLevelIndex">The zero-based game-level index that is linked up.</param>
        /// <param name="gameLevelLinkUpValue">
        /// The value of the specified game level when it is linked up.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="gameLevelLinkUpValue"/> is null.
        /// </exception>
        /// <remarks>
        /// When a game-level is linked and its status is link-up, the instance of
        /// the <see cref="GameLevelLinkUpValue"/> should be used.
        /// </remarks>
        public GameLevelLinkedData(uint gameLevelIndex, GameLevelLinkUpValue gameLevelLinkUpValue)
        {
            GameLevelIndex = gameLevelIndex;
            GameLevelLinkUpValue = gameLevelLinkUpValue ?? throw new ArgumentNullException(nameof(gameLevelLinkUpValue));
            GameLevelLinkDownValue = null;
        }

        /// <summary>
        /// Constructs an instance of <see cref="GameLevelLinkedData"/> for a specific
        /// game level that is linked down.
        /// </summary>
        /// <param name="gameLevelIndex">The zero-based game-level index that is linked down.</param>
        /// <param name="gameLevelLinkDownValue">
        /// The value of the specified game level when it is linked down.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="gameLevelLinkDownValue"/> is null.
        /// </exception>
        /// <remarks>
        /// When a game-level is linked and its status is link-down, the instance of
        /// the <see cref="GameLevelLinkDownValue"/> should be used.
        /// </remarks>
        public GameLevelLinkedData(uint gameLevelIndex, GameLevelLinkDownValue gameLevelLinkDownValue)
        {
            GameLevelIndex = gameLevelIndex;
            GameLevelLinkUpValue = null;
            GameLevelLinkDownValue = gameLevelLinkDownValue ?? throw new ArgumentNullException(nameof(gameLevelLinkDownValue));
        }

        #endregion

        #region ICompactSerializable Members

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, GameLevelIndex);
            CompactSerializer.Write(stream, GameLevelLinkUpValue);
            CompactSerializer.Write(stream, GameLevelLinkDownValue);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            GameLevelIndex = CompactSerializer.ReadUint(stream);
            GameLevelLinkUpValue = CompactSerializer.ReadSerializable<GameLevelLinkUpValue>(stream);
            GameLevelLinkDownValue = CompactSerializer.ReadSerializable<GameLevelLinkDownValue>(stream);
        }

        #endregion

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Game Level({GameLevelIndex})");
            builder.AppendLine($"Linked({IsLinked})");
            if(IsLinked)
            {
                builder.AppendLine($"Linked Status({IsLinkUp})");
                builder.AppendLine(IsLinkUp ? GameLevelLinkUpValue.ToString() : GameLevelLinkDownValue.ToString());
            }

            return builder.ToString();
        }
    }
}
