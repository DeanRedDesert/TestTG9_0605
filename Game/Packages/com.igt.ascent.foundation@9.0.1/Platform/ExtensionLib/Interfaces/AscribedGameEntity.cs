//-----------------------------------------------------------------------
// <copyright file = "AscribedGameEntity.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionLib.Interfaces
{
    using System;

    /// <summary>
    /// This class defines the ascribed game entity for extension.
    /// </summary>
    [Serializable]
    public sealed class AscribedGameEntity : IEquatable<AscribedGameEntity>
    {
        /// <summary>
        /// Gets the ascribed game type.
        /// </summary>
        public AscribedGameType AscribedGameType { get; }

        /// <summary>
        /// Gets the ascribed game identifier.
        /// </summary>
        public string AscribedGameIdentifier { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="AscribedGameEntity"/>.
        /// </summary>
        /// <param name="ascribedGameType">The ascribed game type.</param>
        /// <param name="ascribedGameIdentifier">The ascribed game identifier.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="ascribedGameIdentifier"/> is null or empty.
        /// </exception>
        public AscribedGameEntity(AscribedGameType ascribedGameType, string ascribedGameIdentifier)
        {
            if(string.IsNullOrEmpty(ascribedGameIdentifier))
            {
                throw new ArgumentNullException(nameof(ascribedGameIdentifier));
            }

            AscribedGameType = ascribedGameType;
            AscribedGameIdentifier = ascribedGameIdentifier;
        }

        /// <summary>
        /// Overrides base implementation to provide better information.
        /// </summary>
        /// <returns>
        /// A string describing the object.
        /// </returns>
        public override string ToString()
        {
            return $"AscribedGame: Type({AscribedGameType})/Identifier({AscribedGameIdentifier})";
        }

        /// <inheritdoc />
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

            return obj.GetType() == GetType() && Equals((AscribedGameEntity)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return AscribedGameIdentifier.GetHashCode() ^ AscribedGameType.GetHashCode();
        }

        /// <summary>
        /// Overloads the operator ==.
        /// </summary>
        public static bool operator ==(AscribedGameEntity left, AscribedGameEntity right)
        {
            if(ReferenceEquals(left, right))
            {
                return true;
            }
            if(ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }
            return left.Equals(right);
        }

        /// <summary>
        /// Overloads the operator !=.
        /// </summary>
        public static bool operator !=(AscribedGameEntity left, AscribedGameEntity right)
        {
            return !(left == right);
        }

        /// <inheritdoc />
        public bool Equals(AscribedGameEntity other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            if(ReferenceEquals(this, other))
            {
                return true;
            }

            return AscribedGameIdentifier == other.AscribedGameIdentifier && AscribedGameType == other.AscribedGameType;
        }
    }
}