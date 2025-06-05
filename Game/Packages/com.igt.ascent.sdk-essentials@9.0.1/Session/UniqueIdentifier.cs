//-----------------------------------------------------------------------
// <copyright file = "UniqueIdentifier.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Session
{
    using System;
    using System.Threading;
    using Cloneable;
    using CompactSerialization;

    /// <summary>
    /// A serializable application-wide unique identifier.
    /// It is guaranteed that every instance created by the method <see cref="UniqueIdentifier.New()"/> is unique
    /// to the existing ones; however, the instance deserialized from a stream would be an identical copy of the
    /// original one.
    /// </summary>
    /// <devdoc>
    /// The constructor <see cref="UniqueIdentifier()"/> is reserved for deserialization purpose and not supposed to be
    /// invoked by the user code.
    /// </devdoc>
    [Serializable]
    public sealed class UniqueIdentifier : IEquatable<UniqueIdentifier>, ICompactSerializable,
        IDeepCloneable
    {
        #region Static Members

        /// <summary>
        /// A class level counter used to generate unique instances.
        /// </summary>
        private static long counter;

        #endregion

        #region Private Fields

        /// <summary>
        /// The token of this instance used for identification.
        /// </summary>
        private long token;

        #endregion

        #region Constructors & Initialization

        /// <summary>
        /// Creates a new unique identifier.
        /// </summary>
        /// <returns>A new unique identifier.</returns>
        public static UniqueIdentifier New()
        {
            return new UniqueIdentifier(Interlocked.Increment(ref counter));
        }

        /// <summary>
        /// The default constructor which is used for deserialization purpose and not supposed to be invoked
        /// by the user code.
        /// </summary>
        public UniqueIdentifier()
        {
        }

        /// <summary>
        /// Constructs the unique identifier by the specified token.
        /// </summary>
        /// <param name="token">The token used to initialize the unique identifier.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="token"/> is not greater than 0.</exception>
        private UniqueIdentifier(long token)
        {
            if(token <= 0)
            {
                throw new ArgumentException("The token must be greater than 0.", nameof(token));
            }

            Initialize(token);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Retrieves the token.
        /// </summary>
        /// <returns>The token.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the unique identifier instance is accessed before initialized.
        /// </exception>
        private long RetrieveToken()
        {
            if(token <= 0)
            {
                throw new InvalidOperationException(
                    $"{"The unique identifier is accessed before initialized. "}{"To create a new unique identifier, "}{"please call UniqueIdentifier.New() instead of the constructor."}");
            }
            return token;
        }

        /// <summary>
        /// Initializes the unique identifier.
        /// </summary>
        /// <param name="value">The value used to initialize the unique identifier.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this unique identifier instance has already been initialized.
        /// </exception>
        private void Initialize(long value)
        {
            if(token > 0)
            {
                throw new InvalidOperationException("The unique identifier has already been initialized.");
            }

            token = value;
        }

        #endregion

        #region Overrides

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as UniqueIdentifier);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return RetrieveToken().GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Unique Identifier:{RetrieveToken()}";
        }

        #endregion

        #region IEquatable Members

        /// <inheritdoc />
        public bool Equals(UniqueIdentifier other)
        {
            return this == other;
        }

        #endregion

        #region Overload Operators

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="left">The left side object to compare.</param>
        /// <param name="right">The right side object to compare.</param>
        /// <returns>True if the objects are equal.</returns>
        public static bool operator ==(UniqueIdentifier left, UniqueIdentifier right)
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

            return left.RetrieveToken() == right.RetrieveToken();
        }

        /// <summary>
        /// Tests for inequality.
        /// </summary>
        /// <param name="left">The left side object to compare.</param>
        /// <param name="right">The right side object to compare.</param>
        /// <returns>True if the objects are not equal.</returns>
        public static bool operator !=(UniqueIdentifier left, UniqueIdentifier right)
        {
            return !(left == right);
        }

        #endregion

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(System.IO.Stream stream)
        {
            CompactSerializer.Write(stream, RetrieveToken());
        }

        /// <inheritdoc />
        public void Deserialize(System.IO.Stream stream)
        {
            var cachedToken = CompactSerializer.ReadLong(stream);
            if(cachedToken <= 0)
            {
                throw new InvalidOperationException("Deserialization failed. The token must be greater than 0.");
            }

            Initialize(cachedToken);
        }

        #endregion

        #region IDeepCloneable Members

        /// <inheritdoc />
        public object DeepClone()
        {
            // This type is supposed to be immutable. However, the invoking to Deserialize() of an existing instance could 
            // corrupt its immutibility. Thus, we must disallow such invoking for an instance already in use.
            return this;
        }

        #endregion
    }
}
