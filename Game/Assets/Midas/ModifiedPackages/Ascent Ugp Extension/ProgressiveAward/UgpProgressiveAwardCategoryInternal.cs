//-----------------------------------------------------------------------
// <copyright file = "UgpProgressiveAwardCategoryInternal.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ProgressiveAward
{
    using System;
    using System.Xml.Serialization;
    using F2L.Schemas.Internal;
    using F2XTransport;
    using Ugp;

    /// <summary>
    /// Defines the possible progressive award pay types.
    /// </summary>
    public enum ProgressiveAwardPayTypeEnum
    {
        /// <summary>
        /// Transfer the progressive amount to the credit meter.
        /// </summary>
        CreditMeter,

        /// <summary>
        /// Transfer the progressive amount to the win meter.
        /// </summary>
        WinMeter,

        /// <summary>
        /// Call an attendant to award the progressive.
        /// </summary>
        Attendant,

        /// <summary>
        /// Transfer the progressive amount to Cashless.
        /// </summary>
        Cashless,
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(ElementName = "UgpProgressiveAwardCategory", IsNullable = false)]
    [DisableCodeCoverageInspection]
    public class UgpProgressiveAwardCategoryInternal : UgpCategoryBase<UgpProgressiveAwardCategoryMessage>
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpProgressiveAwardCategoryMessage : ICategoryMessage
    {
        #region Implementation of ICategoryMessage

        /// <summary>
        /// The message for the category. All categories have a main type and that type then contains a choice item.
        /// Each different type the choice represents a different message for that category.
        /// </summary>
        [XmlElement("ProgressiveVerifiedSend", typeof(UgpProgressiveAwardCategoryProgressiveVerifiedSend))]
        [XmlElement("ProgressiveVerifiedReply", typeof(UgpProgressiveAwardCategoryProgressiveVerifiedReply))]
        [XmlElement("ProgressiveProgressiveAwardStartingSend", typeof(UgpProgressiveAwardCategoryProgressiveAwardStartingSend))]
        [XmlElement("ProgressiveProgressiveAwardStartingReply", typeof(UgpProgressiveAwardCategoryProgressiveAwardStartingReply))]
        [XmlElement("ProgressiveFinishedDisplaySend", typeof(UgpProgressiveAwardCategoryProgressiveFinishedDispaySend))]
        [XmlElement("ProgressiveFinishedDisplayReply", typeof(UgpProgressiveAwardCategoryProgressiveFinishedDispayReply))]
        [XmlElement("ProgressiveProgressivePaidSend", typeof(UgpProgressiveAwardCategoryProgressivePaidSend))]
        [XmlElement("ProgressiveProgressivePaidReply", typeof(UgpProgressiveAwardCategoryProgressivePaidReply))]
        public object Item { get; set; }

        #endregion
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpProgressiveAwardCategoryProgressiveVerifiedSend : UgpCategorySend
    {
        /// <remarks/>
        public int AwardIndex;

        /// <remarks/>
        public string ProgressiveLevelId { get; set; }

        /// <remarks/>
        public ProgressiveAwardPayTypeEnum PayType { get; set; }

        /// <remarks/>
        public AmountType VerifiedAmount { get; set; }
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpProgressiveAwardCategoryProgressiveVerifiedReply : UgpCategoryReply
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpProgressiveAwardCategoryProgressiveAwardStartingSend : UgpCategorySend
    {
        /// <remarks/>
        public int AwardIndex;

        /// <remarks/>
        public string ProgressiveLevelId { get; set; }
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpProgressiveAwardCategoryProgressiveAwardStartingReply : UgpCategoryReply
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpProgressiveAwardCategoryProgressiveFinishedDispaySend : UgpCategorySend
    {
        /// <remarks/>
        public int AwardIndex;

        /// <remarks/>
        public string ProgressiveLevelId { get; set; }
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpProgressiveAwardCategoryProgressiveFinishedDispayReply : UgpCategoryReply
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpProgressiveAwardCategoryProgressivePaidSend : UgpCategorySend
    {
        /// <remarks/>
        public int AwardIndex;

        /// <remarks/>
        public string ProgressiveLevelId { get; set; }

        /// <remarks/>
        public AmountType TransferredAmount { get; set; }
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpProgressiveAwardCategoryProgressivePaidReply : UgpCategoryReply
    {
    }
}