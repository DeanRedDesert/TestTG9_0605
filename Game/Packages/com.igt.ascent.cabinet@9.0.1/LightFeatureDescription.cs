//-----------------------------------------------------------------------
// <copyright file = "LightFeatureDescription.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System.Collections.Generic;

    /// <summary>
    /// Class which provides a description of a light feature.
    /// </summary>
    public class LightFeatureDescription
    {
        /// <summary>
        /// The ID of the feature.
        /// </summary>
        public string FeatureId { get; }

        /// <summary>
        /// The sub feature type.
        /// </summary>
        public LightSubFeature SubFeature { get; }

        /// <summary>
        /// A list of the light groups in the feature.
        /// </summary>
        public IEnumerable<ILightGroup> Groups { get; }

        /// <summary>
        /// Construct a light feature description.
        /// </summary>
        /// <param name="featureId">The feature id.</param>
        /// <param name="lightSubFeature">The light sub feature.</param>
        /// <param name="groups">List of groups in the feature.</param>
        public LightFeatureDescription(string featureId, LightSubFeature lightSubFeature, IEnumerable<ILightGroup> groups)
        {
            FeatureId = featureId;
            SubFeature = lightSubFeature;
            Groups = new List<ILightGroup>(groups);
        }
    }
}
