//-----------------------------------------------------------------------
// <copyright file = "UgpPidCategoryInternal.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Pid
{
    using System;
    using System.Xml.Serialization;
    using F2L.Schemas.Internal;
    using F2XTransport;
    using Ugp;

    /// <summary>
    /// Defines the internal enum of game information display style.
    /// </summary>
    public enum GameInformationDisplayStyleEnum
    {
        /// <summary>
        /// Don't display the game information.
        /// </summary>
        None = 0,

        /// <summary>
        /// Display the game information using the "Victorian" style.
        /// </summary>
        Victoria = 1,

        /// <summary>
        /// Display the game information using the "Queensland" style.
        /// </summary>
        Queensland = 2,

        /// <summary>
        /// Display the game information using the "NewZealand" style.
        /// </summary>
        NewZealand = 3,
    }

    /// <summary>
    /// Defines the internal enum of Session tracking options.
    /// </summary>
    public enum SessionTrackingOptionEnum
    {
        /// <summary>
        /// Disabled.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// Viewable.
        /// </summary>
        Viewable = 1,

        /// <summary>
        /// Controlled by player.
        /// </summary>
        PlayerControlled = 2,
    }

    /// <summary>
    /// This internal class contains information of PID configuration.
    /// </summary>
    [Serializable]
    [DisableCodeCoverageInspection]
    public class PidConfigurationInfo
    {
        #region Properties

        /// <summary>
        /// Gets or sets the flag indicating if the PID main entry screen is enabled.
        /// </summary>
        public bool IsMainEntryEnabled { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating if RequestService is enabled.
        /// </summary>
        public bool IsRequestServiceEnabled { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating if the request service is active.
        /// It is used to sync service activation state between foundation and game. True means active.
        /// </summary>
        public bool IsRequestServiceActivated { get; set; }

        /// <summary>
        /// Gets or sets the game information display style.
        /// </summary>
        public GameInformationDisplayStyleEnum GameInformationDisplayStyle { get; set; }

        /// <summary>
        /// Gets or sets the session tracking option.
        /// </summary>
        public SessionTrackingOptionEnum SessionTrackingOption { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating if game rules is enabled.
        /// </summary>
        public bool IsGameRulesEnabled { get; set; }

        /// <summary>
        /// Gets or sets the Information Menu Timeout.
        /// A value of zero indicates no timeout action.
        /// </summary>
        public TimeSpan InformationMenuTimeout { get; set; }

        /// <summary>
        /// Gets or sets the Session Start Message Timeout.
        /// A value of zero indicates no timeout action.
        /// </summary>
        public TimeSpan SessionStartMessageTimeout { get; set; }

        /// <summary>
        /// Gets or sets the View Session Screen Timeout.
        /// A value of zero indicates no timeout action.
        /// </summary>
        public TimeSpan ViewSessionScreenTimeout { get; set; }

        /// <summary>
        /// Gets or sets the View Game Information Timeout.
        /// A value of zero indicates no timeout action.
        /// </summary>
        public TimeSpan ViewGameInformationTimeout { get; set; }

        /// <summary>
        /// Gets or sets the View Game Rules Timeout.
        /// A value of zero indicates no timeout action.
        /// </summary>
        public TimeSpan ViewGameRulesTimeout { get; set; }

        /// <summary>
        /// Gets or sets the View Pay Table Timeout.
        /// A value of zero indicates no timeout action.
        /// </summary>
        public TimeSpan ViewPayTableTimeout { get; set; }

        /// <summary>
        /// Gets or sets the Session Timeout Interval.
        /// Session timeout interval indicates the amount of idle time in seconds before the EGM automatically
        /// stops session tracking. A value of zero indicates no timeout action.
        /// </summary>
        public TimeSpan SessionTimeoutInterval { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating if the Session Timeout Interval starts to count on zero.
        /// </summary>
        public bool SessionTimeoutStartOnZeroCredits { get; set; }

        /// <summary>
        /// Gets or sets the total number of link enrolments.
        /// If the game does not support PID, reject this parameter.
        /// Total number of progressive and mystery link jackpots EGM is enrolled in.
        /// </summary>
        public ushort TotalNumberLinkEnrolments { get; set; }

        /// <summary>
        /// Gets or sets the total link percentage contributions.
        /// </summary>
        public string TotalLinkPercentageContributions { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating if Jackpot display message includes count or not.
        /// </summary>
        public bool ShowLinkJackpotCount { get; set; }

        /// <summary>
        /// Gets or sets the RTP of the link jackpot components.
        /// </summary>
        public double LinkRtpForGameRtp { get; set; }

        #endregion
    }

    /// <summary>
    /// This internal class contains the information of a PID session.
    /// </summary>
    [Serializable]
    [DisableCodeCoverageInspection]
    public class PidSessionDataInfo
    {
        #region Properties

        /// <summary>
        /// Gets or sets the flag indicating if session tracking is active.
        /// </summary>
        public bool IsSessionTrackingActive { get; set; }

        /// <summary>
        /// Gets or sets the credit meter at the start of the session, in units fo cent.
        /// </summary>
        public long CreditMeterAtStart { get; set; }

        /// <summary>
        /// Gets or sets the currently available credits, in units fo cent.
        /// </summary>
        public long AvailableCredits { get; set; }

        /// <summary>
        /// Gets or sets the amount of cash in during the session.
        /// </summary>
        public long CashIn { get; set; }

        /// <summary>
        /// Gets or sets the amount of cash out during the session.
        /// </summary>
        public long CashOut { get; set; }

        /// <summary>
        /// Gets or sets the amount of credits played, in units fo cent.
        /// </summary>
        public long CreditsPlayed { get; set; }

        /// <summary>
        /// Gets or sets the amount of credits won, in units fo cent.
        /// </summary>
        public long CreditsWon { get; set; }

        /// <summary>
        /// Gets or sets the amount won or lost during the session.
        /// </summary>
        public long SessionWinOrLoss { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating if the current or last PID tracking session is a winning session.
        /// </summary>
        public bool IsWinningSession { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating if the session is for Crown EGM.
        /// </summary>
        public bool IsCrown { get; set; }

        /// <summary>
        /// Gets or sets the date/time the session was started.
        /// </summary>
        public DateTime SessionStarted { get; set; }

        /// <summary>
        /// Gets or sets the current session duration.
        /// </summary>
        public TimeSpan SessionDuration { get; set; }

        #endregion
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(ElementName = "UgpPidCategory", IsNullable = false)]
    [DisableCodeCoverageInspection]
    public class UgpPidCategoryInternal : UgpCategoryBase<UgpPidCategoryMessage>
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpPidCategoryMessage : ICategoryMessage
    {
        #region Implementation of ICategoryMessage

        /// <summary>
        /// The message for the category. All categories have a main type and that type then contains a choice item.
        /// Each different type the choice represents a different message for that category.
        /// </summary>
        [XmlElement("PidStartTrackingSend", typeof(UgpPidCategoryStartTrackingSend))]
        [XmlElement("PidStartTrackingReply", typeof(UgpPidCategoryStartTrackingReply))]
        [XmlElement("PidStopTrackingSend", typeof(UgpPidCategoryStopTrackingSend))]
        [XmlElement("PidStopTrackingReply", typeof(UgpPidCategoryStopTrackingReply))]
        [XmlElement("PidGetSessionDataSend", typeof(UgpPidCategoryGetSessionDataSend))]
        [XmlElement("PidGetSessionDataReply", typeof(UgpPidCategoryGetSessionDataReply))]
        [XmlElement("PidGetPidConfigurationSend", typeof(UgpPidCategoryGetPidConfigurationSend))]
        [XmlElement("PidGetPidConfigurationReply", typeof(UgpPidCategoryGetPidConfigurationReply))]
        [XmlElement("PidCategoryActivationStatusChangedSend", typeof(UgpPidCategoryActivationStatusChangedSend))]
        [XmlElement("PidCategoryActivationStatusChangedReply", typeof(UgpPidCategoryActivationStatusChangedReply))]
        [XmlElement("PidCategoryForceActivationSend", typeof(UgpPidCategoryForceActivationSend))]
        [XmlElement("PidCategoryForceActivationReply", typeof(UgpPidCategoryForceActivationReply))]
        [XmlElement("PidGameInformationScreenEnteredSend", typeof(UgpPidCategoryGameInformationScreenEnteredSend))]
        [XmlElement("PidGameInformationScreenEnteredReply", typeof(UgpPidCategoryGameInformationScreenEnteredReply))]
        [XmlElement("PidSessionInformationScreenEnteredSend", typeof(UgpPidCategorySessionInformationScreenEnteredSend))]
        [XmlElement("PidSessionInformationScreenEnteredReply", typeof(UgpPidCategorySessionInformationScreenEnteredReply))]
        [XmlElement("PidAttendantServiceRequestedSend", typeof(UgpPidCategoryAttendantServiceRequestedSend))]
        [XmlElement("PidAttendantServiceRequestedReply", typeof(UgpPidCategoryAttendantServiceRequestedReply))]
        [XmlElement("PidRequestForcePayoutSend", typeof(UgpPidCategoryRequestForcePayoutSend))]
        [XmlElement("PidRequestForcePayoutReply", typeof(UgpPidCategoryRequestForcePayoutReply))]
        [XmlElement("PidConfigurationChangedSend", typeof(UgpPidCategoryConfigurationChangedSend))]
        [XmlElement("PidConfigurationChangedReply", typeof(UgpPidCategoryConfigurationChangedReply))]
        public object Item { get; set; }

        #endregion
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpPidCategoryStartTrackingSend : UgpCategorySend
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpPidCategoryStartTrackingReply : UgpCategoryReply
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpPidCategoryStopTrackingSend : UgpCategorySend
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpPidCategoryStopTrackingReply : UgpCategoryReply
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpPidCategoryGetSessionDataSend : UgpCategorySend
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpPidCategoryGetSessionDataReply : UgpCategoryReply
    {
        /// <remarks/>
        public bool IsTracking { get; set; }

        /// <remarks/>
        public AmountType MoneyIn { get; set; }

        /// <remarks/>
        public AmountType Played { get; set; }

        /// <remarks/>
        public AmountType Won { get; set; }

        /// <remarks/>
        public AmountType SessionWinOrLoss { get; set; }

        /// <remarks/>
        public bool IsWinningSession { get; set; }

        /// <remarks/>
        public bool IsCrown { get; set; }

        /// <remarks/>
        public AmountType MoneyOut { get; set; }

        /// <remarks/>
        public long TimeStartedTick { get; set; }

        /// <remarks/>
        public long TotalPlayedTicks { get; set; }

        /// <remarks/>
        public long CurrentTimeTick { get; set; }

        /// <remarks/>
        public AmountType AvailableCashAtStart { get; set; }

        /// <remarks/>
        public AmountType AvailableCash { get; set; }
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpPidCategoryGetPidConfigurationSend : UgpCategorySend
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpPidCategoryGetPidConfigurationReply : UgpCategoryReply
    {
        /// <remarks/>
        public bool IsMainEntryEnabled { get; set; }

        /// <remarks/>
        public bool IsRequestServiceEnabled { get; set; }

        /// <remarks/>
        public bool IsRequestServiceActivated { get; set; }

        /// <remarks/>
        public GameInformationDisplayStyleEnum GameInformationDisplayStyle { get; set; }

        /// <remarks/>
        public SessionTrackingOptionEnum SessionTrackingOption { get; set; }

        /// <remarks/>
        public bool IsGameRulesEnabled { get; set; }

        /// <remarks/>
        public long InformationMenuTimeoutTicks { get; set; }

        /// <remarks/>
        public long SessionStartMessageTimeoutTicks { get; set; }

        /// <remarks/>
        public long ViewSessionScreenTimeoutTicks { get; set; }

        /// <remarks/>
        public long ViewGameInformationTimeoutTicks { get; set; }

        /// <remarks/>
        public long ViewGameRulesTimeoutTicks { get; set; }

        /// <remarks/>
        public long ViewPayTableTimeoutTicks { get; set; }

        /// <remarks/>
        public long SessionTimeoutIntervalTicks { get; set; }

        /// <remarks/>
        public bool SessionTimeoutStartOnZeroCredits { get; set; }

        /// <remarks/>
        public ushort TotalNumberLinkEnrolments { get; set; }

        /// <remarks/>
        public string TotalLinkPercentageContributions { get; set; }

        /// <remarks/>
        public bool ShowLinkJackpotCount { get; set; }

        /// <remarks/>
        public double LinkRTPForGameRTP { get; set; }
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpPidCategoryForceActivationSend : UgpCategorySend
    {
        /// <remarks/>
        public bool IsActive { get; set; }
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpPidCategoryForceActivationReply : UgpCategoryReply
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpPidCategoryActivationStatusChangedSend : UgpCategorySend
    {
        /// <remarks/>
        public bool IsActive { get; set; }
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpPidCategoryActivationStatusChangedReply : UgpCategoryReply
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpPidCategoryGameInformationScreenEnteredSend : UgpCategorySend
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpPidCategoryGameInformationScreenEnteredReply : UgpCategoryReply
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpPidCategorySessionInformationScreenEnteredSend : UgpCategorySend
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpPidCategorySessionInformationScreenEnteredReply : UgpCategoryReply
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpPidCategoryAttendantServiceRequestedSend : UgpCategorySend
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpPidCategoryAttendantServiceRequestedReply : UgpCategoryReply
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpPidCategoryRequestForcePayoutSend : UgpCategorySend
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpPidCategoryRequestForcePayoutReply : UgpCategoryReply
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpPidCategoryConfigurationChangedSend : UgpCategorySend
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpPidCategoryConfigurationChangedReply : UgpCategoryReply
    {
    }
}