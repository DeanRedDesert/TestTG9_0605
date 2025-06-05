//-----------------------------------------------------------------------
// <copyright file = "ProgressiveLink.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// TODO: http://cspjira:8080/jira/browse/AS-8697
// ReSharper disable NonReadonlyMemberInGetHashCode
namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System;

    /// <summary>
    /// Class that represents a progressive link setup.
    /// </summary>
    [Serializable]
    public class ProgressiveLink : IEquatable<ProgressiveLink>
    {
        /// <summary>
        /// Get and set the game level that is mapped
        /// to a controller level.
        /// </summary>
        public int GameLevel { get; set; }

        /// <summary>
        /// Get and set the controller's name to which
        /// the game level is linked.
        /// </summary>
        public string ControllerName { get; set; }

        /// <summary>
        /// Get and set the controller level to which
        /// the game level is mapped.
        /// </summary>
        public int ControllerLevel { get; set; }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = GameLevel.GetHashCode() ^ ControllerLevel.GetHashCode();
            
            if(ControllerName != null)
            {
                hash ^= ControllerName.GetHashCode();
            }

            return hash;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var equals = false;

            if(ReferenceEquals(this, obj))
            {
                equals = true;
            }
            else
            {
                if(obj is ProgressiveLink linkObj)
                {
                    equals = Equals(linkObj);
                }
            }

            return equals;
        }

        /// <summary>
        /// Determines if two ProgressiveLink instances are the same.
        /// </summary>
        /// <param name="other">The instance to compare with.</param>
        /// <returns>True if both instances are equal.</returns>
        public bool Equals(ProgressiveLink other)
        {
            return other != null && (ReferenceEquals(this, other) ||
                (GameLevel == other.GameLevel && ControllerName == other.ControllerName
                    && ControllerLevel == other.ControllerLevel));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Game Level {GameLevel} is linked to {ControllerName} Controller Level {ControllerLevel}";
        }
    }
}
