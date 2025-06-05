//-----------------------------------------------------------------------
// <copyright file = "UgpExternalJackpotsCategoryInternal.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ExternalJackpots
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using F2XTransport;
    using Ugp;

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(ElementName = "UgpExternalJackpotsCategory", IsNullable = false)]
    [DisableCodeCoverageInspection]
    public class UgpExternalJackpotsCategoryInternal : UgpCategoryBase<UgpExternalJackpotsCategoryMessage>
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpExternalJackpotsCategoryMessage : ICategoryMessage
    {
        #region Implementation of ICategoryMessage

        /// <summary>
        /// The message for the category. All categories have a main type and that type then contains a choice item.
        /// Each different type the choice represents a different message for that category.
        /// </summary>
        [XmlElement("UpdateJackpotsSend", typeof(UgpExternalJackpotsCategoryUpdateJackpotsSend))]
        [XmlElement("UpdateJackpotsReply", typeof(UgpExternalJackpotsCategoryUpdateJackpotsReply))]
		[XmlElement("GetJackpotsSend", typeof(UgpExternalJackpotsCategoryGetJackpotsSend))]
		[XmlElement("GetJackpotsReply", typeof(UgpExternalJackpotsCategoryGetJackpotsReply))]
        public object Item { get; set; }

        #endregion
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpExternalJackpotsCategoryUpdateJackpotsSend : UgpCategorySend
    {
        /// <remarks/>
        public bool IsVisible { get; set; }

        /// <remarks/>
        public int IconId { get; set; }

        /// <remarks/>
        public List<UgpExternalJackpotsJackpot> Jackpots { get; set; }
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpExternalJackpotsCategoryUpdateJackpotsReply : UgpCategoryReply
    {
    }

	/// <remarks/>
	[Serializable]
	[XmlType(AnonymousType = true)]
	public class UgpExternalJackpotsCategoryGetJackpotsSend : UgpCategorySend
	{
	}

	/// <remarks/>
	[Serializable]
	[XmlType(AnonymousType = true)]
	public class UgpExternalJackpotsCategoryGetJackpotsReply : UgpCategoryReply
	{
		private List<UgpExternalJackpotsJackpot> jackpots = new List<UgpExternalJackpotsJackpot>();

		/// <remarks/>
		public List<UgpExternalJackpotsJackpot> Jackpots
		{
			get { return jackpots; }
		}
	}

    /// <summary>
    /// Implement an external jackpot. 
    /// </summary>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpExternalJackpotsJackpot
    {
        #region Properties

        /// <summary>
        /// Jackpot's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Jackpot's value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Whether the jackpot is visible for display.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Jackpot's Icon Id.
        /// </summary>
        public int IconId { get; set; }

        #endregion
    }
}