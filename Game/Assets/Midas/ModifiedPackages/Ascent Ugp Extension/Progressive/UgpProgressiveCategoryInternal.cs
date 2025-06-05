//-----------------------------------------------------------------------
// <copyright file = "UgpProgressiveCategoryInternal.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Progressive
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using F2L.Schemas.Internal;
    using F2XTransport;
    using Ugp;

    /// <remarks />
    /// <remarks />
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(IsNullable = false)]
    public enum ProgressiveType
    {
        /// <remarks />
        Triggered,

        /// <remarks />
        Mystery,

        /// <remarks />
        MysteryPostGame
    }

    /// <remarks />
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(ElementName = "UgpProgressiveCategory", IsNullable = false)]
    [DisableCodeCoverageInspection]
    public class UgpProgressiveCategoryInternal : UgpCategoryBase<ProgressiveCategoryMessage>
    {
    }

    /// <remarks />
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class ProgressiveCategoryMessage : ICategoryMessage
    {
        /// <remarks />
        [XmlElement("ProgressiveLevelInfoReply", typeof(ProgressiveLevelInfoReply))]
        [XmlElement("ProgressiveLevelInfoSend", typeof(ProgressiveLevelInfoSend))]
        [XmlElement("ProgressiveListReply", typeof(ProgressiveListReply))]
        [XmlElement("ProgressiveListSend", typeof(ProgressiveListSend))]
        public object Item { get; set; }
    }

    /// <remarks />
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class ProgressiveLevelInfoReply : UgpCategoryReply
    {
        /// <remarks />
        public ProgressiveLevelInfo Info { get; set; }
    }

    /// <remarks />
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class ProgressiveLevelInfoSend : UgpCategorySend
    {
        /// <remarks />
        public string ProgressiveLevelId { get; set; }
    }

    /// <remarks />
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class ProgressiveListReply : UgpCategoryReply
    {
        /// <remarks />
        public List<ProgressiveLevelInfo> ProgressivesList { get; set; }
    }

    /// <remarks />
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class ProgressiveListSend : UgpCategorySend
    {
    }

    /// <remarks />
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class ProgressiveLevelInfo
    {
        /// <remarks />
        public string Id { get; set; }

        /// <remarks />
        public string Name { get; set; }

        /// <remarks />
        public double IncrementPercentage { get; set; }

        /// <remarks />
        public double HiddenIncrementPercentage { get; set; }

        /// <remarks />
        public AmountType Startup { get; set; }

        /// <remarks />
        public AmountType Ceiling { get; set; }

        /// <remarks />
        public bool IsStandalone { get; set; }

        /// <remarks />
        public ProgressiveType ProgressiveType { get; set; }

        /// <remarks />
        public double Rtp { get; set; }

        /// <remarks />
        public double TriggerProbability { get; set; }
    }
}
