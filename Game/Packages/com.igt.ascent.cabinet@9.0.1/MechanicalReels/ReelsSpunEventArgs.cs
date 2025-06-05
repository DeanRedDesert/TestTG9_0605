//-----------------------------------------------------------------------
// <copyright file = "ReelsSpunEventArgs.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// TODO: http://cspjira:8080/jira/browse/AS-8697
// ReSharper disable NonReadonlyMemberInGetHashCode
namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    using System;

    /// <summary>
    /// Event arguments which contain state information about the set of reels affected
    /// by the current spin command.
    /// </summary>
    public class ReelsSpunEventArgs : EventArgs, IEquatable<ReelsSpunEventArgs>
    {
        /// <summary>
        /// The device the event is for.
        /// </summary>
        public string FeatureId { get; set; }

        /// <summary>
        /// The current state of the affected reels.
        /// </summary>
        public ReelsSpunState ReelsSpunState { get; set; }

        #region Constructor

        /// <summary>
        /// Constructs an instance of <see cref="ReelStatusEventArgs"/>.
        /// </summary>
        public ReelsSpunEventArgs()
        {
            ReelsSpunState = ReelsSpunState.AllStopped;
        }

        #endregion

        #region Equality Operations

        /// <inheritdoc/>
        public override bool Equals(object other)
        {
            var otherStatus = other as ReelsSpunEventArgs;

            if(otherStatus == null)
            {
                return false;
            }

            return Equals(otherStatus);
        }

        /// <inheritdoc/>
        public bool Equals(ReelsSpunEventArgs other)
        {
            if(other == null)
            {
                return false;
            }

            return other.FeatureId == FeatureId &&
                   other.ReelsSpunState == ReelsSpunState;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return FeatureId.GetHashCode() ^
                   ReelsSpunState.GetHashCode();
        }

        /// <summary>
        /// Overloaded to match equals.
        /// </summary>
        /// <param name="first">First object for comparison.</param>
        /// <param name="second">Second object for comparison.</param>
        /// <returns>True if the objects are equal.</returns>
        public static bool operator ==(ReelsSpunEventArgs first, ReelsSpunEventArgs second)
        {
            if(ReferenceEquals(first, second))
            {
                return true;
            }

            if((object)first == null || (object)second == null)
            {
                return false;
            }

            return first.Equals(second);
        }

        /// <summary>
        /// Overloaded to match equals.
        /// </summary>
        /// <param name="first">First object for comparison.</param>
        /// <param name="second">Second object for comparison.</param>
        /// <returns>True if the objects are not equal.</returns>
        public static bool operator !=(ReelsSpunEventArgs first, ReelsSpunEventArgs second)
        {
            return !(first == second);
        }

        #endregion
    }
}
