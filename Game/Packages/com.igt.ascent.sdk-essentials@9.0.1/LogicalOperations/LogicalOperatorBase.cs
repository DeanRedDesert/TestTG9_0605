// -----------------------------------------------------------------------
// <copyright file = "LogicalOperatorBase.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.LogicalOperations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Base class of all <see cref="ILogicalOperator{T}"/> implementations.
    /// </summary>
    /// <typeparam name="T">
    /// The data type this operator and its operands work on.
    /// </typeparam>
    public abstract class LogicalOperatorBase<T> : ILogicalOperator<T>
    {
        #region Private Fields

        /// <summary>
        /// List of operands.
        /// </summary>
        protected readonly List<ILogicalOperand<T>> Operands = new List<ILogicalOperand<T>>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of a logical operator with up to two operands.
        /// </summary>
        /// <param name="operand1">
        /// The first node.
        /// This parameter is optional.  If not specified, it defaults to null.
        /// </param>
        /// <param name="operand2">
        /// The first node.
        /// This parameter is optional.  If not specified, it defaults to null.
        /// </param>
        protected LogicalOperatorBase(ILogicalOperand<T> operand1 = null, ILogicalOperand<T> operand2 = null)
        {
            if(operand1 != null)
            {
                Operands.Add(operand1);
            }

            if(operand2 != null)
            {
                Operands.Add(operand2);
            }
        }

        /// <summary>
        /// Initializes a new instance of a logical operator with a list of operands.
        /// </summary>
        /// <param name="operands">
        /// A list of operands to use with the operator.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="operands"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="operands"/> contains null element(s).
        /// </exception>
        protected LogicalOperatorBase(IEnumerable<ILogicalOperand<T>> operands)
        {
            if(operands == null)
            {
                throw new ArgumentNullException(nameof(operands));
            }

            Operands.AddRange(operands);

            if(Operands.Any(operand => operand == null))
            {
                throw new ArgumentException("There is one or more null elements in the list.", nameof(operands));
            }
        }

        #endregion

        #region ILogicalNode<T> Implementation

        /// <inheritdoc/>
        public abstract bool Execute(T data);

        #endregion

        #region ILogicalOperator<T> Implementation

        /// <inheritdoc/>
        public void AddOperand(ILogicalOperand<T> operand)
        {
            if(operand == null)
            {
                throw new ArgumentNullException(nameof(operand));
            }

            Operands.Add(operand);
        }

        #endregion
    }
}