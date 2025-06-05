// -----------------------------------------------------------------------
// <copyright file = "CoplayerInitData.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using Communication.Platform.Interfaces;
    using Communication.Platform.ShellLib.Interfaces;
    using Game.Core.Money;

    /// <summary>
    /// Simple wrapper of all the initial data needed for a coplayer to start running.
    /// This includes both static configuration data kept by the shell and
    /// runtime data tracked by the shell.
    /// </summary>
    /// <remarks>
    /// For runtime data, these are the values at the moment of calling.
    /// Future update will be pushed to the coplayer by shell via various shell events.
    /// </remarks>
    internal sealed class CoplayerInitData
    {
        #region Properties

        #region Config Data

        // Static configuration data sent by Foundation to shell, but to be consumed by coplayers.

        public MachineWideBetConstraints MachineWideBetConstraints { get; set; }
        public CreditFormatter CreditFormatter { get; set; }
        public int MinimumBaseGameTime { get; set; }
        public int MinimumFreeSpinTime { get; set; }
        public CreditMeterDisplayBehaviorMode CreditMeterBehavior { get; set; }
        public MaxBetButtonBehavior MaxBetButtonBehavior { get; set; }
        public bool DisplayVideoReelsForStepper { get; set; }

        #endregion

        #region Initial Values of Context Properties

        // Runtime data sent by Foundation to shell, but to be consumed by coplayers.

        public DisplayControlState DisplayControlState { get; set; }
        public bool CanBet { get; set; }
        public bool CanCommitGameCycle { get; set; }
        public long PlayerBettable { get; set; }
        public string Culture { get; set; }

        #endregion

        #endregion
    }
}