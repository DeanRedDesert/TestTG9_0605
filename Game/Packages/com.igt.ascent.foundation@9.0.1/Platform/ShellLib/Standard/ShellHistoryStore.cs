// -----------------------------------------------------------------------
// <copyright file = "ShellHistoryStore.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Game.Core.Communication.Foundation.F2X;
    using Game.Core.Communication.Foundation.F2X.Schemas.Internal.ShellHistoryStore;
    using Interfaces;
    using Platform.Interfaces;
    using Restricted.EventManagement.Interfaces;

    /// <summary>
    /// Implementation of the <see cref="IShellHistoryStore"/> that uses
    /// F2X to communicate with the Foundation to support shell history store.
    /// </summary>
    internal sealed class ShellHistoryStore : ShellHistoryStoreBase
    {
        #region Private Fields

        private readonly CategoryInitializer<IShellHistoryStoreCategory> shellHistoryStoreCategory;

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public ShellHistoryStore(object eventSender,
                                 IEventDispatcher transactionalEventDispatcher,
                                 ICriticalDataStoreAccessValidator storeAccessValidator)
            : base(eventSender, transactionalEventDispatcher, storeAccessValidator)
        {
            shellHistoryStoreCategory = new CategoryInitializer<IShellHistoryStoreCategory>();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Initializes the instance of <see cref="ShellHistoryStore"/> whose values become available after construction,
        /// e.g. when a connection is established with the Foundation.
        /// </summary>
        /// <param name="category">
        /// The category interface for communicating with the Foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="category"/> is null.
        /// </exception>
        public void Initialize(IShellHistoryStoreCategory category)
        {
            if(category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            shellHistoryStoreCategory.Initialize(category);
        }

        #endregion

        #region ShellHistoryStoreBase Overrides

        /// <inheritdoc/>
        public override void NewContext(IShellContext shellContext)
        {
            WritePermittedList = shellHistoryStoreCategory.Instance.GetShellHistoryWritePermitted().ToList();
        }

        /// <inheritdoc/>
        protected override ICriticalDataBlock DoRead(int coplayerId, IList<CriticalDataName> nameList)
        {
            var names = nameList.Select(name => (string)name);
            var contentItems = shellHistoryStoreCategory.Instance.ReadCritData(coplayerId, names);

            return new CriticalDataBlock(contentItems.ToDictionary(contentItem => contentItem.Key,
                                                                   contentItem => contentItem.Value));
        }

        /// <inheritdoc/>
        protected override void DoRemove(int coplayerId, IList<CriticalDataName> nameList)
        {
            var names = nameList.Select(name => (string)name);
            shellHistoryStoreCategory.Instance.RemoveCritData(coplayerId, names);
        }

        /// <inheritdoc/>
        protected override void DoWrite(int coplayerId, ICriticalDataBlock criticalDataBlock)
        {
            // nameList has been validated so far.
            var nameList = criticalDataBlock.GetNameList();

            var itemsToWrite = nameList.Select(name => new CriticalDataItemListItem
                                                           {
                                                               Key = name,
                                                               Value = criticalDataBlock.GetSerializedData(name)
                                                           });

            shellHistoryStoreCategory.Instance.WriteCritData(coplayerId, itemsToWrite);
        }

        #endregion
    }
}