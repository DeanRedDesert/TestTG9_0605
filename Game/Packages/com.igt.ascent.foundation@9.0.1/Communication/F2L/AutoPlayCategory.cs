//-----------------------------------------------------------------------
// <copyright file = "AutoPlayCategory.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L
{
    using System;
    using System.Collections.Generic;
    using F2XTransport;
    using Schemas;
    using Schemas.Internal;

    /// <summary>
    /// Implementation of the F2L auto play API category.
    /// </summary>
    public class AutoPlayCategory : F2LTransactionalCategoryBase<AutoPlay>, IAutoPlayCategory, IMultiVersionSupport
    {
        #region Versioning Information

        /// <summary>
        /// The name of the method that will appear in <see cref="methodSupportingVersions"/>.
        /// </summary>
        private const string MethodGetConfigDataAutoPlaySpeedIncreaseAllowed = "GetConfigDataAutoPlaySpeedIncreaseAllowed";

        /// <summary>
        /// A look-up table for the methods that are NOT available in all supported versions.
        /// Keyed by the method name, the value is the version where the method becomes available.
        /// </summary>
        private readonly Dictionary<string, VersionType> methodSupportingVersions = new Dictionary<string, VersionType>
        {
            {MethodGetConfigDataAutoPlaySpeedIncreaseAllowed, new VersionType(1, 1)}
        };

        /// <summary>
        /// All versions supported by this category class.
        /// </summary>
        private readonly List<VersionType> supportedVersions = new List<VersionType>
        {
            new VersionType(1, 0),
            new VersionType(1, 1),
        };

        #endregion

        #region Fields

        /// <summary>
        /// Object which implements the auto play category callbacks.
        /// </summary>
        private readonly IAutoPlayCategoryCallbacks callbackHandler;

        /// <summary>
        /// The version to use for communications by this category.
        /// Initialized to 0.0. Will be set by <see cref="SetVersion"/>.
        /// </summary>
        private VersionType effectiveVersion = new VersionType(0, 0);

        #endregion

        #region Constructor and Initialization

        /// <summary>
        /// Create an instance of the auto play category.
        /// </summary>
        /// <param name="transport">Transport that this handler will be installed in.</param>
        /// <param name="callbackHandler">Auto play category callback handler.</param>
        /// <exception cref="ArgumentNullException">Thrown when the callback handler is null.</exception>
        public AutoPlayCategory(IF2XTransport transport, IAutoPlayCategoryCallbacks callbackHandler)
            : base(transport)
        {
            if(callbackHandler == null)
            {
                throw new ArgumentNullException("callbackHandler", "Argument may not be null.");
            }

            this.callbackHandler = callbackHandler;

            ConfigureHandlers();
        }

        /// <summary>
        /// Configure the handler table for all auto play category messages which can be received.
        /// </summary>
        private void ConfigureHandlers()
        {
            AddMessagehandler<AutoPlayAutoPlayOnRequestSend>(HandleAutoPlayAutoPlayOnRequestSend);
            AddMessagehandler<AutoPlayAutoPlayOffSend>(HandleAutoPlayAutoPlayOffSend);
        }

        #endregion

        #region IApiCategory Overrides

        /// <inheritdoc />
        public override uint MajorVersion
        {
            get { return effectiveVersion.MajorVersion; }
        }

        /// <inheritdoc />
        public override uint MinorVersion
        {
            get { return effectiveVersion.MinorVersion; }
        }

        /// <inheritdoc />
        public override MessageCategory Category
        {
            get { return MessageCategory.AutoPlay; }
        }

        #endregion

        #region Message Handlers

        /// <summary>
        /// Handler for the AutoPlayAutoPlayOnRequestSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleAutoPlayAutoPlayOnRequestSend(AutoPlayAutoPlayOnRequestSend message)
        {
            var successful = callbackHandler.ProcessAutoPlayOnRequest();
            var reply = CreateReply<AutoPlayAutoPlayOnRequestReply>(0, "");
            var autoPlayOnReply = (AutoPlayAutoPlayOnRequestReply)reply.Message.Item;
            autoPlayOnReply.AutoPlayOnRequestSuccessful = successful;

            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handler for the AutoPlayAutoPlayOffSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleAutoPlayAutoPlayOffSend(AutoPlayAutoPlayOffSend message)
        {
            callbackHandler.ProcessAutoPlayOff();
            var reply = CreateReply<AutoPlayAutoPlayOffReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        #endregion

        #region IMultiVersionSupport Members

        /// <inheritdoc/>
        public void SetVersion(uint major, uint minor)
        {
            var version = new VersionType(major, minor);

            if(!supportedVersions.Contains(version))
            {
                throw new ArgumentException(
                    string.Format("{0} is not supported by AutoPlayCategory class.", version));
            }

            effectiveVersion = version;
        }

        #endregion

        #region IAutoPlayCategory Members

        /// <inheritdoc />
        public bool GetConfigDataPlayerAutoPlayEnabled()
        {
            var request = CreateTransactionalRequest<AutoPlayGetConfigDataPlayerAutoPlayEnabledSend>();
            var reply = SendMessageAndGetReply<AutoPlayGetConfigDataPlayerAutoPlayEnabledReply>(Channel.Foundation,
                                                                                                   request);
            CheckReply(reply.Reply);
            return reply.AutoPlayEnabled;
        }

        /// <inheritdoc />
        public bool GetConfigDataPlayerAutoPlayConfirmationRequired()
        {
            var request = CreateTransactionalRequest<AutoPlayGetConfigDataPlayerAutoPlayConfirmationRequiredSend>();
            var reply = SendMessageAndGetReply<AutoPlayGetConfigDataPlayerAutoPlayConfirmationRequiredReply>(Channel.Foundation,
                                                                                                   request);
            CheckReply(reply.Reply);
            return reply.AutoPlayConfirmationRequired;
        }

        /// <inheritdoc/>
        public bool? GetConfigDataAutoPlaySpeedIncreaseAllowed()
        {
            bool? result = null;
            if(IsMethodSupported(MethodGetConfigDataAutoPlaySpeedIncreaseAllowed))
            {
                var request = CreateTransactionalRequest<AutoPlayGetConfigDataAutoPlaySpeedIncreaseAllowedSend>();

                var reply = SendMessageAndGetReply<AutoPlayGetConfigDataAutoPlaySpeedIncreaseAllowedReply>(
                    Channel.Foundation, request);

                CheckReply(reply.Reply);

                result = reply.AutoPlaySpeedIncreaseAllowed;
            }
            return result;
        }

        /// <inheritdoc />
        public bool IsAutoPlayOn()
        {
            var request = CreateTransactionalRequest<AutoPlayIsAutoPlayOnSend>();
            var reply = SendMessageAndGetReply<AutoPlayIsAutoPlayOnReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
            return reply.AutoPlayOn;
        }

        /// <inheritdoc />
        public bool SetAutoPlayOnRequest()
        {
            var request = CreateTransactionalRequest<AutoPlaySetAutoPlayOnRequestSend>();
            var reply = SendMessageAndGetReply<AutoPlaySetAutoPlayOnRequestReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
            return reply.SetSuccessful;
        }

        /// <inheritdoc />
        public void SetAutoPlayOff()
        {
            var request = CreateTransactionalRequest<AutoPlaySetAutoPlayOffSend>();
            var reply = SendMessageAndGetReply<AutoPlaySetAutoPlayOffReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Check if a method is supported by the effective version of the category.
        /// </summary>
        /// <param name="methodName">The name of the method to check.</param>
        /// <returns>True if the method is supported.  False otherwise.</returns>
        private bool IsMethodSupported(string methodName)
        {
            // Methods not in the dictionary are available in all versions.
            var result = true;

            if(methodSupportingVersions.ContainsKey(methodName))
            {
                result = effectiveVersion >= methodSupportingVersions[methodName];
            }

            return result;
        }

        #endregion
    }
}
