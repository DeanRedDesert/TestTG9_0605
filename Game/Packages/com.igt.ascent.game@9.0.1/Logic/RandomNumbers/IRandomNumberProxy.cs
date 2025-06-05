//-----------------------------------------------------------------------
// <copyright file = "IRandomNumberProxy.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.RandomNumbers
{
    using Evaluator;

    /// <summary>
    /// An interface that supports all of the requirements for drawing random numbers that are used to determine the
    /// outcome of a game.
    /// </summary>
    /// <remarks>
    /// The <see cref="RandomNumberProxy"/> implementation stores only a limited number of
    /// random numbers for audit.  When more numbers than the limit are generated before
    /// AuditNumbers() is called, older numbers will be overwritten by newer numbers.
    /// </remarks>
    public interface IRandomNumberProxy : IAuditNumbers, IAuditlessRandomNumberProxy
    {
    }
}