//-----------------------------------------------------------------------
// <copyright file = "ReelStatusEventArgs.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// TODO: http://cspjira:8080/jira/browse/AS-8697
// ReSharper disable NonReadonlyMemberInGetHashCode
namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    using System;
    using CSI.Schemas;

    /// <summary>
    /// Event arguments which contain status about reels.
    /// </summary>
    [Serializable]
    public class ReelStatusEventArgs : EventArgs, IEquatable<ReelStatusEventArgs>
    {
        /// <summary>
        /// The device the event is for.
        /// </summary>
        public string FeatureId { get; set; }

        /// <summary>
        /// Reel number the status is associated with.
        /// </summary>
        public byte ReelNumber { get; set; }

        /// <summary>
        /// The status of an individual reel.
        /// </summary>
        public ReelStatus Status { get; set; }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return $"Reel {ReelNumber} on {FeatureId} in status of {Status}";
        }

        #region Equality Operations

        /// <inheritdoc/>
        public override bool Equals(object other)
        {
            var otherStatus = other as ReelStatusEventArgs;

            if(otherStatus == null)
            {
                return false;
            }

            return Equals(otherStatus);
        }

        /// <inheritdoc/>
        public bool Equals(ReelStatusEventArgs other)
        {
            if(other == null)
            {
                return false;
            }

            return other.FeatureId == FeatureId && other.ReelNumber == ReelNumber && other.Status == Status;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return FeatureId.GetHashCode() ^ ReelNumber ^ Status.GetHashCode();
        }

        /// <summary>
        /// Overloaded to match equals.
        /// </summary>
        /// <param name="first">First object for comparison.</param>
        /// <param name="second">Second object for comparison.</param>
        /// <returns>True if the objects are equal.</returns>
        public static bool operator ==(ReelStatusEventArgs first, ReelStatusEventArgs second)
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
        public static bool operator !=(ReelStatusEventArgs first, ReelStatusEventArgs second)
        {
            return !(first == second);
        }

        #endregion
    }
}
