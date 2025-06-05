// -----------------------------------------------------------------------
// <copyright file = "LogicalOr.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.LogicalOperations
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A logical OR operator.  When executed, it returns the OR result of all its operands.
    /// </summary>
    /// <typeparam name="T">
    /// The data type this operator and its operands work on.
    /// </typeparam>
    public class LogicalOr<T> : LogicalOperatorBase<T>
    {
        #region Constructors

        /// <inheritdoc/>
        public LogicalOr(ILogicalOperand<T> operand1 = null, ILogicalOperand<T> operand2 = null) : base(operand1, operand2)
        {
        }

        /// <inheritdoc/>
        public LogicalOr(IEnumerable<ILogicalOperand<T>> operands) : base(operands)
        {
        }

        #endregion

        #region LogicalOperatorBase<T> Overrides

        /// <inheritdoc/>
        /// <returns>
        /// True if any of the operands returns true;
        /// False if all operands return false, or when there is no operand defined.
        /// </returns>
        public override bool Execute(T data)
        {
            return Operands.Any(operand => operand.Execute(data));
        }

        #endregion
    }
}