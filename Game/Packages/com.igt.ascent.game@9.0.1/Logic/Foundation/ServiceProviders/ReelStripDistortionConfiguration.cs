// -----------------------------------------------------------------------
//  <copyright file = "ReelStripDistortionConfiguration.cs" company = "IGT">
//      Copyright (c) 2017 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

// TODO: http://cspjira:8080/jira/browse/AS-8697
// ReSharper disable NonReadonlyMemberInGetHashCode
namespace IGT.Game.Core.Logic.Foundation.ServiceProviders
{
    using System;
    using System.IO;
    using Cloneable;
    using CompactSerialization;

    /// <summary>
    /// Contains information about limits enforced on the reel strip distortion by the jurisdiction.
    /// </summary>
    [Serializable]
    public class ReelStripDistortionConfiguration : ICompactSerializable, IEquatable<ReelStripDistortionConfiguration>,
        IDeepCloneable
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <remarks>
        /// Necessary to support <see cref="ICompactSerializable"/>.
        /// </remarks>
        public ReelStripDistortionConfiguration()
        {
        }

        /// <summary>
        /// Creates a new <see cref="ReelStripDistortionConfiguration"/> object.
        /// </summary>
        /// <param name="enabled"><code>true</code> if reel strip distortion is allowed; otherwise <code>false</code>.</param>
        /// <param name="limit">The maximim allowed reel strip distortion factor.</param>
        public ReelStripDistortionConfiguration(bool enabled, float limit)
        {
            Enabled = enabled;
            Limit = limit;
        }

        /// <summary>
        /// Get if reel strip distortion is enabled.
        /// </summary>
        /// <remarks>
        /// <code>true</code> if reel strip distortion is allowed; otherwise <code>false</code>.
        /// </remarks>
        public bool Enabled { get; private  set; }

        /// <summary>
        /// Get the maximum allowed reel strip distortion factor.
        /// </summary>
        /// <remarks>Valid values are between <code>1.0f</code> and <code>10000.0f</code>.</remarks>
        public float Limit { get; private set; }

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, Enabled);
            CompactSerializer.Write(stream, Limit);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            Enabled = CompactSerializer.ReadBool(stream);
            Limit = CompactSerializer.ReadFloat(stream);
        }

        #region IDeepCloneable Members

        /// <inheritDoc/>
        public object DeepClone()
        {
            // This type is supposed to be immutable. However, the invoking to Deserialize() of an existing instance could 
            // corrupt its immutability. Thus, we must disallow such invoking for an instance already in use.
            return this;
        }

        #endregion

        /// <summary>
        /// Determines whether the specified <see cref="ReelStripDistortionConfiguration"/> object is equal to the current object.
        /// </summary>
        /// <param name="other">The <see cref="ReelStripDistortionConfiguration"/> to compare with the current object.</param>
        /// <returns><code>true</code> if the specified <see cref="ReelStripDistortionConfiguration"/> is equal to the current
        /// object; otherwise <code>false</code></returns>
        public bool Equals(ReelStripDistortionConfiguration other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            if(ReferenceEquals(this, other))
            {
                return true;
            }
            return Enabled == other.Enabled && Limit.Equals(other.Limit);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as ReelStripDistortionConfiguration);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Enabled.GetHashCode() * 397) ^ Limit.GetHashCode();
            }
        }
    }
}