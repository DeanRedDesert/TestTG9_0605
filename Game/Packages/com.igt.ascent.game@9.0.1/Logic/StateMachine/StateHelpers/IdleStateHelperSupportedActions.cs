//-----------------------------------------------------------------------
// <copyright file = "IdleStateHelperSupportedActions.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine.StateHelpers
{
    /// <summary>
    /// This class contains a list of actions supported by the Idle State
    /// </summary>
    public static class IdleStateHelperSupportedActions
    {
        ///<summary>
        /// Requests starting a game.
        ///</summary>
        public const string StartGameRequest = "StartGame";

        ///<summary>
        /// Requests changing the Auto-play state.  Requires generic data.
        ///</summary>
        public const string AutoPlayChangeStateRequest = "AutoPlayStateChanged";

        ///<summary>
        /// Starts auto play.
        ///</summary>
        public const string AutoPlayStart = "AutoPlayStart";

        ///<summary>
        /// Requests auto play.
        ///</summary>
        public const string AutoPlayStop = "AutoPlayStop";

        /// <summary>
        /// Continue auto play.
        /// </summary>
        public const string AutoPlayContinue = "AutoPlayContinue";

        ///<summary>
        /// Requests a cash out.
        ///</summary>
        public const string CashOutRequest = "CashOut";

        ///<summary>
        /// Requests a language change.  Requires generic data.
        ///</summary>
        public const string ChangeLanguageRequest = "ChangeLanguage";

        ///<summary>
        /// Requests showing the theme selection menu.
        ///</summary>
        public const string ShowThemeSelectionMenuRequest = "ShowThemeSelectionMenu";

        ///<summary>
        /// Requests a denomination change.  Requires generic data.
        ///</summary>
        public const string ChangeDenominationRequest = "ChangeDenomination";

        /// <summary>
        /// Notify the Logic to handle a Transfer request.
        /// </summary>
        public const string TransferBankToWagerableRequest = "TransferBankToWagerable";

        /// <summary>
        /// Requests to playoff residual credits.
        /// </summary>
        public const string RoundWagerUpPlayoffRequest = "RoundWagerUpPlayoffRequest";

        /// <summary>
        /// Requests to cycle the bet. This is intended to not commit the bet just update
        /// the current bet data.
        /// </summary>
        public const string CycleBetRequest = "CycleBetRequest";
    }
}
