// -----------------------------------------------------------------------
// <copyright file = "ILogicalOperand.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.LogicalOperations
{
    /// <summary>
    /// This interface defines an operand that executes on data of type <typeparamref name="T"/>,
    /// and returns a Boolean flag as the result, which could be part of a logical expression.
    /// </summary>
    /// <typeparam name="T">
    /// The data type this operand works on.
    /// </typeparam>
    public interface ILogicalOperand<in T>
    {
        /// <summary>
        /// Executes on the <paramref name="data"/> passed in, and returns a Boolean flag as the result.
        /// </summary>
        /// <param name="data">
        /// The data to work on.
        /// </param>
        /// <returns>
        /// A flag indicating the Boolean result of the execution.
        /// </returns>
        bool Execute(T data);
    }
}