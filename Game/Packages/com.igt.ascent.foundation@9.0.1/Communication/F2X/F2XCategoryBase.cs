//-----------------------------------------------------------------------
// <copyright file = "F2XCategoryBase.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2X
{
    using F2XTransport;
    using Schemas.Internal.Types;
    using Schemas.Serializers;
    using Version = Schemas.Internal.Types.Version;

    /// <summary>
    /// Class which implements base functionality of the F2X categories.
    /// </summary>
    /// <typeparam name="TCategory">Type of the F2X category.</typeparam>
    public abstract class F2XCategoryBase<TCategory> : CategoryBase<TCategory> where TCategory : class, ICategory, new()
    {
        #region Constructor

        /// <summary>
        /// Construct the F2X category with the given transport.
        /// </summary>
        /// <param name="transport">Transport that this handler will be installed in.</param>
        protected F2XCategoryBase(IF2XTransport transport) : base(transport)
        {
            MessageSerializer = CreateMessageSerializer(XmlSerializerContract.Instance);
        }

        #endregion

        /// <inheritdoc />
        protected override TCategory CreateReply<TReply>(int errorCode, string errorMessage)
        {
            var categoryMessage = new TCategory();

            var reply = new TReply();

            // Create a reply exception only if an error has occurred.
            if(errorCode != 0)
            {
                // Invoke the setter of the Exception property with a reply code and message.
                var replyProperty = typeof(TReply).GetProperty("Exception");
                var setter = replyProperty.GetSetMethod();
                setter.Invoke(reply,
                    new object[] {new ReplyException {ErrorCode = errorCode, ErrorDescription = errorMessage}});
            }

            categoryMessage.Message.Item = reply;
            categoryMessage.Version = new Version(MajorVersion, MinorVersion);

            return categoryMessage;
        }

        /// <inheritdoc />
        protected override TCategory CreateBasicRequest<TRequest>()
        {
            var categoryMessage = new TCategory();

            var request = new TRequest();

            categoryMessage.Message.Item = request;
            categoryMessage.Version = new Version(MajorVersion, MinorVersion);

            return categoryMessage;
        }

        /// <summary>
        /// Check if the given reply contains an error. If so throw a FoundationReplyException.
        /// </summary>
        /// <param name="reply">Reply to check for an error.</param>
        /// <exception cref="FoundationReplyException">
        /// Thrown if an error has occurred from the foundation.
        /// </exception>
        protected virtual void CheckReply(ReplyException reply)
        {
            if(reply != null && reply.ErrorCode != 0)
            {
                throw new FoundationReplyException(reply.ErrorCode, reply.ErrorDescription);
            }
        }
    }
}
