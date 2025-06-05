//-----------------------------------------------------------------------
// <copyright file = "UgpReserveCategoryInternal.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Reserve
{
    using System;
    using System.Xml.Serialization;
    using F2XTransport;
    using Ugp;

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(ElementName = "UgpReserveCategory", IsNullable = false)]
    [DisableCodeCoverageInspection]
    public class UgpReserveCategoryInternal : UgpCategoryBase<UgpReserveCategoryMessage>
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpReserveCategoryMessage : ICategoryMessage
    {
        #region Implementation of ICategoryMessage

        /// <summary>
        /// The message for the category. All categories have a main type and that type then contains a choice item.
        /// Each different type the choice represents a different message for that category.
        /// </summary>
        [XmlElement("SetReserveParametersSend", typeof(UgpReserveCategorySetReserveParametersSend))]
        [XmlElement("SetReserveParametersReply", typeof(UgpReserveCategorySetReserveParametersReply))]
        [XmlElement("SetReserveActivationSend", typeof(UgpReserveCategorySetActivationChangedSend))]
        [XmlElement("SetReserveActivationReply", typeof(UgpReserveCategorySetActivationChangedReply))]
		[XmlElement("GetReserveParametersSend", typeof(UgpReserveCategoryGetReserveParametersSend))]
		[XmlElement("GetReserveParametersReply", typeof(UgpReserveCategoryGetReserveParametersReply))]
        public object Item { get; set; }

        #endregion
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpReserveCategorySetReserveParametersSend : UgpCategorySend
    {
        /// <remarks/>
        public bool IsReserveAllowedWithCredits { get; set; }

        /// <remarks/>
        public bool IsReserveAllowedWithoutCredits { get; set; }

        /// <remarks/>
        public long ReserveTimeWithCreditsMilliseconds { get; set; }

        /// <remarks/>
        public long ReserveTimeWithoutCreditsMilliseconds { get; set; }
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpReserveCategorySetReserveParametersReply : UgpCategoryReply
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpReserveCategorySetActivationChangedSend : UgpCategorySend
    {
        /// <remarks/>
        public bool IsReserveActive { get; set; }
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpReserveCategorySetActivationChangedReply : UgpCategoryReply
    {
    }

	/// <remarks/>
	[Serializable]
	[XmlType(AnonymousType = true)]
	[DisableCodeCoverageInspection]
	public class UgpReserveCategoryGetReserveParametersSend : UgpCategorySend
	{
	}

	/// <remarks/>
	[Serializable]
	[XmlType(AnonymousType = true)]
	[DisableCodeCoverageInspection]
	public class UgpReserveCategoryGetReserveParametersReply : UgpCategoryReply
	{
		/// <remarks/>
		public bool IsReserveAllowedWithCredits { get; set; }

		/// <remarks/>
		public bool IsReserveAllowedWithoutCredits { get; set; }

		/// <remarks/>
		public long ReserveTimeWithCreditsMilliseconds { get; set; }

		/// <remarks/>
		public long ReserveTimeWithoutCreditsMilliseconds { get; set; }
	}
}
