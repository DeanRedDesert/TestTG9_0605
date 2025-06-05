// -----------------------------------------------------------------------
// <copyright file = "ICanBetOperand.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.BettingPermit.Interfaces
{
    using Game.Core.Money;
    using LogicalOperations;

    /// <summary>
    /// A logical operand that works with data of <see cref="Amount"/> and
    /// is used for game side can-bet logic.
    /// </summary>
    public interface ICanBetOperand : ILogicalOperand<Amount>
    {
    }
}