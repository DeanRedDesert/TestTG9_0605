//-----------------------------------------------------------------------
// <copyright file = "IStandaloneHelperUgpMachineConfiguration.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MachineConfiguration
{
    /// <summary>
    /// Standalone helper interface for UGP machine configuration.
    /// </summary>
    public interface IStandaloneHelperUgpMachineConfiguration
    {
        /// <summary>
        /// Sets the clock configuration.
        /// </summary>
        /// <param name="isClockVisible">
        /// The flag indicating if the clock is visible.
        /// </param>
        /// <param name="clockFormat">
        /// The clock format to set.
        /// </param>
        void SetClockConfiguration(bool isClockVisible, string clockFormat);

        /// <summary>
        /// Sets the tokenisation.
        /// </summary>
        /// <param name="tokenisation">The tokenisation value in cents.</param>
        void SetTokenisation(long tokenisation);

        /// <summary>
        /// Sets the game cycle time.
        /// </summary>
        /// <param name="gameCycleTime">
        /// The game cycle time.
        /// </param>
        void SetGameCycleTime(long gameCycleTime);

        /// <summary>
        /// Sets the continuous play allowed flag.
        /// </summary>
        /// <param name="continuousPlayAllowed">
        /// The tokenisation value in cents.
        /// </param>
        void SetContinuousPlayAllowed(bool continuousPlayAllowed);

        /// <summary>
        /// Sets the flag indicating if feature auto start is enabled.
        /// </summary>
        /// <param name="featureAutoStartEnabled">
        /// The flag indicating if feature auto start is enabled.
        /// </param>
        void SetFeatureAutoStartEnabled(bool featureAutoStartEnabled);

        /// <summary>
        /// Sets the current maximum bet.
        /// </summary>
        /// <param name="currentMaximumBet">
        /// The tokenisation value in cents.
        /// </param>
        void SetCurrentMaximumBet(long currentMaximumBet);

        /// <summary>
        /// Sets the win cap style.
        /// </summary>
        /// <param name="winCapStyle">
        /// The win cap style to use.
        /// </param>
        void SetWinCapStyle(UgpMachineConfigurationWinCapStyle winCapStyle);

        /// <summary>
        /// Sets the flag indicating if slam spin is allowed.
        /// </summary>
        /// <param name="slamSpinAllowed">
        /// The flag indicating if slam spin is allowed.
        /// </param>
        void SetSlamSpinAllowed(bool slamSpinAllowed);

		/// <summary>
		/// Sets the machine configuration.
		/// </summary>
		/// <param name="isClockVisible">
		/// The flag indicating if the clock is visible.
		/// </param>
		/// <param name="clockFormat">
		/// The format of the time on the clock.
		/// </param>
		/// <param name="tokenisation">
		/// The tokenisation value (the cents value of one token).
		/// </param>
		/// <param name="gameCycleTime">
		/// The game cycle time in milli-seconds.
		/// </param>
		/// <param name="isContinuousPlayAllowed">
		/// The flag indicating if continuous play is allowed.
		/// </param>
		/// <param name="isFeatureAutoStartEnabled">
		/// The flag indicating if the features should automatically start.
		/// </param>
		/// <param name="currentMaximumBet">
		/// The current maximum bet (in cents).
		/// </param>
		/// <param name="winCapStyle">
		/// The win cap style.
		/// </param>
		/// <param name="isSlamSpinAllowed">
		/// The flag indicating if slam spin is allowed.
		/// </param>
		/// <param name="qcomJurisdiction">
		/// The QCOM defined jurisdiction.
		/// </param>
		/// <param name="cabinetId">
		/// The detected CabinetId.
		/// </param>
		/// <param name="brainBoxId">
		/// The detected Brain Box Id.
		/// </param>
		/// <param name="gpu">
		/// The detected GPU name.
		/// </param>
		void SetMachineConfiguration(bool isClockVisible, string clockFormat, long tokenisation,
			long gameCycleTime, bool isContinuousPlayAllowed, bool isFeatureAutoStartEnabled,
			long currentMaximumBet, UgpMachineConfigurationWinCapStyle winCapStyle, bool isSlamSpinAllowed,
			int qcomJurisdiction, string cabinetId, string brainBoxId, string gpu);
    }
}