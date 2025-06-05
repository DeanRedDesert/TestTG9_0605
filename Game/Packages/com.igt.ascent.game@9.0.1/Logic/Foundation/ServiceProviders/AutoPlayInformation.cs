//-----------------------------------------------------------------------
// <copyright file = "AutoPlayInformation.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// TODO: http://cspjira:8080/jira/browse/AS-8697
// ReSharper disable NonReadonlyMemberInGetHashCode
namespace IGT.Game.Core.Logic.Foundation.ServiceProviders
{
    using System;
    using System.IO;
    using Cloneable;
    using CompactSerialization;

    /// <summary>
    /// This class gives all the information needed to control the auto play.
    /// </summary>
    /// <remarks> This class is used for the payload in the game service.</remarks>
    [Serializable]
    public class AutoPlayInformation : ICompactSerializable, IEquatable<AutoPlayInformation>,
        IDeepCloneable
    {
        /// <summary>
        /// This flag indicates whether the auto play can be started by the player.
        /// </summary>
        public bool CanPlayerInitiate { get; set; }

        /// <summary>
        /// This flag indicates if there is enough money to commit current bet.
        /// </summary>
        public bool CanCommitBet { get; set; }

        /// <summary>
        /// This flag indicates if the auto play has been started or not.
        /// </summary>
        public bool IsOn { get; set; }

        /// <summary>
        /// This flag indicates if the auto play confirmation is required.
        /// </summary>
        public bool IsConfirmationRequired { get; set; }

        /// <summary>
        /// This flag indicates if increasing the game speed during auto play is allowed.
        /// If it is null the Foundation can not provide the information.
        /// </summary>
        public bool? IsSpeedIncreaseAllowed { get; set; }

        /// <inheritDoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as AutoPlayInformation);
        }

        /// <inheritDoc/>
        public bool Equals(AutoPlayInformation autoPlayInformation)
        {
            return this == autoPlayInformation;
        }

        /// <inheritDoc/>
        public override int GetHashCode()
        {
            var hash = 13;
            hash = hash * 7 + CanPlayerInitiate.GetHashCode();
            hash = hash * 7 + CanCommitBet.GetHashCode();
            hash = hash * 7 + IsOn.GetHashCode();
            hash = hash * 7 + IsConfirmationRequired.GetHashCode();
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            hash = hash * 7 + IsSpeedIncreaseAllowed.HasValue.GetHashCode();
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            if(IsSpeedIncreaseAllowed.HasValue)
            {
                // ReSharper disable once NonReadonlyMemberInGetHashCode
                hash = hash * 7 + IsSpeedIncreaseAllowed.Value.GetHashCode();
            }
            return hash;
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="left">The left side object to compare.</param>
        /// <param name="right">The right side object to compare.</param>
        /// <returns>True if the objects are equal.</returns>
        public static bool operator ==(AutoPlayInformation left, AutoPlayInformation right)
        {

            if(ReferenceEquals(left, null) && ReferenceEquals(right, null))
            {
                return true;
            }
            
            if(ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                // If only one of the instances is null, return false.
                return false;
            }
            
            if(ReferenceEquals(left, right))
            {
                // The objects are the same instance.
                return true;
            }

            return left.CanPlayerInitiate == right.CanPlayerInitiate
                   && left.CanCommitBet == right.CanCommitBet
                   && left.IsOn == right.IsOn
                   && left.IsConfirmationRequired == right.IsConfirmationRequired
                   && left.IsSpeedIncreaseAllowed == right.IsSpeedIncreaseAllowed;
        }

        /// <summary>
        /// Tests for inequality.
        /// </summary>
        /// <param name="left">The left side object to compare.</param>
        /// <param name="right">The right side object to compare.</param>
        /// <returns>True if the objects are not equal.</returns>
        public static bool operator !=(AutoPlayInformation left, AutoPlayInformation right)
        {
            return !(left == right);
        }

        /// <inheritDoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, CanPlayerInitiate);
            CompactSerializer.Write(stream, CanCommitBet);
            CompactSerializer.Write(stream, IsOn);
            CompactSerializer.Write(stream, IsConfirmationRequired);
            CompactSerializer.Write(stream, IsSpeedIncreaseAllowed);
        }

        /// <inheritDoc/>
        public void Deserialize(Stream stream)
        {
            CanPlayerInitiate = CompactSerializer.ReadBool(stream);
            CanCommitBet = CompactSerializer.ReadBool(stream);
            IsOn = CompactSerializer.ReadBool(stream);
            IsConfirmationRequired = CompactSerializer.ReadBool(stream);
            IsSpeedIncreaseAllowed = CompactSerializer.ReadNullable<bool>(stream);
        }

        /// <inheritDoc/>
       public object DeepClone()
        {
            var copy = new AutoPlayInformation
                       {
                           CanPlayerInitiate = CanPlayerInitiate,
                           CanCommitBet = CanCommitBet,
                           IsOn = IsOn,
                           IsConfirmationRequired = IsConfirmationRequired,
                           IsSpeedIncreaseAllowed = IsSpeedIncreaseAllowed
                       };
            return copy;
        }
    }
}
