//-----------------------------------------------------------------------
// <copyright file = "ThemeContext.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// TODO: http://cspjira:8080/jira/browse/AS-8697
// ReSharper disable NonReadonlyMemberInGetHashCode
namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;
    using System.IO;
    using Ascent.Communication.Platform.Interfaces;
    using Game.Core.CompactSerialization;

    /// <summary>
    /// This struct represents a theme context.
    /// </summary>
    [Serializable]
    public struct ThemeContext : ICompactSerializable, IEquatable<ThemeContext>
    {
        /// <summary>
        /// Represents an invalid theme context.
        /// </summary>
        public static readonly ThemeContext Invalid = new ThemeContext();

        /// <summary>
        /// The mode for the new theme context.
        /// </summary>
        public GameMode GameContextMode { get; private set; }

        /// <summary>
        /// The denomination for the context.
        /// </summary>
        public long Denomination { get; private set; }

        /// <summary>
        /// The name of the paytable the theme context is to use.
        /// </summary>
        public string PaytableName { get; private set; }

        /// <summary>
        /// The file which contains the paytable for the theme context.
        /// The file may logically relate to many paytables, so the
        /// paytable file name must be separate from the paytable name.
        /// </summary>
        /// <remarks>
        /// This is the absolute path of the paytable file.
        /// </remarks>
        public string PaytableFileName { get; private set; }

        /// <summary>
        /// Flag indicating that the (play mode) context is a context that 
        /// should be considered new to the player.
        /// </summary>
        public bool NewlySelectedForPlay { get; private set; }

        /// <summary>
        /// The sub-mode for the new theme context.
        /// </summary>
        public GameSubMode GameSubMode { get; private set; }

        /// <summary>
        /// Construct a theme context with given parameters.
        /// </summary>
        /// <param name="gameContextMode">The mode of the game context.</param>
        /// <param name="denomination">The denomination to use in the game context.</param>
        /// <param name="paytableName">The paytable to use in the game context.</param>
        /// <param name="paytableFileName">The paytable file to use in new game context.</param>
        /// <param name="newlySelectedForPlay">Flag indicating that the context(play mode) should be considered
        /// new to the player.</param>
        /// <param name="gameSubMode">The game sub-mode of the game context.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when denomination is less than 1.
        /// </exception>
        public ThemeContext(GameMode gameContextMode,
                            long denomination,
                            string paytableName,
                            string paytableFileName,
                            bool newlySelectedForPlay,
                            GameSubMode gameSubMode = GameSubMode.Standard)
            : this()
        {
            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0");
            }

            GameContextMode = gameContextMode;
            Denomination = denomination;
            PaytableName = paytableName;
            PaytableFileName = paytableFileName;
            NewlySelectedForPlay = newlySelectedForPlay;
            GameSubMode = gameSubMode;
        }

        /// <summary>
        /// Override value type's implementation for better performance.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value;
        /// otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            var result = false;

            if(obj is ThemeContext context)
            {
                result = Equals(context);
            }

            return result;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <returns>The hash code generated.</returns>
        public override int GetHashCode()
        {
            var hash = 23;
            hash = hash * 37 + GameContextMode.GetHashCode();
            hash = hash * 37 + Denomination.GetHashCode();
            if(PaytableName != null)
            {
                hash = hash * 37 + PaytableName.GetHashCode();
            }
            if(PaytableFileName != null)
            {
                hash = hash * 37 + PaytableFileName.GetHashCode();
            }
            hash = hash * 37 + NewlySelectedForPlay.GetHashCode();
            hash = hash * 51 + GameSubMode.GetHashCode();

            return hash;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are considered equal.  False otherwise.</returns>
        public static bool operator ==(ThemeContext left, ThemeContext right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are not considered equal.  False otherwise.</returns>
        public static bool operator !=(ThemeContext left, ThemeContext right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return
                $"{GameContextMode}/{Denomination}/{PaytableName}/{PaytableFileName}/{NewlySelectedForPlay}/{GameSubMode}";
        }

        #region IEquatable<ThemeContext> Members

        /// <summary>
        /// Indicates whether this instance and a specified theme context are equal.
        /// </summary>
        /// <param name="themeContext">Another theme context to compare to.</param>
        /// <returns>true if <paramref name="themeContext"/> and this instance represent the same value;
        /// otherwise, false.</returns>
        public bool Equals(ThemeContext themeContext)
        {
            return GameContextMode == themeContext.GameContextMode &&
                   Denomination == themeContext.Denomination &&
                   PaytableName == themeContext.PaytableName &&
                   PaytableFileName == themeContext.PaytableFileName &&
                   NewlySelectedForPlay == themeContext.NewlySelectedForPlay &&
                   GameSubMode == themeContext.GameSubMode;
        }

        #endregion

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, GameContextMode);
            CompactSerializer.Write(stream, Denomination);
            CompactSerializer.Write(stream, PaytableName);
            CompactSerializer.Write(stream, PaytableFileName);
            CompactSerializer.Write(stream, NewlySelectedForPlay);
            CompactSerializer.Write(stream, GameSubMode);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            GameContextMode = CompactSerializer.ReadEnum<GameMode>(stream);
            Denomination = CompactSerializer.ReadLong(stream);
            PaytableName = CompactSerializer.ReadString(stream);
            PaytableFileName = CompactSerializer.ReadString(stream);
            NewlySelectedForPlay = CompactSerializer.ReadBool(stream);
            GameSubMode = CompactSerializer.ReadEnum<GameSubMode>(stream);
        }

        #endregion
    }
}
