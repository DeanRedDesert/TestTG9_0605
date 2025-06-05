// -----------------------------------------------------------------------
// <copyright file = "MarketingBehavior.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;
    using System.IO;
    using Game.Core.CompactSerialization;

    /// <summary>
    /// This data structure represents EGM wide marketing behavior.
    /// </summary>
    [Serializable]
    public class MarketingBehavior : ICompactSerializable
    {
        /// <summary>
        /// Indicates what kind of advertisement should be displayed on top screen.
        /// </summary>
        public TopScreenGameAdvertisementType TopScreenGameAdvertisement { get; set; }

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, TopScreenGameAdvertisement);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            TopScreenGameAdvertisement = CompactSerializer.ReadEnum<TopScreenGameAdvertisementType>(stream);
        }
    }
}