//-----------------------------------------------------------------------
// <copyright file = "DeviceIdentifier.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using CSI.Schemas;

    /// <summary>
    /// Data structure which identifies a device in the CSI.
    /// </summary>
    public readonly struct DeviceIdentifier
    {
        /// <summary>
        /// Gets the type of the device.
        /// </summary>
        public DeviceType DeviceType { get; }

        /// <summary>
        /// Gets the id of the device.
        /// </summary>
        public string DeviceId { get; }

        /// <summary>
        /// Construct an instance of the identifier.
        /// </summary>
        /// <param name="deviceType">The type of the device.</param>
        /// <param name="deviceId">The ID of the device.</param>
        public DeviceIdentifier(DeviceType deviceType, string deviceId) : this()
        {
            DeviceType = deviceType;
            DeviceId = deviceId;
        }

        /// <summary>
        /// Overload Equals for enhanced performance.
        /// </summary>
        /// <param name="rightHand">The right hand object for the equality check.</param>
        /// <returns>True if the right hand object equals to this object.  False otherwise.</returns>
        private bool Equals(DeviceIdentifier rightHand)
        {
            return DeviceType == rightHand.DeviceType && DeviceId == rightHand.DeviceId;
        }

        #region Overrides

        /// <summary>
        /// Override value type's implementation for better performance.
        /// </summary>
        /// <param name="rightHand">The right hand object for the equality check.</param>
        /// <returns>True if the right hand object equals to this object.  False otherwise.</returns>
        public override bool Equals(object rightHand)
        {
            var result = false;

            if(rightHand is DeviceIdentifier hand)
            {
                result = Equals(hand);
            }

            return result;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <returns>The hash code generated.</returns>
        public override int GetHashCode()
        {
            var hash = (int)DeviceType << 24;

            if(DeviceId != null)
            {
                //Equivalent strings return equivalent hash codes.
                hash ^= DeviceId.GetHashCode();
            }

            return hash;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are considered equal.  False otherwise.</returns>
        public static bool operator ==(DeviceIdentifier left, DeviceIdentifier right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are not considered equal.  False otherwise.</returns>
        public static bool operator !=(DeviceIdentifier left, DeviceIdentifier right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return $"Device {DeviceType} / {DeviceId}";
        }

        #endregion
    }
}
