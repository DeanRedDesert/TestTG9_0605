// -----------------------------------------------------------------------
// <copyright file = "GameCycleBettingConfigData.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.CoplayerLib.Interfaces
{
    using System;
    using System.Text;

    /// <summary>
    /// This class represents the config data related to the game cycle betting.
    /// </summary>
    [Serializable]
    public class GameCycleBettingConfigData
    {
        #region Properties

        /// <summary>
        /// Gets the max bet of the current game play, in units of game denomination.
        /// </summary>
        public long MaxBetCredits { get; private set; }

        /// <summary>
        /// Gets the min bet of the current game play, in units of game denomination.
        /// </summary>
        public long MinBetCredits { get; private set; }

        /// <summary>
        /// Gets the min bet for configuring the button panel, in units of game denomination.
        /// 0 if not defined.
        /// </summary>

        public long ButtonPanelMinBet { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="GameCycleBettingConfigData"/>.
        /// </summary>
        /// <param name="maxBetCredits">
        /// The max bet of the current game play, in units of game denomination.
        /// </param>
        /// <param name="minBetCredits">
        /// The min bet of the current game play, in units of game denomination.
        /// </param>
        /// <param name="buttonPanelMinBet">
        /// The min bet for configuring the button panel, in units of game denomination.
        /// </param>
        public GameCycleBettingConfigData(long maxBetCredits, long minBetCredits, long buttonPanelMinBet)
        {
            MaxBetCredits = maxBetCredits;
            MinBetCredits = minBetCredits;
            ButtonPanelMinBet = buttonPanelMinBet;
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("GameCycleBettingConfigData -");
            builder.AppendLine("\t MaxBetCredits: " + MaxBetCredits);
            builder.AppendLine("\t MinBetCredits: " + MinBetCredits);
            builder.AppendLine("\t ButtonPanelMinBet: " + ButtonPanelMinBet);

            return builder.ToString();
        }

        #endregion
    }
}