// -----------------------------------------------------------------------
// <copyright file = "LogicalAnd.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.LogicalOperations
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A logical AND operator.  When executed, it returns the AND result of all its operands.
    /// </summary>
    /// <typeparam name="T">
    /// The data type this operator and its operands work on.
    /// </typeparam>
    public class LogicalAnd<T> : LogicalOperatorBase<T>
    {
        #region Constructors

        /// <inheritdoc/>
        public LogicalAnd(ILogicalOperand<T> operand1 = null, ILogicalOperand<T> operand2 = null) : base(operand1, operand2)
        {
        }

        /// <inheritdoc/>
        public LogicalAnd(IEnumerable<ILogicalOperand<T>> operands) : base(operands)
        {
        }

        #endregion

        #region LogicalOperatorBase<T> Overrides

        /// <inheritdoc/>
        /// <returns>
        /// True if all of the operands return true, or when there is no operand defined;
        /// False if any of the operands returns false.
        /// </returns>
        public override bool Execute(T data)
        {
            return Operands.All(operand => operand.Execute(data));
        }

        #endregion
    }
}