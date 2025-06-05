// -----------------------------------------------------------------------
// <copyright file = "GameCycleStateTransitionedEventArgs.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.CoplayerLib.Interfaces
{
    using System;
    using System.Text;
    using IGT.Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// An <see cref="EventArgs"/> class that contains information about game cycle state transitions.
    /// </summary>
    [Serializable]
    public class GameCycleStateTransitionedEventArgs : NonTransactionalEventArgs
    {
        /// <summary>
        /// Gets the state that was transitioned from.
        /// </summary>
        public GameCycleState FromState { get; private set; }

        /// <summary>
        /// Gets the state that was transitioned to.
        /// </summary>
        public GameCycleState ToState { get; private set; }

        /// <summary>
        /// Creates an instance of <see cref="GameCycleStateTransitionedEventArgs"/> with the give from and to states.
        /// </summary>
        /// <param name="fromState">The state that was transitioned from.</param>
        /// <param name="toState">The state that was transitioned to.</param>
        public GameCycleStateTransitionedEventArgs(GameCycleState fromState, GameCycleState toState)
        {
            FromState = fromState;
            ToState = toState;
        }

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("GameCycleStateTransitionedEventArgs -");
            builder.Append(base.ToString());
            builder.AppendLine("\t FromState: " + FromState);
            builder.AppendLine("\t ToState: " + ToState);

            return builder.ToString();
        }

        #endregion
    }
}
