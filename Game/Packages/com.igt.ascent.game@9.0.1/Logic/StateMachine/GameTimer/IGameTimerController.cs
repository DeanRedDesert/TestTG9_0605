//-----------------------------------------------------------------------
// <copyright file = "IGameTimerController.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine.GameTimer
{
    using System;

    /// <summary>
    /// An interface that manages timers that can be persisted and have their events raised on the game logic thread.
    /// </summary>
    public interface IGameTimerController
    {
        #region Methods

        /// <summary>
        /// Creates a new timer. If a timer of the same name exists in memory, it is overwritten.
        /// </summary>
        /// <param name="name">The name of this timer.</param>
        /// <param name="baseValue">The number of tick counts that this timer will count up/down from.</param>
        /// <param name="targetValue">The number of tick counts that this timer will count up/down to.</param>
        /// <param name="tickInterval">The time interval (in seconds) that this timer will tick at.</param>
        /// <param name="persistenceInterval">The interval (in seconds) that this timer will be persisted at; 0 if no
        /// persistence is needed.</param>
        /// <returns>An instance of <see cref="IGameTimer"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is null or empty.</exception>
        IGameTimer CreateTimer(string name, uint baseValue, uint targetValue, uint tickInterval, uint persistenceInterval);

        /// <summary>
        /// Loads and overwrites timer states for timers that are persistent. If no previously persisted data is found in
        /// critical data then the existing timer state is left unchanged.
        /// </summary>
        void LoadTimerStates();

        /// <summary>
        /// Gets an existing timer in memory.
        /// </summary>
        /// <param name="name">The name of this timer.</param>
        /// <returns>An instance of <see cref="IGameTimer"/> if the timer was found in memory; null otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if name is null or empty.</exception>
        IGameTimer GetTimer(string name);

        /// <summary>
        /// Removes an existing timer. Any persisted timer state will also be removed from safe storage.
        /// The timer must not be active if it is to be removed; it needs to be stopped or reset first.
        /// </summary>
        /// <param name="name">The name of the timer to remove.</param>
        /// <returns>True if the timer was successfully removed; false if it did not exist or it exists but is
        /// currently active.
        /// </returns>
        bool RemoveTimer(string name);

        /// <summary>
        /// Initializes an existing game timer controller and its game timers.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Pause all existing timers. Paused timers are not incrementing or decrementing,
        /// none of their events are being raised, and no persistence actions are being taken.
        /// </summary>
        void PauseAll();

        /// <summary>
        /// Resume the last status of all timers before the <see cref="PauseAll()"/> method was called.
        /// </summary>
        void ResumeAll();

        /// <summary>
        /// A flag indicating initialization; true means the timer controller has been initialized, false means it
        /// has not, or has been disposed and needs to be initialized again.
        /// </summary>
        bool Initialized { get; }

        /// <summary>
        /// A flag indicating that all existing timers are paused as a result of the <see cref="PauseAll"/> method above.
        /// </summary>
        bool Paused { get; }

        #endregion
    }
}
