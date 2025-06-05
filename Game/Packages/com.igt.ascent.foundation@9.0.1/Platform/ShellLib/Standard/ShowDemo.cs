// -----------------------------------------------------------------------
// <copyright file = "ShowDemo.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Standard
{
    using Game.Core.Communication.Foundation.F2X;
    using Game.Core.Communication.Foundation.F2X.Schemas.Internal.Types;
    using Interfaces;
    using Platform.Interfaces;

    /// <summary>
    /// Implementation of the <see cref="IShowDemo"/> that uses
    /// F2X to communicate with the Foundation to support the show demo status.
    /// </summary>
    internal sealed class ShowDemo : IShowDemo, IContextCache<IShellContext>
    {
        #region Private Fields

        /// <summary>
        /// The interface for the show demo category.
        /// </summary>
        /// <remarks>
        /// This category will be null on a release machine in the field.
        /// It only has a value on development and show machines.
        /// </remarks>
        private IShowDemoCategory showDemoCategory;

        #endregion

        #region Public methods

        /// <summary>
        /// Initializes the instance of <see cref="ShowDemo"/> whose values become available after construction,
        /// e.g. when a connection is established with the Foundation.
        /// </summary>
        /// <param name="category">
        /// The category interface for communicating with the Foundation.
        /// </param>
        public void Initialize(IShowDemoCategory category)
        {
            IsAvailable = category != null;

            showDemoCategory = category;
        }

        #endregion

        #region IContextCache Implementation

        /// <inheritdoc/>
        public void NewContext(IShellContext shellContext)
        {
            ShowDemoEnvironment = IsAvailable ? (ShowDemoEnvironment)showDemoCategory.GetShowDemoProperties().Environment :
                                                ShowDemoEnvironment.Unknown;
        }

        #endregion

        #region IShowDemo Implementation

        /// <inheritdoc/>
        public bool IsAvailable { get; private set; }

        /// <inheritdoc/>
        /// <remarks>
        /// Returns <see cref="Platform.Interfaces.ShowDemoEnvironment.Unknown"/> if the category is null (release environment).
        /// </remarks>
        public ShowDemoEnvironment ShowDemoEnvironment { get; private set; }

        /// <inheritdoc/>
        public void AddMoney(long amount)
        {
            if(IsAvailable)
            {
                var amountToAdd = new Amount(amount);
                showDemoCategory.AddMoney(amountToAdd);
            }
        }

        #endregion
    }
}