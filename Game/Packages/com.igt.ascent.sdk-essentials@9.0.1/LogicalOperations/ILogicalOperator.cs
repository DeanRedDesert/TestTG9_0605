// -----------------------------------------------------------------------
// <copyright file = "ILogicalOperator.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.LogicalOperations
{
    /// <summary>
    /// This interface defines an operator that maintains a list of <see cref="ILogicalOperand{T}"/>,
    /// and is able to execute all its operands on a data of type <typeparamref name="T"/>,
    /// and returns a Boolean flag as the result, which could be part of a logical expression.
    /// </summary>
    /// <typeparam name="T">
    /// The data type this operator and its operands work on.
    /// </typeparam>
    public interface ILogicalOperator<T> : ILogicalOperand<T>
    {
        /// <summary>
        /// Adds an operand to this operator.
        /// </summary>
        /// <param name="operand">
        /// The operand to add.
        /// </param>
        void AddOperand(ILogicalOperand<T> operand);
    }
}