// -----------------------------------------------------------------------
// <copyright file = "ShellHistoryControl.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Game.Core.Communication.Foundation.F2X;
    using Game.Core.Communication.Foundation.F2XCallbacks;
    using Interfaces;
    using Platform.Interfaces;

    /// <summary>
    /// Implementation of the <see cref="IShellHistoryControl"/> that uses
    /// F2X to communicate with the Foundation to support history control.
    /// </summary>
    internal sealed class ShellHistoryControl : IShellHistoryControl
    {
        #region Private Fields

        private readonly CategoryInitializer<IShellHistoryControlCategory> shellHistoryControlCategory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ShellHistoryStore"/>.
        /// </summary>
        public ShellHistoryControl()
        {
            shellHistoryControlCategory = new CategoryInitializer<IShellHistoryControlCategory>();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Initializes the instance of <see cref="ShellHistoryControl"/> whose values become available after construction,
        /// e.g. when a connection is established with the Foundation.
        /// </summary>
        /// <param name="category">
        /// The category interface for communicating with the Foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="category"/> is null.
        /// </exception>
        public void Initialize(IShellHistoryControlCategory category)
        {
            if(category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            shellHistoryControlCategory.Initialize(category);
        }

        #endregion

        #region IShellHistoryControl Implementation

        /// <inheritdoc/>
        public void BindSessionToHistory(int sessionId)
        {
            if(sessionId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sessionId), "Session must be greater than 0.");
            }

            shellHistoryControlCategory.Instance.BindCoplayerSessionToHistory(sessionId);
        }

        /// <inheritdoc/>
        public HistoryThemeInfo GetHistoryThemeInformation()
        {
            var reply = shellHistoryControlCategory.Instance.GetHistoryContext();

            return new HistoryThemeInfo(reply.ThemeSelector.ThemeIdentifier.ToToken(),
                                        reply.G2SThemeIdentifier,
                                        reply.ThemeTag,
                                        reply.ThemeTagDataFile,
                                        reply.ThemeSelector.Denom);
        }

        /// <inheritdoc/>
        public ICriticalDataBlock ReadCriticalData(IList<string> nameList)
        {
            if(nameList == null || nameList.Count == 0)
            {
                throw new ArgumentException("The name list can not be null or empty.");
            }

            var contentItems = shellHistoryControlCategory.Instance.ReadCritData(nameList);

            return new CriticalDataBlock(contentItems.ToDictionary(contentItem => contentItem.Key,
                                                                   contentItem => contentItem.Value));
        }

        #endregion
    }
}