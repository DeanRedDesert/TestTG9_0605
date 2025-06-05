//-----------------------------------------------------------------------
// <copyright file = "IAuditlessRandomNumberProxy.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.RandomNumbers
{
    using Evaluator;

    /// <summary>
    /// An interface that supports requirements for drawing random numbers that are used to determine the
    /// outcome of a game, but does not support auditing the random numbers.
    /// </summary>
    public interface IAuditlessRandomNumberProxy : IRandomNumbers, INumberSequence
    {
    }
}