//-----------------------------------------------------------------------
// <copyright file = "StateMachineHelper.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine.StateHelpers
{
    /// <summary>
    ///   This class assists in the creation of statemachines by providing common functionality.
    /// </summary>
    public class StateMachineHelper
    {
        ///<summary>
        /// Typical functionality provided for the Idle state.
        ///</summary>
        public virtual IdleStateHelper Idle { get; } = new IdleStateHelper();

        ///<summary>
        /// Typical functionality provided to enroll and commit a game.
        ///</summary>
        public virtual EnrollCommitStateHelper EnrollCommit { get; } = new EnrollCommitStateHelper();

        ///<summary>
        /// Typical functionality provided to adjust an outcome with the foundation.
        ///</summary>
        public virtual AdjustOutcomeStateHelper AdjustOutcome { get; } = new AdjustOutcomeStateHelper();

        ///<summary>
        ///</summary>
        public virtual FinalizeStateHelper Finalize { get; } = new FinalizeStateHelper();

        private readonly DoubleUpOfferStateHelper doubleUpOffer = new DoubleUpOfferStateHelper();

        ///<summary>
        /// Typical functionality provided to DoubleUp offer state.
        ///</summary>
        public virtual DoubleUpOfferStateHelper DoubleUpOffer => doubleUpOffer;

        ///<summary>
        /// Default constructor sets the default handlers for the IdleStateHandler;
        ///</summary>
        public StateMachineHelper ()
        {
            // TODO: Fix virtual member call in ctor
            // ReSharper disable once VirtualMemberCallInConstructor
            Idle.SetDefaultHandlers();
            doubleUpOffer.SetDefaultHandlers();
        }

    }
}
