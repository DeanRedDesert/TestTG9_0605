//-----------------------------------------------------------------------
// <copyright file = "ActionRequestCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using F2X;

    /// <summary>
    /// This class implements callback methods supported by the F2X
    /// Action Request API category.
    /// </summary>
    internal class ActionRequestCallbackHandler : IActionRequestCategoryCallbacks
    {
        /// <summary>
        /// The callback interface for handling transactions.
        /// </summary>
        private readonly ITransactionCallbacks transactionCallbacks;

        /// <summary>
        /// Initializes an instance of <see cref="ActionRequestCallbackHandler"/>.
        /// </summary>
        /// <param name="transactionCallbacks">The callback interface for handling transactions.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="transactionCallbacks"/> is null.
        /// </exception>
        public ActionRequestCallbackHandler(ITransactionCallbacks transactionCallbacks)
        {
            if(transactionCallbacks == null)
            {
                throw new ArgumentNullException("transactionCallbacks");
            }

            this.transactionCallbacks = transactionCallbacks;
        }

        #region IActionRequestCategoryCallbacks Members

        /// <inheritdoc/>
        public string ProcessActionResponse(byte[] payload)
        {
            transactionCallbacks.ProcessActionResponse(payload);

            return null;
        }

        #endregion
    }
}
