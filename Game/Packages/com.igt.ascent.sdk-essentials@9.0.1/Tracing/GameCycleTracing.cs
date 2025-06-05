// -----------------------------------------------------------------------
// <copyright file = "GameCycleTracing.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Tracing
{
    using EventDefinitions;

    /// <summary>
    /// This class provides APIs for tracing a game cycle
    /// to measure the rate of play performance, such as:
    /// <list type="bullet">
    ///     <item>Game Start Time</item>
    ///     <item>Game Slam Time (if applicable)</item>
    ///     <item>Game End Time</item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Different types of games could emit different sets of events.  For example,
    /// Slot games emit events of reel spin while Poker games emit something else.
    /// It is up to the event consumers to pick the right events for processing.
    /// </para>
    /// <para>
    /// A typical losing slot game cycle consists of the following events:
    /// <list type="bullet">
    ///     <item><see cref="BetButtonTriggered"/></item>
    ///     <item><see cref="ReelSpinStart"/></item>
    ///     <item><see cref="SlamButtonTriggered"/> (Optional)</item>
    ///     <item><see cref="ReelSpinStop"/></item>
    ///     <item><see cref="BetButtonReady"/></item>
    /// </list>
    /// </para>
    /// <para>
    /// A typical winning slot game cycle consists of the following events:
    /// <list type="bullet">
    ///     <item><see cref="BetButtonTriggered"/></item>
    ///     <item><see cref="ReelSpinStart"/></item>
    ///     <item><see cref="SlamButtonTriggered"/>(Optional)</item>
    ///     <item><see cref="ReelSpinStop"/></item>
    ///     <item><see cref="WinCelebrationStart"/></item>
    ///     <item><see cref="WinCelebrationStop"/></item>
    ///     <item><see cref="BetButtonReady"/></item>
    /// </list>
    /// </para>
    /// <para>
    /// Events for when a bonus game is entered and exited can be called using the following events:
    ///     <item><see cref="BonusTriggered"/></item>
    ///     <item><see cref="BonusComplete"/></item>
    /// This will allow for bonus games times to be filtered out 
    /// of overall rate of play time metrics if desired.
    /// </para>
    /// <para>
    /// Rate of play performance is measured based on the time stamps of the above events.
    /// For example, Game Start Time is the duration between <see cref="BetButtonTriggered"/>
    /// and <see cref="ReelSpinStart"/>.
    /// </para>
    /// </remarks>
    public sealed class GameCycleTracing
    {
        #region Singleton Implementation

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static GameCycleTracing Log { get; } = new GameCycleTracing();

        /// <summary>
        /// Private constructor.
        /// </summary>
        private GameCycleTracing()
        {
        }

        #endregion

        #region Tracing Methods

        /// <summary>
        /// Tracing event indicating that the bet button is ready for starting a new game cycle.
        /// </summary>
        /// <remarks>
        /// This event marks the end of the Game Slam Time and Game End Time.
        /// </remarks>
        /// <param name="callSite">The call site emitting the tracing event.</param>
        public void BetButtonReady(CallSite callSite)
        {
            GameCycleTracingEventSource.Log.BetButtonReady(callSite);
        }

        /// <summary>
        /// Tracing event indicating that a bet button has been pressed to start a new game cycle.
        /// </summary>
        /// <remarks>
        /// This event marks the start of the Game Start Time.
        /// </remarks>
        /// <param name="callSite">The call site emitting the tracing event.</param>
        /// <param name="buttonActionTimeStamp">
        /// The time stamp associated with the physical action (on press or on release) that
        /// eventually triggers the function(s) of a button (physical button or on screen button).
        /// This is mostly used to correlate a button trigger with the IO event in Foundation
        /// for more precise game cycle calculation.
        /// </param>
        public void BetButtonTriggered(CallSite callSite, long buttonActionTimeStamp)
        {
            GameCycleTracingEventSource.Log.BetButtonTriggered(callSite, buttonActionTimeStamp);
        }

        /// <summary>
        /// Tracing event indicating that the reels start spinning.
        /// </summary>
        /// <remarks>
        /// This event marks the end of the Game Start Time.
        /// </remarks>
        /// <param name="callSite">The call site emitting the tracing event.</param>
        public void ReelSpinStart(CallSite callSite)
        {
            GameCycleTracingEventSource.Log.ReelSpinStart(callSite);
        }

        /// <summary>
        /// Tracing event indicating that the reels stop spinning.
        /// </summary>
        /// <remarks>
        /// This event marks the start of the Game End Time for a losing game.
        /// </remarks>
        /// <param name="callSite">The call site emitting the tracing event.</param>
        public void ReelSpinStop(CallSite callSite)
        {
            GameCycleTracingEventSource.Log.ReelSpinStop(callSite);
        }

        /// <summary>
        /// Tracing event indicating that a slam button has been pressed.
        /// </summary>
        /// <remarks>
        /// This event marks the start of the Game Slam Time.
        /// </remarks>
        /// <param name="callSite">The call site emitting the tracing event.</param>
        /// <param name="buttonActionTimeStamp">
        /// The time stamp associated with the physical action (on press or on release) that
        /// eventually triggers the function(s) of a button (physical button or on screen button).
        /// This is mostly used to correlate a button trigger with the IO event in Foundation
        /// for more precise game cycle calculation.
        /// </param>
        public void SlamButtonTriggered(CallSite callSite, long buttonActionTimeStamp)
        {
            GameCycleTracingEventSource.Log.SlamButtonTriggered(callSite, buttonActionTimeStamp);
        }

        /// <summary>
        /// Tracing event indicating that the win celebration starts.
        /// </summary>
        /// <remarks>
        /// This event is currently not used in rate of play measurement.
        /// </remarks>
        /// <param name="callSite">The call site emitting the tracing event.</param>
        public void WinCelebrationStart(CallSite callSite)
        {
            GameCycleTracingEventSource.Log.WinCelebrationStart(callSite);
        }

        /// <summary>
        /// Tracing event indicating that the win celebration completes.
        /// </summary>
        /// <remarks>
        /// This event marks the start of the Game End Time for a winning game.
        /// </remarks>
        /// <param name="callSite">The call site emitting the tracing event.</param>
        public void WinCelebrationStop(CallSite callSite)
        {
            GameCycleTracingEventSource.Log.WinCelebrationStop(callSite);
        }

        /// <summary>
        /// Tracing event indicating that a bonus game has been entered in the current game cycle.
        /// </summary>
        /// <param name="callsite">The call site emitting the tracing event.</param>
        public void BonusTriggered(CallSite callsite)
        {
            GameCycleTracingEventSource.Log.BonusTriggered(callsite);
        }

        /// <summary>
        /// Tracing event indicating that a bonus game has completed in the current game cycle.
        /// </summary>
        /// <param name="callsite">The call site emitting the tracing event.</param>
        public void BonusComplete(CallSite callsite)
        {
            GameCycleTracingEventSource.Log.BonusComplete(callsite);
        }

        #endregion
    }
}