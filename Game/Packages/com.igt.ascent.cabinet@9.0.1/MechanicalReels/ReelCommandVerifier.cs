//-----------------------------------------------------------------------
// <copyright file = "ReelCommandVerifier.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Class which verifies parameters for mechanical reels calls.
    /// </summary>
    public static class ReelCommandVerifier
    {
        #region Public Methods

        /// <summary>
        /// Verify the spin profiles against the device description.
        /// </summary>
        /// <param name="featureDescription">The device description to verify against.</param>
        /// <param name="spinProfiles">The spin profiles to verify.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="spinProfiles"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="spinProfiles"/> is empty.
        /// </exception>
        /// <exception cref="DuplicateReelException">
        /// Thrown when a single reel is specified multiple times.
        /// </exception>
        /// <exception cref="InvalidReelException">
        /// Thrown when a specified reel is not within the range of valid reels.
        /// </exception>
        /// <exception cref="InvalidDecelerationIndexException">
        /// Thrown when a specified deceleration index is not within the range of valid indexes.
        /// </exception>
        /// <exception cref="InvalidReelStopException">
        /// Thrown if any of the specified stops are out of range.
        /// </exception>
        /// <exception cref="InvalidHoverLimitException">
        /// Thrown if any specified hover limit values exceed the stop count for their reels.
        /// </exception>
        public static void VerifySpin(ReelFeatureDescription featureDescription, ICollection<SpinProfile> spinProfiles)
        {
            if(spinProfiles == null)
            {
                throw new ArgumentNullException(nameof(spinProfiles));
            }

            if(!spinProfiles.Any())
            {
                throw new ArgumentException("Data list is empty", nameof(spinProfiles));
            }

            var reelIndexes = new List<byte>();

            foreach(var spinProfile in spinProfiles)
            {
                VerifyReelIndex(featureDescription, spinProfile.ReelNumber, reelIndexes);
                VerifyDecelerationIndex(featureDescription.FeatureId, spinProfile.Deceleration, featureDescription.SupportedDecelerationProfiles);

                var reelDescription = featureDescription.ReelDescriptions.ElementAt(spinProfile.ReelNumber);
                VerifyStop(featureDescription.FeatureId, spinProfile.ReelNumber, spinProfile.Stop, reelDescription.NumberOfStops);
                VerifyAttributes(featureDescription.FeatureId, spinProfile.ReelNumber, spinProfile.Attributes, reelDescription);
            }
        }

        /// <summary>
        /// Verify the stops against the device description.
        /// </summary>
        /// <param name="description">The device description to verify against.</param>
        /// <param name="reelStops">The collection of <see cref="ReelStop"/> objects.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="reelStops"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="reelStops"/> is empty.
        /// </exception>
        /// <exception cref="DuplicateReelException">
        /// Thrown when a single reel is specified multiple times.
        /// </exception>
        /// <exception cref="InvalidReelException">
        /// Thrown when a specified reel is not within the range of valid reels.
        /// </exception>
        /// <exception cref="InvalidReelStopException">
        /// Thrown if any of the specified stops are out of range.
        /// </exception>
        public static void VerifyStops(ReelFeatureDescription description, ICollection<ReelStop> reelStops)
        {
            if(reelStops == null)
            {
                throw new ArgumentNullException(nameof(reelStops));
            }

            if(!reelStops.Any())
            {
                throw new ArgumentException("Data list is empty", nameof(reelStops));
            }

            var reelIndexes = new List<byte>();

            foreach(var reelStop in reelStops)
            {
                VerifyReelIndex(description, reelStop.ReelNumber, reelIndexes);
                VerifyStop(description.FeatureId, reelStop.ReelNumber, reelStop.Stop,
                           description.ReelDescriptions.ElementAt(reelStop.ReelNumber).NumberOfStops);
            }
        }

        /// <summary>
        /// Verify reel stops against the device description.
        /// </summary>
        /// <param name="description">The device description to verify against.</param>
        /// <param name="reelStops">The collection of reel stops, where the index of the collection 
        /// is the reel index (the reel number.)</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="reelStops"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="reelStops"/> is empty.
        /// </exception>
        /// <exception cref="DuplicateReelException">
        /// Thrown when a single reel is specified multiple times.
        /// </exception>
        /// <exception cref="InvalidReelException">
        /// Thrown when a specified reel is not within the range of valid reels.
        /// </exception>
        /// <exception cref="InvalidReelStopException">
        /// Thrown if any of the specified stops are out of range.
        /// </exception>
        public static void VerifyStops(ReelFeatureDescription description, ICollection<byte> reelStops)
        {
            if(reelStops == null)
            {
                throw new ArgumentNullException(nameof(reelStops));
            }

            if(!reelStops.Any())
            {
                throw new ArgumentException("Data list is empty", nameof(reelStops));
            }

            var reelIndexes = new List<byte>();
            byte reelIndex = 0;
            foreach(var reelStop in reelStops)
            {
                VerifyReelIndex(description, reelIndex, reelIndexes);
                VerifyStop(description.FeatureId, reelIndex, reelStop,
                           description.ReelDescriptions.ElementAt(reelIndex++).NumberOfStops);
            }
        }

        /// <summary>
        /// Verify the spin profiles against the device description.
        /// </summary>
        /// <param name="description">The device description to verify against.</param>
        /// <param name="speedIndex">The speed index to verify.</param>
        /// <param name="spinProfiles">The spin profiles to verify.</param>
        /// <exception cref="InvalidSpeedIndexException">
        /// Thrown if <paramref name="speedIndex"/> is out of range.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="spinProfiles"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="spinProfiles"/> is empty.
        /// </exception>
        /// <exception cref="InvalidReelDirectionException">
        /// Thrown if any of <paramref name="spinProfiles"/> uses
        /// <see cref="ReelDirection.Shortest"/> as the reel direction.
        /// </exception>
        /// <exception cref="DuplicateReelException">
        /// Thrown when a single reel is specified multiple times.
        /// </exception>
        /// <exception cref="InvalidReelException">
        /// Thrown when a specified reel is not within the range of valid reels.
        /// </exception>
        /// <exception cref="InvalidReelStopException">
        /// Thrown if any of the specified stops are out of range.
        /// </exception>
        public static void VerifySynchronousSpin(ReelFeatureDescription description, ushort speedIndex,
                                                 ICollection<SynchronousSpinProfile> spinProfiles)
        {
            if(speedIndex >= description.SupportedSpeeds.Count)
            {
                throw new InvalidSpeedIndexException(description.FeatureId, speedIndex);
            }

            if(spinProfiles == null)
            {
                throw new ArgumentNullException(nameof(spinProfiles));
            }

            if(!spinProfiles.Any())
            {
                throw new ArgumentException("Data list is empty", nameof(spinProfiles));
            }

            foreach(var spinProfile in spinProfiles)
            {
                if(spinProfile.Direction == ReelDirection.Shortest)
                {
                    throw new InvalidReelDirectionException(description.FeatureId, spinProfile.ReelNumber, ReelDirection.Shortest);
                }
            }

            var reelIndexes = new List<byte>();

            foreach(var spinProfile in spinProfiles)
            {
                VerifyReelIndex(description, spinProfile.ReelNumber, reelIndexes);

                VerifyStop(description.FeatureId, spinProfile.ReelNumber, spinProfile.Stop,
                           description.ReelDescriptions.ElementAt(spinProfile.ReelNumber).NumberOfStops);
            }
        }

        /// <summary>
        /// Verify the set stop order command against the device description.
        /// </summary>
        /// <param name="description">The device description to verify against.</param>
        /// <param name="reels">The reel stop order to verify. An empty list is allowed which will tell the foundation
        /// to reset any previously set stop orders.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="reels"/> is null.
        /// </exception>
        /// <exception cref="DuplicateReelException">
        /// Thrown when a single reel is specified multiple times.
        /// </exception>
        /// <exception cref="InvalidReelException">
        /// Thrown when a specified reel is not within the range of valid reels.
        /// </exception>
        public static void VerifySetStopOrder(ReelFeatureDescription description, ICollection<byte> reels)
        {
            if(reels == null)
            {
                throw new ArgumentNullException(nameof(reels));
            }

            if(reels.Any())
            {
                VerifyReelIndexes(description, reels);
            }
        }

        /// <summary>
        /// Verify that the reels contained in the synchronous stop command are valid.
        /// </summary>
        /// <param name="description">The device description to verify against.</param>
        /// <param name="reels">The reels to verify.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="reels"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="reels"/> is empty.
        /// </exception>
        /// <exception cref="DuplicateReelException">
        /// Thrown when a single reel is specified multiple times.
        /// </exception>
        /// <exception cref="InvalidReelException">
        /// Thrown when a specified reel is not within the range of valid reels.
        /// </exception>
        public static void VerifySynchronousStop(ReelFeatureDescription description, ICollection<byte> reels)
        {
            if(reels == null)
            {
                throw new ArgumentNullException(nameof(reels));
            }

            if(!reels.Any())
            {
                throw new ArgumentException("Data list is empty", nameof(reels));
            }

            VerifyReelIndexes(description, reels);
        }

        /// <summary>
        /// Verify the given reel indexes.
        /// </summary>
        /// <param name="description">Description of the reel feature.</param>
        /// <param name="reels">The reels to verify.</param>
        public static void VerifyReelIndexes(ReelFeatureDescription description, IEnumerable<byte> reels)
        {
            var reelIndexes = new List<byte>();

            foreach(var reel in reels)
            {
                VerifyReelIndex(description, reel, reelIndexes);
            }
        }

        /// <summary>
        /// Verify that the device contains the specified reel.
        /// </summary>
        /// <param name="description">The device description to verify against.</param>
        /// <param name="reel">The reel index to verify.</param>
        /// <exception cref="InvalidReelException">
        /// Thrown when a specified reel is not within the range of valid reels.
        /// </exception>
        public static void VerifyReelIndex(ReelFeatureDescription description, byte reel)
        {
            if(reel >= description.ReelDescriptions.Count())
            {
                throw new InvalidReelException(description.FeatureId, reel);
            }
        }

        /// <summary>
        /// Verify that the specified collection of change speed profiles is valid.
        /// </summary>
        /// <param name="description">The device description to verify against.</param>
        /// <param name="changeSpeedProfiles">The collection of reel indexes vs. <see cref="ChangeSpeedProfile"/> enums to verify.</param>
        /// <exception cref="InvalidChangeSpeedProfileException">
        /// Thrown when a specified change speed profile is not valid.
        /// </exception>
        public static void VerifyChangeSpeedProfiles(ReelFeatureDescription description, IDictionary<byte, ChangeSpeedProfile> changeSpeedProfiles)
        {
            if(changeSpeedProfiles?.Any() != true)
            {
                throw new InvalidChangeSpeedProfileException(description.FeatureId);
            }

            VerifyReelIndexes(description, changeSpeedProfiles.Keys);

            // Check for and disallow 'shortest' directions.
            foreach(var changeSpeedProfile in changeSpeedProfiles)
            {
                if(changeSpeedProfile.Value.Direction == ReelDirection.Shortest)
                {
                    throw new InvalidChangeSpeedProfileException(description.FeatureId, 
                                                                 changeSpeedProfile.Value.Number, 
                                                                 changeSpeedProfile.Value.Direction);
                }
            }
        }
        
        #endregion

        #region Private Methods

        /// <summary>
        /// Verify the deceleration index.
        /// </summary>
        /// <param name="featureId">The device description to verify against.</param>
        /// <param name="decelerationIndex">The deceleration index to verify.</param>
        /// <param name="decelerationProfiles">The deceleration indexes that the reel shelf supports.</param>
        /// <exception cref="InvalidDecelerationIndexException">
        /// Thrown when a specified deceleration index is not within the range of valid indexes.
        /// </exception>
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static void VerifyDecelerationIndex(string featureId, ushort decelerationIndex, ICollection<ushort> decelerationProfiles)
        {
            if(decelerationIndex >= decelerationProfiles.Count)
            {
                throw new InvalidDecelerationIndexException(featureId, decelerationIndex);
            }
        }

        /// <summary>
        /// Verify a reel index.
        /// </summary>
        /// <param name="description">Description of the device the reel is a member of.</param>
        /// <param name="reel">The number of the reel.</param>
        /// <param name="reelIndexes">
        /// List of indexes which have already been verified. For most commands it is not valid for the same index to
        /// appear more than once. After the reel has been verified it will be added to the list.
        /// </param>
        /// <exception cref="DuplicateReelException">
        /// Thrown when a single reel is specified multiple times.
        /// </exception>
        /// <exception cref="InvalidReelException">
        /// Thrown when a specified reel is not within the range of valid reels.
        /// </exception>
        private static void VerifyReelIndex(ReelFeatureDescription description, byte reel, ICollection<byte> reelIndexes)
        {
            if(reelIndexes.Contains(reel))
            {
                throw new DuplicateReelException(description.FeatureId, reel);
            }
            if(reel >= description.ReelDescriptions.Count())
            {
                throw new InvalidReelException(description.FeatureId, reel);
            }

            reelIndexes.Add(reel);
        }

        /// <summary>
        /// Verify the stop against the given information.
        /// </summary>
        /// <param name="featureId">The ID of the device for error reporting purposes.</param>
        /// <param name="reelNumber">The reel number for error reporting purposes.</param>
        /// <param name="stop">The stop to verify.</param>
        /// <param name="numberOfStops">The number of stops supported by the reel.</param>
        /// <exception cref="InvalidReelStopException">
        /// Thrown if the stop is not within the valid range of stops.
        /// </exception>
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static void VerifyStop(string featureId, byte reelNumber, byte stop, byte numberOfStops)
        {
            if(stop >= numberOfStops && stop != SpinProfile.NoStop)
            {
                throw new InvalidReelStopException(featureId, reelNumber, stop);
            }
        }

        /// <summary>
        /// Verify the attributes against the reel description.
        /// </summary>
        /// <param name="featureId">The ID of the device for error reporting purposes.</param>
        /// <param name="reelNumber">The reel number for error reporting purposes.</param>
        /// <param name="spinAttributes">Attributes to verify.</param>
        /// <param name="reelDescription">Description of the reel.</param>
        /// <exception cref="InvalidHoverLimitException">
        /// Thrown if the hover limit values are specified and exceed the stop count for the reel,
        /// or if the lower and upper limits match.
        /// </exception>
        private static void VerifyAttributes(string featureId, byte reelNumber, SpinAttributes spinAttributes,
            ReelDescription reelDescription)
        {
            if(spinAttributes == null)
            {
                return;
            }

            if(spinAttributes.Hover.Level == HoverLevel.Custom)
            {
                var numberOfStops = reelDescription.NumberOfStops;
                var limits = spinAttributes.Hover.Limits;
                if(limits.LowerLimit >= numberOfStops ||
                   limits.UpperLimit >= numberOfStops ||
                   limits.LowerLimit == limits.UpperLimit)
                {
                    throw new InvalidHoverLimitException(featureId, reelNumber, numberOfStops, limits);
                }
            }
        }

        #endregion
    }
}
