//-----------------------------------------------------------------------
// <copyright file = "ShowCategory.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L
{
    using System;
    using System.Collections.Generic;
    using F2XTransport;
    using Schemas.Internal;
    using ShowEnvironment = Ascent.Communication.Platform.GameLib.Interfaces.ShowEnvironment;

    /// <summary>
    /// F2L 'Show' category implementation. Handles adding money for a show/demo/gaffe game.
    /// </summary>
    public class ShowCategory : F2LTransactionalCategoryBase<ShowDemo>, IShowCategory, IMultiVersionSupport
    {
        #region Versioning Information

        /// <summary>
        /// The name of the method that will appear in <see cref="methodSupportingVersions"/>.
        /// </summary>
        private const string MethodGetShowEnvironment = "GetShowEnvironment";

        /// <summary>
        /// A look up table for the methods that are NOT available in all supported versions.
        /// Keyed by the method name, the value is the version where the method becomes available.
        /// </summary>
        private readonly Dictionary<string, VersionType> methodSupportingVersions = new Dictionary<string, VersionType>
        {
            {MethodGetShowEnvironment, new VersionType(2, 1)},
        };

        /// <summary>
        /// All versions supported by this category class.
        /// </summary>
        private readonly List<VersionType> supportedVersions = new List<VersionType>
        {
            new VersionType(2, 0),
            new VersionType(2, 1),
        };

        #endregion

        #region Fields

        /// <summary>
        /// The version to use for communications by this category.
        /// Initialized to 0.0.  Will be set by <see cref="SetVersion"/>.
        /// </summary>
        private VersionType effectiveVersion = new VersionType(0, 0);

        #endregion

        #region Constructors

        /// <summary>
        /// Create an instance of the Show Demo category.
        /// </summary>
        /// <param name="transport">The transport object this category is installed in.</param>
        public ShowCategory(IF2XTransport transport)
            : base(transport)
        {
        }

        #endregion

        #region IApiCategory Members

        /// <inheritdoc />
        public override MessageCategory Category
        {
            get { return MessageCategory.Show; }
        }

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

        #endregion

        #region IShowDemoCategory Members

        /// <inheritdoc />
        public bool AddMoney(long value, long denomination)
        {
            long amount;

            checked
            {
                amount = (value * denomination);
            }

            var request = CreateBasicRequest<ShowDemoAddMoneySend>();
            var content = (ShowDemoAddMoneySend)request.Message.Item;
            content.Amount = amount;

            var reply = SendMessageAndGetReply<ShowDemoAddMoneyReply>(Channel.Game, request);
            return reply.Reply.ReplyCode == 0;
        }

        /// <inheritdoc />
        public ShowEnvironment GetShowEnvironment()
        {
            if(IsMethodSupported(MethodGetShowEnvironment))
            {
                var request = CreateTransactionalRequest<ShowDemoGetShowEnvironmentSend>();

                var reply = SendMessageAndGetReply<ShowDemoGetShowEnvironmentReply>(Channel.Foundation, request);
                return (ShowEnvironment)reply.ShowEnvironment;
            }
            return ShowEnvironment.Invalid;
        }

        #endregion

        #region IMultiVersionSupport Members

        /// <inheritdoc />
        public void SetVersion(uint major, uint minor)
        {
            var version = new VersionType(major, minor);

            if(!supportedVersions.Contains(version))
            {
                throw new ArgumentException(
                    string.Format("{0} is not supported by ShowCategory class.", version));
            }

            effectiveVersion = version;
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