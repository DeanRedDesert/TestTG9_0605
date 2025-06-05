//-----------------------------------------------------------------------
// <copyright file = "PortalId.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Encapsulates the portal's id.
    /// </summary>
    public class PortalId : IEquatable<PortalId>
    {
        /// <summary>
        /// The stored representation of the Id in GUID format.
        /// </summary>
        private readonly Guid id;

        /// <summary>
        /// Instantiates a <see cref="PortalId" /> object with the specified input.
        /// </summary>
        /// <param name="guid">The portal's Id in string format.</param>
        /// <exception cref="ArgumentNullException">Thrown if input is null/empty.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the input does not match the format of a valid GUID.
        /// </exception>
        public PortalId(string guid)
        {
            if(string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException(nameof(guid));
            }

            try
            {
                id = new Guid(guid);
            }
            catch(FormatException)
            {
                throw new ArgumentException("The input GUID is of an invalid format.");
            }
            catch(OverflowException)
            {
                throw new ArgumentException("The input GUID is of an invalid format.");
            }
        }

        /// <summary>
        /// Returns a string representation of the Id.
        /// </summary>
        public string Id => id.ToString();

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string that specifically describes the object.</returns>
        public override string ToString()
        {
            return $"PortalId: Id({Id})";
        }

        /// <summary>
        /// Compares the guid of this instance to other.
        /// </summary>
        /// <param name="other">Second instance to compare against.</param>
        /// <returns>True if both instances are equal; otherwise, false.</returns>
        public bool Equals(PortalId other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            if(ReferenceEquals(this, other))
            {
                return true;
            }
            return id.Equals(other.id);
        }

        /// <summary>
        /// Compares the guid of this instance to obj.
        /// </summary>
        /// <param name="obj">Second instance to compare against.</param>
        /// <returns>True if both instances are equal; otherwise, false.</returns>
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
            if(obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((PortalId)obj);
        }

        /// <summary>
        /// Generates the hashcode for this object.
        /// </summary>
        /// <returns>The hashcode for this object.</returns>
        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        /// <summary>
        /// An invalid portal identifier.
        /// </summary>
        public static PortalId InvalidPortalId => new PortalId("00000000-0000-0000-0000-000000000000");
    }
}