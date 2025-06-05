// -----------------------------------------------------------------------
// <copyright file = "ITransactionWeightVerificationDependency.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    using System;

    /// <summary>
    /// This interface defines APIs for validating transaction weight.
    /// </summary>
    public interface ITransactionWeightVerificationDependency
    {
        /// <summary>
        /// Check if an open transaction, either heavyweight or lightweight, is available for the operation,
        /// </summary>
        /// <remarks>
        /// Only use this method when you are sure that a lightweight transaction would also work for your operation.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when there is no open transaction available.
        /// The exact type of the exception will depend on the implementation.
        /// </exception>
        void MustHaveOpenTransaction();

        /// <summary>
        /// Check if a heavyweight transaction is available for the operation.
        /// </summary>
        /// <remarks>
        /// Usually the operations in interface extensions would require a heavyweight transaction.
        /// Use this method when you are not sure whether your operations can work with lightweight transactions.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when there is no open transaction available.
        /// The exact type of the exception will depend on the implementation.
        /// </exception>
        void MustHaveHeavyweightTransaction();
    }
}