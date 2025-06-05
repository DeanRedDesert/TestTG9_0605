//-----------------------------------------------------------------------
// <copyright file = "ProgressiveBroadcastData.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// TODO: http://cspjira:8080/jira/browse/AS-8697
// ReSharper disable NonReadonlyMemberInGetHashCode
namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;
    using System.IO;
    using Game.Core.Cloneable;
    using Game.Core.CompactSerialization;
    using Platform.Interfaces;

    /// <summary>
    /// Class that represents the information of a progressive level being
    /// broadcast-ed from the progressive controller to the presentation.
    /// </summary>
    [Serializable]
    public class ProgressiveBroadcastData : IEquatable<ProgressiveBroadcastData>, ICompactSerializable,
                                            IDeepCloneable, IProgressiveBroadcastData
    {
        /// <summary>
        /// Amount to lock the given progressive at, in base units.
        /// </summary>
        public long? LockAmount { get; protected set; }

        /// <summary>
        /// Actual monetary amount for the progressive level, in base units.
        /// This property does not regard the progressive lock amount. 
        /// </summary>
        public long ActualAmount { get; protected set; }

        /// <summary>
        /// Flag indicating if the progressive is locked.
        /// </summary>
        public bool IsLocked => LockAmount != null;

        /// <summary>
        /// Arbitrary value to use when serializing a null value.
        /// </summary>
        private const long NullSerializationValue = -1;

        /// <summary>
        /// Default constructor for ProgressiveBroadcastData.
        /// </summary>
        public ProgressiveBroadcastData() : this(0, "")
        {
        }

        /// <summary>
        /// Constructor taking parameters for monetary amount, prize string, and lock value.
        /// </summary>
        /// <param name="amountValue">Progressive monetary amount, in base units.</param>
        /// <param name="prizeString">Progressive prize string.</param>
        /// <param name="lockValue">Progressive locked monetary amount, in base units.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="lockValue"/> is a negative value</exception>
        public ProgressiveBroadcastData(long amountValue, string prizeString, long? lockValue = null)
        {
            if(lockValue < 0)
            {
                throw new ArgumentException(
                    $"Progressive amounts cannot be locked to {lockValue} because it is a negative value.", nameof(lockValue));
            }

            ActualAmount = amountValue;
            PrizeString = prizeString ?? string.Empty;
            LockAmount = lockValue;
        }

        /// <summary>
        /// Locks the progressive at a given value.
        /// </summary>
        /// <devdoc>
        /// The progressive cannot be locked to a negative value, because there is a chance it could overlap with the 
        /// arbitrary nullable serialization negative value.
        /// </devdoc>
        /// <param name="value">
        /// Value to lock the progressive at, in base units.
        /// Passing null as a value will unlock the progressive.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="value"/> is a negative value.
        /// </exception>
        public virtual void Lock(long? value)
        {
            if(value < 0)
            {
                throw new ArgumentException(
                    $"Progressive amounts cannot be locked to {value} because it is a negative value.", nameof(value));
            }

            LockAmount = value;
        }

        /// <summary>
        /// Updates the current progressive data values.
        /// </summary>
        /// <param name="amountValue">The new progressive amount, in base units.</param>
        /// <param name="prizeString">The new progressive prize string.</param>
        public virtual void Update(long amountValue, string prizeString)
        {
            ActualAmount = amountValue;
            PrizeString = prizeString;
        }

        #region IProgressiveBroadcastData Implementation

        /// <inheritdoc />
        /// <remarks>Will return the locked value if the <see cref="LockAmount"/> is not null.</remarks>
        public virtual long Amount => LockAmount ?? ActualAmount;

        /// <inheritdoc />
        public string PrizeString { get; protected set; }

        #endregion

        #region Equality Members

        /// <summary>
        /// Overload Equals for enhanced performance.
        /// </summary>
        /// <param name="rightHand">The right hand object for the equality check.</param>
        /// <returns>True if the right hand object equals to this object.  False otherwise.</returns>
        public virtual bool Equals(ProgressiveBroadcastData rightHand)
        {
            if((object)rightHand == null)
            {
                return false;
            }

            return ActualAmount == rightHand.ActualAmount &&
                   PrizeString == rightHand.PrizeString &&
                   LockAmount == rightHand.LockAmount;
        }

        /// <summary>
        /// Override value type's implementation for better performance.
        /// </summary>
        /// <param name="obj">The right hand object for the equality check.</param>
        /// <returns>True if the right hand object equals to this object.  False otherwise.</returns>
        public override bool Equals(object obj)
        {
            var result = false;
            var other = obj as ProgressiveBroadcastData;
            if(other != null)
            {
                result = Equals(other);
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
            hash = hash * 37 + ActualAmount.GetHashCode();
            hash = PrizeString != null ? hash * 37 + PrizeString.GetHashCode() : hash;
            hash = LockAmount != null ? hash * 37 + LockAmount.GetHashCode() : hash;
            return hash;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are considered equal.  False otherwise.</returns>
        public static bool operator ==(ProgressiveBroadcastData left, ProgressiveBroadcastData right)
        {
            if(ReferenceEquals(left, right))
            {
                return true;
            }

            if((object)left == null || (object)right == null)
            {
                return false;
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are not considered equal.  False otherwise.</returns>
        public static bool operator !=(ProgressiveBroadcastData left, ProgressiveBroadcastData right)
        {
            return !(left == right);
        }

        #endregion

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var outputString = $"Amount({ActualAmount / 100.0:C})/Prize({PrizeString})";

            if(LockAmount != null)
            {
                outputString += $" Locked at {LockAmount / 100.0:C}";
            }

            return outputString;
        }

        #region ICompactSerializable Implementation

        /// <inheritdoc />
        public virtual void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, ActualAmount);
            CompactSerializer.Write(stream, PrizeString);
            SerializeNullableLong(stream, LockAmount);
        }

        /// <inheritdoc />
        public virtual void Deserialize(Stream stream)
        {
            ActualAmount = CompactSerializer.ReadLong(stream);
            PrizeString = CompactSerializer.ReadString(stream);
            LockAmount = DeserializeNullableLong(stream);
        }

        #endregion

        #region IDeepCloneable Implementation

        /// <inheritdoc />
        public object DeepClone()
        {
            var copy = new ProgressiveBroadcastData
                           {
                               ActualAmount = ActualAmount,
                               PrizeString = PrizeString,
                               LockAmount = LockAmount
                           };
            return copy;
        }

        #endregion

        /// <summary>
        /// Serializes a nullable long. By convention, a "null" will be serialized as 
        /// <see cref="NullSerializationValue"/>, and deserialized as a null.
        /// </summary>
        /// <param name="valueToSerialize">Nullable long to serialize.</param>
        /// <param name="stream">The stream where the nullable long data is serialized to.</param>
        protected static void SerializeNullableLong(Stream stream, long? valueToSerialize)
        {
            var value = valueToSerialize ?? NullSerializationValue;
            CompactSerializer.Serialize(stream, value);
        }

        /// <summary>
        /// Deserializes a nullable long. By convention, a "null" will be serialized as 
        /// <see cref="NullSerializationValue"/>, and deserialized as a null.
        /// </summary>
        /// <param name="stream">The stream where the nullable long data is read from.</param>
        /// <returns>Nullable long that was deserialized.</returns>
        protected static long? DeserializeNullableLong(Stream stream)
        {
            long? returnValue = null;

            var value = CompactSerializer.ReadLong(stream);

            if(value != NullSerializationValue)
            {
                returnValue = value;
            }

            return returnValue;
        }
    }
}
