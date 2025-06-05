//-----------------------------------------------------------------------
// <copyright file = "F2LCategoryBase.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L
{
    using F2XTransport;
    using Schemas;
    using Schemas.Internal;
    using Schemas.Serializers;

    /// <summary>
    /// Class which implements base functionality of the F2L categories.
    /// </summary>
    /// <typeparam name="TCategory"></typeparam>
    public abstract class F2LCategoryBase<TCategory> : CategoryBase<TCategory> where TCategory : class, ICategory, new()
    {
        #region Constructor

        /// <summary>
        /// Construct the category with the given transport.
        /// </summary>
        /// <param name="transport">Transport that this handler will be installed in.</param>
        protected F2LCategoryBase(IF2XTransport transport) : base(transport)
        {
            MessageSerializer = CreateMessageSerializer(XmlSerializerContract.Instance);
        }

        #endregion

        /// <inheritdoc />
        protected override TCategory CreateReply<TReply>(int replyCode, string replyMessage)
        {
            var categoryMessage = new TCategory();
            var reply = new TReply();

            //Invoke the setter of the reply property with a reply code.
            var replyProperty = typeof(TReply).GetProperty("Reply");
            var setter = replyProperty.GetSetMethod();
            setter.Invoke(reply,
                          new object[] { new ReplyCodeType { ReplyCode = replyCode, ErrorDescription = replyMessage } });
            categoryMessage.Message.Item = reply;
            categoryMessage.Version = new VersionType(MajorVersion, MinorVersion);
            return categoryMessage;
        }

        /// <inheritdoc />
        protected override TCategory CreateBasicRequest<TRequest>()
        {
            var categoryMessage = new TCategory();
            var request = new TRequest();

            categoryMessage.Message.Item = request;
            categoryMessage.Version = new VersionType(MajorVersion, MinorVersion);

            return categoryMessage;
        }

        /// <summary>
        /// Check if the given reply contains an error. If so throw a FoundationReplyException.
        /// </summary>
        /// <param name="reply">Reply to check for an error.</param>
        protected virtual void CheckReply(ReplyCodeType reply)
        {
            if(reply.ReplyCode != 0)
            {
                throw new FoundationReplyException(reply.ReplyCode, reply.ErrorDescription);
            }
        }
    }
}
