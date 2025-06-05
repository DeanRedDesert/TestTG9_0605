// -----------------------------------------------------------------------
// <copyright file = "ButtonFunction.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// This type enumerates the cabinet button functions.
    /// </summary>
    public enum ButtonFunction
    {
        /// <summary>
        /// The foundation is unable to discern the button's functionality.
        /// </summary>
        Unknown,

        /// <summary>
        /// Button does nothing (no button insert).
        /// </summary>
        NoFunction,

        /// <summary>
        /// Accept relevant action or information.
        /// </summary>
        Accept,

        /// <summary>
        /// Start autoplay.
        /// </summary>
        AutoPlay,

        /// <summary>
        /// Increase current bet by one.
        /// </summary>
        BetOne,

        /// <summary>
        /// Bet n per line button 1.
        /// </summary>
        BetPerLine1,

        /// <summary>
        /// Bet n per line button 2.
        /// </summary>
        BetPerLine2,

        /// <summary>
        /// Bet n per line button 3.
        /// </summary>
        BetPerLine3,

        /// <summary>
        /// Bet n per line button 4.
        /// </summary>
        BetPerLine4,

        /// <summary>
        /// Bet n per line button 5.
        /// </summary>
        BetPerLine5,

        /// <summary>
        /// Increase lines or bet.
        /// </summary>
        BetPlus,

        /// <summary>
        /// Decrease lines or bet.
        /// </summary>
        BetMinus,

        /// <summary>
        /// Choose black while playing gamble feature.
        /// </summary>
        Black,

        /// <summary>
        /// Cash out the player credits.
        /// </summary>
        CashOut,

        /// <summary>
        /// Normal game start (initiated by handle).
        /// </summary>
        HandleSwitch,

        /// <summary>
        /// Start gamble feature.
        /// </summary>
        Gamble,

        /// <summary>
        /// Open the game rules window.
        /// </summary>
        GameRules,

        /// <summary>
        /// Service button for Australia.
        /// </summary>
        Info,

        /// <summary>
        /// Start a game with maximum bet.
        /// </summary>
        MaxBet,

        /// <summary>
        /// Open the theme selection menu.
        /// </summary>
        MoreGames,

        /// <summary>
        /// Choose red while playing gamble feature.
        /// </summary>
        Red,

        /// <summary>
        /// Reject relevant action or information.
        /// </summary>
        Reject,

        /// <summary>
        /// Start a game with the bet of the last game.
        /// </summary>
        RepeatBet,

        /// <summary>
        /// Select n lines button 1.
        /// </summary>
        SelectLines1,

        /// <summary>
        /// Select n lines button 2.
        /// </summary>
        SelectLines2,

        /// <summary>
        /// Select n lines button 3.
        /// </summary>
        SelectLines3,

        /// <summary>
        /// Select n lines button 4.
        /// </summary>
        SelectLines4,

        /// <summary>
        /// Select n lines button 5.
        /// </summary>
        SelectLines5,

        /// <summary>
        /// Signal for service.
        /// </summary>
        Service,

        /// <summary>
        /// Open the game's gaff menu.
        /// </summary>
        ShowGaff,

        /// <summary>
        /// Normal game start.
        /// </summary>
        Start,

        /// <summary>
        /// Take win when playing the gamble feature.
        /// </summary>
        TakeWin,

        /// <summary>
        /// Open the tournament menu (if applicable).
        /// </summary>
        TournamentMenu
    }
}