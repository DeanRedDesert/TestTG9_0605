// -----------------------------------------------------------------------
// <copyright file = "PidSessionData.cs" company = "IGT">
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
    /// This class contains the information of a PID session.
    /// </summary>
    [Serializable]
    public class PidSessionData : ICompactSerializable
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

        #region Constructors

        /// <summary>
        /// Constructs an instance of the <see cref="PidSessionData"/>.
        /// </summary>
        /// <param name="pidSessionDataInfo">
        /// The instance of internal class <see cref="PidSessionDataInfo"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="pidSessionDataInfo"/> is null.
        /// </exception>
        public PidSessionData(PidSessionDataInfo pidSessionDataInfo)
        {
            if(pidSessionDataInfo == null)
            {
                throw new ArgumentNullException(nameof(pidSessionDataInfo));
            }

            IsSessionTrackingActive = pidSessionDataInfo.IsSessionTrackingActive;
            CreditMeterAtStart = pidSessionDataInfo.CreditMeterAtStart;
            AvailableCredits = pidSessionDataInfo.AvailableCredits;
            CashIn = pidSessionDataInfo.CashIn;
            CashOut = pidSessionDataInfo.CashOut;
            CreditsPlayed = pidSessionDataInfo.CreditsPlayed;
            CreditsWon = pidSessionDataInfo.CreditsWon;
            SessionWinOrLoss = pidSessionDataInfo.SessionWinOrLoss;
            IsWinningSession = pidSessionDataInfo.IsWinningSession;
            IsCrown = pidSessionDataInfo.IsCrown;
            SessionStarted = pidSessionDataInfo.SessionStarted;
            SessionDuration = pidSessionDataInfo.SessionDuration;
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

            builder.AppendLine("PID Session Data -");
            builder.AppendLine("\t IsSessionTrackingActive = " + IsSessionTrackingActive);
            builder.AppendLine("\t CreditMeterAtStart = " + CreditMeterAtStart);
            builder.AppendLine("\t AvailableCredits = " + AvailableCredits);
            builder.AppendLine("\t CashIn = " + CashIn);
            builder.AppendLine("\t CashOut = " + CashOut);
            builder.AppendLine("\t CreditsPlayed = " + CreditsPlayed);
            builder.AppendLine("\t CreditsWon = " + CreditsWon);
            builder.AppendLine("\t SessionWinOrLoss = " + SessionWinOrLoss);
            builder.AppendLine("\t IsWinningSession = " + IsWinningSession);
            builder.AppendLine("\t IsCrown = " + IsCrown);
            builder.AppendLine("\t SessionStarted = " + SessionStarted);
            builder.AppendLine("\t SessionDuration = " + SessionDuration);

            return builder.ToString();
        }

        #region ICompactSerializable Members

        /// <summary>
        /// Constructs an instance of the <see cref="PidSessionData"/>.
        /// </summary>
        /// <remarks>
        /// Types implementing ICompactSerializable must have a public parameter-less constructor.
        /// </remarks>
        public PidSessionData()
        {
        }

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, IsSessionTrackingActive);
            CompactSerializer.Write(stream, CreditMeterAtStart);
            CompactSerializer.Write(stream, AvailableCredits);
            CompactSerializer.Write(stream, CashIn);
            CompactSerializer.Write(stream, CashOut);
            CompactSerializer.Write(stream, CreditsPlayed);
            CompactSerializer.Write(stream, CreditsWon);
            CompactSerializer.Write(stream, SessionWinOrLoss);
            CompactSerializer.Write(stream, IsWinningSession);
            CompactSerializer.Write(stream, IsCrown);
            CompactSerializer.Write(stream, SessionStarted.Ticks);
            CompactSerializer.Write(stream, SessionDuration.Ticks);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            IsSessionTrackingActive = CompactSerializer.ReadBool(stream);
            CreditMeterAtStart = CompactSerializer.ReadLong(stream);
            AvailableCredits = CompactSerializer.ReadLong(stream);
            CashIn = CompactSerializer.ReadLong(stream);
            CashOut = CompactSerializer.ReadLong(stream);
            CreditsPlayed = CompactSerializer.ReadLong(stream);
            CreditsWon = CompactSerializer.ReadLong(stream);
            SessionWinOrLoss = CompactSerializer.ReadLong(stream);
            IsWinningSession = CompactSerializer.ReadBool(stream);
            IsCrown = CompactSerializer.ReadBool(stream);
            SessionStarted = new DateTime(CompactSerializer.ReadLong(stream));
            SessionDuration = new TimeSpan(CompactSerializer.ReadLong(stream));
        }

        #endregion
    }
}
