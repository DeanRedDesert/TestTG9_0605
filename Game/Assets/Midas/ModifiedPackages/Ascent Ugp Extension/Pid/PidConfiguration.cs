// -----------------------------------------------------------------------
// <copyright file = "PidConfiguration.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Pid
{
    using System;
    using System.IO;
    using System.Text;
    using CompactSerialization;

    /// <summary>
    /// This class contains information of PID (Player Information Display) configuration.
    /// </summary>
    [Serializable]
    public class PidConfiguration : ICompactSerializable
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
        public GameInformationDisplayStyle GameInformationDisplayStyle { get; set; }

        /// <summary>
        /// Gets or sets the session tracking option.
        /// </summary>
        public SessionTrackingOption SessionTrackingOption { get; set; }

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

        #region Constructors

        /// <summary>
        /// Constructs an instance of the <see cref="PidConfiguration"/>.
        /// </summary>
        /// <param name="pidConfigurationInfo">
        /// The instance of internal class <see cref="PidConfigurationInfo"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="pidConfigurationInfo"/> is null.
        /// </exception>
        public PidConfiguration(PidConfigurationInfo pidConfigurationInfo)
        {
            if(pidConfigurationInfo is null)
            {
                throw new ArgumentNullException(nameof(pidConfigurationInfo));
            }

            IsMainEntryEnabled = pidConfigurationInfo.IsMainEntryEnabled;
            IsRequestServiceEnabled = pidConfigurationInfo.IsRequestServiceEnabled;
            IsRequestServiceActivated = pidConfigurationInfo.IsRequestServiceActivated;
            GameInformationDisplayStyle = pidConfigurationInfo.GameInformationDisplayStyle.ToPublic();
            SessionTrackingOption = pidConfigurationInfo.SessionTrackingOption.ToPublic();
            IsGameRulesEnabled = pidConfigurationInfo.IsGameRulesEnabled;
            InformationMenuTimeout = pidConfigurationInfo.InformationMenuTimeout;
            SessionStartMessageTimeout = pidConfigurationInfo.SessionStartMessageTimeout;
            ViewSessionScreenTimeout = pidConfigurationInfo.ViewSessionScreenTimeout;
            ViewGameInformationTimeout = pidConfigurationInfo.ViewGameInformationTimeout;
            ViewGameRulesTimeout = pidConfigurationInfo.ViewGameRulesTimeout;
            ViewPayTableTimeout = pidConfigurationInfo.ViewPayTableTimeout;
            SessionTimeoutInterval = pidConfigurationInfo.SessionTimeoutInterval;
            SessionTimeoutStartOnZeroCredits = pidConfigurationInfo.SessionTimeoutStartOnZeroCredits;
            TotalNumberLinkEnrolments = pidConfigurationInfo.TotalNumberLinkEnrolments;
            TotalLinkPercentageContributions = pidConfigurationInfo.TotalLinkPercentageContributions;
            ShowLinkJackpotCount = pidConfigurationInfo.ShowLinkJackpotCount;
            LinkRtpForGameRtp = pidConfigurationInfo.LinkRtpForGameRtp;
        }

        #endregion

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>
        /// A string describing the object.
        /// </returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("PID Configuration -");
            builder.AppendLine("\t IsMainEntryEnabled = " + IsMainEntryEnabled);
            builder.AppendLine("\t IsRequestServiceEnabled = " + IsRequestServiceEnabled);
            builder.AppendLine("\t IsRequestServiceActivated = " + IsRequestServiceActivated);
            builder.AppendLine("\t GameInformationDisplayStyle = " + GameInformationDisplayStyle);
            builder.AppendLine("\t SessionTrackingOption = " + SessionTrackingOption);
            builder.AppendLine("\t IsGameRulesEnabled = " + IsGameRulesEnabled);
            builder.AppendLine("\t InformationMenuTimeout = " + InformationMenuTimeout);
            builder.AppendLine("\t SessionStartMessageTimeout = " + SessionStartMessageTimeout);
            builder.AppendLine("\t ViewSessionScreenTimeout = " + ViewSessionScreenTimeout);
            builder.AppendLine("\t ViewGameInformationTimeout = " + ViewGameInformationTimeout);
            builder.AppendLine("\t ViewGameRulesTimeout = " + ViewGameRulesTimeout);
            builder.AppendLine("\t ViewPayTableTimeout = " + ViewPayTableTimeout);
            builder.AppendLine("\t SessionTimeoutInterval = " + SessionTimeoutInterval);
            builder.AppendLine("\t SessionTimeoutStartOnZeroCredits = " + SessionTimeoutStartOnZeroCredits);
            builder.AppendLine("\t TotalNumberLinkEnrolments = " + TotalNumberLinkEnrolments);
            builder.AppendLine("\t TotalLinkPercentageContributions = " + TotalLinkPercentageContributions);
            builder.AppendLine("\t ShowLinkJackpotCount = " + ShowLinkJackpotCount);
            builder.AppendLine("\t LinkRtpForGameRtp = " + LinkRtpForGameRtp);

            return builder.ToString();
        }

        #region ICompactSerializable Members

        /// <summary>
        /// Constructs an instance of the <see cref="PidConfiguration"/>.
        /// </summary>
        /// <remarks>
        /// Types implementing ICompactSerializable must have a public parameter-less constructor.
        /// </remarks>
        public PidConfiguration()
        {
        }

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, IsMainEntryEnabled);
            CompactSerializer.Write(stream, IsRequestServiceEnabled);
            CompactSerializer.Write(stream, IsRequestServiceActivated);
            CompactSerializer.Write(stream, GameInformationDisplayStyle);
            CompactSerializer.Write(stream, SessionTrackingOption);
            CompactSerializer.Write(stream, IsGameRulesEnabled);
            CompactSerializer.Write(stream, InformationMenuTimeout.Ticks);
            CompactSerializer.Write(stream, SessionStartMessageTimeout.Ticks);
            CompactSerializer.Write(stream, ViewSessionScreenTimeout.Ticks);
            CompactSerializer.Write(stream, ViewGameInformationTimeout.Ticks);
            CompactSerializer.Write(stream, ViewGameRulesTimeout.Ticks);
            CompactSerializer.Write(stream, ViewPayTableTimeout.Ticks);
            CompactSerializer.Write(stream, SessionTimeoutInterval.Ticks);
            CompactSerializer.Write(stream, SessionTimeoutStartOnZeroCredits);
            CompactSerializer.Write(stream, TotalNumberLinkEnrolments);
            CompactSerializer.Write(stream, TotalLinkPercentageContributions);
            CompactSerializer.Write(stream, ShowLinkJackpotCount);
            CompactSerializer.Write(stream, LinkRtpForGameRtp);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            IsMainEntryEnabled = CompactSerializer.ReadBool(stream);
            IsRequestServiceEnabled = CompactSerializer.ReadBool(stream);
            IsRequestServiceActivated = CompactSerializer.ReadBool(stream);
            GameInformationDisplayStyle = CompactSerializer.ReadEnum<GameInformationDisplayStyle>(stream);
            SessionTrackingOption = CompactSerializer.ReadEnum<SessionTrackingOption>(stream);
            IsGameRulesEnabled = CompactSerializer.ReadBool(stream);
            InformationMenuTimeout = new TimeSpan(CompactSerializer.ReadLong(stream));
            SessionStartMessageTimeout = new TimeSpan(CompactSerializer.ReadLong(stream));
            ViewSessionScreenTimeout = new TimeSpan(CompactSerializer.ReadLong(stream));
            ViewGameInformationTimeout = new TimeSpan(CompactSerializer.ReadLong(stream));
            ViewGameRulesTimeout = new TimeSpan(CompactSerializer.ReadLong(stream));
            ViewPayTableTimeout = new TimeSpan(CompactSerializer.ReadLong(stream));
            SessionTimeoutInterval = new TimeSpan(CompactSerializer.ReadLong(stream));
            SessionTimeoutStartOnZeroCredits = CompactSerializer.ReadBool(stream);
            TotalNumberLinkEnrolments = CompactSerializer.ReadUshort(stream);
            TotalLinkPercentageContributions = CompactSerializer.ReadString(stream);
            ShowLinkJackpotCount = CompactSerializer.ReadBool(stream);
            LinkRtpForGameRtp = CompactSerializer.ReadDouble(stream);
        }

        #endregion
    }
}