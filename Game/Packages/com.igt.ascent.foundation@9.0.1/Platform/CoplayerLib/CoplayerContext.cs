// -----------------------------------------------------------------------
// <copyright file = "CoplayerContext.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.CoplayerLib
{
    using System;
    using Interfaces;
    using Platform.Interfaces;

    /// <inheritdoc cref="ICoplayerContext"/>
    /// <summary>
    /// A simple implementation of <see cref="ICoplayerContext"/>.
    /// </summary>
    [Serializable]
    internal sealed class CoplayerContext : ICoplayerContext, IEquatable<CoplayerContext>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="CoplayerContext"/> with default, non-null values.
        /// </summary>
        public CoplayerContext()
        {
            MountPoint = string.Empty;
            GameMode = GameMode.Invalid;
            G2SThemeId = string.Empty;
            ThemeTag = string.Empty;
            ThemeTagDataFile = string.Empty;
            Denomination = 1;
            PayvarTag = string.Empty;
            PayvarTagDataFile = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CoplayerContext"/>.
        /// </summary>
        /// <param name="coplayerId">
        /// The unique id of the coplayer.
        /// </param>
        /// <param name="mountPoint">
        /// The mount point of the shell application package.
        /// </param>
        /// <param name="gameMode">
        /// The game mode in which the coplayer is running.
        /// </param>
        /// <param name="g2SThemeId">
        /// The G2SThemeId defined in the theme registry.
        /// </param>
        /// <param name="themeTag">
        /// The TagData defined in the theme registry.
        /// </param>
        /// <param name="themeTagDataFile">
        /// The TagDataFile defined in the theme registry.
        /// </param>
        /// <param name="denomination">
        /// The denomination for the coplayer context.
        /// </param>
        /// <param name="payvarTag">
        /// The TagData defined in the payvar registry.
        /// </param>
        /// <param name="payvarTagDataFile">
        /// TagDataFile defined in the payvar registry.
        /// This is usually the path to paytable file.
        /// </param>
        public CoplayerContext(int coplayerId,
                               string mountPoint, GameMode gameMode,
                               string g2SThemeId, string themeTag, string themeTagDataFile,
                               long denomination, string payvarTag, string payvarTagDataFile)
        {
            CoplayerId = coplayerId;
            MountPoint = mountPoint ?? string.Empty;
            GameMode = gameMode;
            G2SThemeId = g2SThemeId ?? string.Empty;
            ThemeTag = themeTag ?? string.Empty;
            ThemeTagDataFile = themeTagDataFile ?? string.Empty;
            Denomination = denomination;
            PayvarTag = payvarTag ?? string.Empty;
            PayvarTagDataFile = payvarTagDataFile ?? string.Empty;
        }


        /// <summary>
        /// Initializes a new instance of <see cref="CoplayerContext"/> by
        /// an object implementing <see cref="ICoplayerContext"/> interface.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="other"/> is null.
        /// </exception>
        public CoplayerContext(ICoplayerContext other)
        {
            if(other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            CoplayerId = other.CoplayerId;
            MountPoint = other.MountPoint ?? string.Empty;
            GameMode = other.GameMode;
            G2SThemeId = other.G2SThemeId ?? string.Empty;
            ThemeTag = other.ThemeTag ?? string.Empty;
            ThemeTagDataFile = other.ThemeTagDataFile ?? string.Empty;
            Denomination = other.Denomination;
            PayvarTag = other.PayvarTag ?? string.Empty;
            PayvarTagDataFile = other.PayvarTagDataFile ?? string.Empty;
        }

        #endregion

        #region ICoplayerContext Implementation

        /// <inheritdoc/>
        public int CoplayerId { get; }

        /// <inheritdoc/>
        public string MountPoint { get; }

        /// <inheritdoc/>
        public GameMode GameMode { get; }

        /// <inheritdoc/>
        public string G2SThemeId { get; }

        /// <inheritdoc/>
        public string ThemeTag { get; }

        /// <inheritdoc/>
        public string ThemeTagDataFile { get; }

        /// <inheritdoc/>
        public long Denomination { get; }

        /// <inheritdoc/>
        public string PayvarTag { get; }

        /// <inheritdoc/>
        public string PayvarTagDataFile { get; }

        #endregion

        #region Equality Implementation

        // Generated by ReSharper

        /// <inheritdoc/>
        public bool Equals(CoplayerContext other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            if(ReferenceEquals(this, other))
            {
                return true;
            }
            return CoplayerId == other.CoplayerId &&
                   string.Equals(MountPoint, other.MountPoint) &&
                   GameMode == other.GameMode &&
                   string.Equals(G2SThemeId, other.G2SThemeId) &&
                   string.Equals(ThemeTag, other.ThemeTag) &&
                   string.Equals(ThemeTagDataFile, other.ThemeTagDataFile) &&
                   Denomination == other.Denomination &&
                   string.Equals(PayvarTag, other.PayvarTag) &&
                   string.Equals(PayvarTagDataFile, other.PayvarTagDataFile);
        }

        /// <inheritdoc/>
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
            var other = obj as CoplayerContext;

            return other != null && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = CoplayerId;
                hashCode = (hashCode * 397) ^ MountPoint.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)GameMode;
                hashCode = (hashCode * 397) ^ G2SThemeId.GetHashCode();
                hashCode = (hashCode * 397) ^ ThemeTag.GetHashCode();
                hashCode = (hashCode * 397) ^ ThemeTagDataFile.GetHashCode();
                hashCode = (hashCode * 397) ^ Denomination.GetHashCode();
                hashCode = (hashCode * 397) ^ PayvarTag.GetHashCode();
                hashCode = (hashCode * 397) ^ PayvarTagDataFile.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Overloads the operator ==.
        /// </summary>
        public static bool operator ==(CoplayerContext left, CoplayerContext right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Overloads the operator !=.
        /// </summary>
        public static bool operator !=(CoplayerContext left, CoplayerContext right)
        {
            return !Equals(left, right);
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Coplayer({CoplayerId})/GameMode({GameMode})/G2S({G2SThemeId})/PayvarTag({PayvarTag})/PayvarTagFile({PayvarTagDataFile})";
        }

        #endregion
    }
}