//-----------------------------------------------------------------------
// <copyright file = "ReelFeatureDescription.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    using System.Collections.Generic;

    /// <summary>
    /// Class which provides a description of a reel feature.
    /// </summary>
    public class ReelFeatureDescription
    {
        /// <summary>
        /// The ID of the feature.
        /// </summary>
        public string FeatureId { get; }

        /// <summary>
        /// The sub feature type.
        /// </summary>
        public ReelSubFeature SubFeature { get; }

        /// <summary>
        /// List of speeds supported by the device. Each 16-bit value
        /// is a speed, in CPM (cycles/rotations per minute), supported by the device.
        /// </summary>
        public ICollection<ushort> SupportedSpeeds { get; }

        /// <summary>
        /// List of the deceleration profiles supported by the device.
        /// Each 16-bit value is a deceleration time, in milliseconds,
        /// supported by the device. Note that this collection is the set of
        /// final deceleration values a reel can support when it is spinning and
        /// beginning to stop. They are not the values specified below
        /// <seealso cref="SupportedAccelerationDecelerationProfiles"/>, 
        /// which describe values used when changing the speed of a reel.
        /// </summary>
        public ICollection<ushort> SupportedDecelerationProfiles { get; }

        /// <summary>
        /// List of the supported <see cref="ReelAccelerationDecelerationTime"/> profiles supported by the device.
        /// Note: These are the supported profiles implemented for 'enhanced reel spin control',
        /// which is supported in CSI Reel category version 1.5+ only.
        /// </summary>
        public ICollection<ReelAccelerationDecelerationTime> SupportedAccelerationDecelerationProfiles { get; }

        /// <summary>
        /// A description of each reel in the feature.
        /// </summary>
        public IEnumerable<ReelDescription> ReelDescriptions { get; }

        /// <summary>
        /// Construct a reel feature description with the given characteristics for CSI Reel category 1.5+.
        /// </summary>
        /// <param name="featureId">The name of the feature.</param>
        /// <param name="subFeature">The sub feature type of the feature.</param>
        /// <param name="supportedSpeeds">List of speeds the feature supports.</param>
        /// <param name="supportedDecelerationProfiles">List of deceleration profiles the feature supports.</param>
        /// <param name="supportedAccelerationDecelerationProfiles">List of acceleration/deceleration profiles the feature supports (CSI Reel Categories 1.5+.)</param>
        /// <param name="reelDescriptions">Descriptions of the reels in the feature.</param>
        public ReelFeatureDescription(string featureId,
                                      ReelSubFeature subFeature,
                                      ICollection<ushort> supportedSpeeds,
                                      ICollection<ushort> supportedDecelerationProfiles,
                                      ICollection<ReelAccelerationDecelerationTime> supportedAccelerationDecelerationProfiles,
                                      IEnumerable<ReelDescription> reelDescriptions):
                                      this(featureId, subFeature, supportedSpeeds, supportedDecelerationProfiles, reelDescriptions)
        {
            SupportedAccelerationDecelerationProfiles = supportedAccelerationDecelerationProfiles;
        }

        /// <summary>
        /// Construct a reel feature description with the given characteristics.
        /// </summary>
        /// <param name="featureId">The name of the feature.</param>
        /// <param name="subFeature">The sub feature type of the feature.</param>
        /// <param name="supportedSpeeds">List of speeds the feature supports.</param>
        /// <param name="supportedDecelerationProfiles">List of deceleration profiles the feature supports.</param>
        /// <param name="reelDescriptions">Descriptions of the reels in the feature.</param>
        public ReelFeatureDescription(string featureId,
                                      ReelSubFeature subFeature,
                                      ICollection<ushort> supportedSpeeds,
                                      ICollection<ushort> supportedDecelerationProfiles,
                                      IEnumerable<ReelDescription> reelDescriptions)
        {
            FeatureId = featureId;
            SubFeature = subFeature;
            SupportedSpeeds = supportedSpeeds;
            SupportedDecelerationProfiles = supportedDecelerationProfiles;
            ReelDescriptions = reelDescriptions;
        }
    }
}
