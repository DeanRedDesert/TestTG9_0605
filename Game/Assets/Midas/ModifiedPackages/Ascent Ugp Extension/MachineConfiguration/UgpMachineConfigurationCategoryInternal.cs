//-----------------------------------------------------------------------
// <copyright file = "UgpMachineConfigurationCategoryInternal.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MachineConfiguration
{
    using System;
    using System.Xml.Serialization;
    using F2XTransport;
    using Ugp;

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(ElementName = "UgpMachineConfigurationCategory", IsNullable = false)]
    [DisableCodeCoverageInspection]
    public class UgpMachineConfigurationCategoryInternal : UgpCategoryBase<UgpMachineConfigurationCategoryMessage>
    {
    }

    /// <remarks/>
    public enum UgpMachineConfigurationWinCapStyleEnum
    {
        /// <remarks/>
        None,

        /// <remarks/>
        Clip,

        /// <remarks/>
        ClipAndBreakout,
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpMachineConfigurationCategoryMessage : ICategoryMessage
    {
        #region Implementation of ICategoryMessage

        /// <summary>
        /// The message for the category. All categories have a main type and that type then contains a choice item.
        /// Each different type the choice represents a different message for that category.
        /// </summary>
        [XmlElement("SetParametersSend", typeof(UgpMachineConfigurationCategorySetParametersSend))]
        [XmlElement("SetParametersReply", typeof(UgpMachineConfigurationCategorySetParametersReply))]
		[XmlElement("GetParametersSend", typeof(UgpMachineConfigurationCategoryGetParametersSend))]
		[XmlElement("GetParametersReply", typeof(UgpMachineConfigurationCategoryGetParametersReply))]
        public object Item { get; set; }

        #endregion
    }

	/// <remarks/>
	[Serializable]
	[XmlType(AnonymousType = true)]
	[DisableCodeCoverageInspection]
	public class UgpMachineConfigurationCategoryGetParametersSend : UgpCategorySend
	{
	}

	/// <remarks/>
	[Serializable]
	[XmlType(AnonymousType = true)]
	[DisableCodeCoverageInspection]
	public class UgpMachineConfigurationCategoryGetParametersReply : UgpCategoryReply
	{
		/// <remarks/>
		public bool IsClockVisible { get; set; }

		/// <remarks/>
		public string ClockFormat { get; set; }

		/// <remarks/>
		public long Tokenisation { get; set; }

		/// <remarks/>
		public long GameCycleTime { get; set; }

		/// <remarks/>
		public bool ContinuousPlayAllowed { get; set; }

		/// <remarks/>
		public bool SlamSpinAllowed { get; set; }

		/// <remarks/>
		public bool FeatureAutoStartEnabled { get; set; }

		/// <remarks/>
		public long CurrentMaximumBet { get; set; }

		/// <remarks/>
		public UgpMachineConfigurationWinCapStyleEnum WinCapStyle { get; set; }

		/// <remarks/>
		public int QcomJurisdiction { get; set; }

		/// <remarks/>
		public string CabinetId { get; set; }

		/// <remarks/>
		public string BrainboxId { get; set; }

		/// <remarks/>
		public string Gpu { get; set; }
	}

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpMachineConfigurationCategorySetParametersSend : UgpCategorySend
    {
        /// <remarks/>
        public bool IsClockVisible { get; set; }

        /// <remarks/>
        public string ClockFormat { get; set; }

        /// <remarks/>
        public long Tokenisation { get; set; }

        /// <remarks/>
        public long GameCycleTime { get; set; }

        /// <remarks/>
        public bool ContinuousPlayAllowed { get; set; }

        /// <remarks/>
        public bool FeatureAutoStartEnabled { get; set; }

        /// <remarks/>
        public long CurrentMaximumBet { get; set; }

        /// <remarks/>
        public UgpMachineConfigurationWinCapStyleEnum WinCapStyle { get; set; }

        /// <remarks/>
        public bool SlamSpinAllowed { get; set; }

		/// <remarks/>
		public int QcomJurisdiction { get; set; }

		/// <remarks/>
		public string CabinetId { get; set; }

		/// <remarks/>
		public string BrainboxId { get; set; }

		/// <remarks/>
		public string Gpu { get; set; }
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpMachineConfigurationCategorySetParametersReply : UgpCategoryReply
    {
    }
}