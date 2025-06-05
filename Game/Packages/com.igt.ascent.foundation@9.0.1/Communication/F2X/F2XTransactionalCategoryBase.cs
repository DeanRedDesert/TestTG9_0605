//-----------------------------------------------------------------------
// <copyright file = "F2XTransactionalCategoryBase.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2X
{
    using System;
    using F2XTransport;

    /// <summary>
    /// Class which extends category support to include transactions.
    /// </summary>
    /// <typeparam name="TCategory">The category type.</typeparam>
    public abstract class F2XTransactionalCategoryBase<TCategory> : F2XCategoryBase<TCategory> where TCategory : class, ICategory, new()
    {
        /// <summary>
        /// Construct the category with the given transport.
        /// </summary>
        /// <param name="transport">Transport that this handler will be installed in.</param>
        protected F2XTransactionalCategoryBase(IF2XTransport transport)
            : base(transport)
        {
        }

        /// <summary>
        /// Create a message containing a request. Overridden to support transaction IDs.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <returns>A message containing the request.</returns>
        protected virtual TCategory CreateTransactionalRequest<TRequest>() where TRequest : class, new()
        {
            var message = base.CreateBasicRequest<TRequest>();

            //Use reflection to set the transaction id of the request.
            var transactionIdProperty = typeof(TRequest).GetProperty("TransactionID");

            if(transactionIdProperty == null)
            {
                throw new InvalidOperationException("The given request type does not have a \"TransactionID\"" +
                    " property.");
            }
            var setter = transactionIdProperty.GetSetMethod();
            setter.Invoke(message.Message.Item, new object[] { Transport.TransactionId });

            return message;
        }
    }
}
