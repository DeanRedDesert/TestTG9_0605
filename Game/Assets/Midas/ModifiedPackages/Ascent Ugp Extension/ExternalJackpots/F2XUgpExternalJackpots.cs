//-----------------------------------------------------------------------
// <copyright file = "F2XUgpExternalJackpots.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ExternalJackpots
{
    using System;
    using Ascent.Restricted.EventManagement.Interfaces;
    using Interfaces;

    /// <summary>
    /// Implementation of the UgpExternalJackpots extended interface that is backed by F2X.
    /// </summary>
    internal class F2XUgpExternalJackpots : IUgpExternalJackpots, IInterfaceExtension
    {
		#region Fields

		/// <summary>
		/// The UgpMachineConfiguration category handler.
		/// </summary>
		private readonly IUgpExternalJackpotsCategory ugpExternalJackpotsCategory;

		#endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="F2XUgpExternalJackpots"/>.
        /// </summary>
		/// <param name="ugpExternalJackpotsCategory">
		/// Category to use to retrieve data from the platform.
		/// </param>
        /// <param name="transactionalEventDispatcher">
        /// Interface for processing a transactional event.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="transactionalEventDispatcher"/> is null.
        /// </exception>
        public F2XUgpExternalJackpots(IUgpExternalJackpotsCategory ugpExternalJackpotsCategory, IEventDispatcher transactionalEventDispatcher)
        {
			if(ugpExternalJackpotsCategory == null)
			{
				throw new ArgumentNullException("ugpExternalJackpotsCategory");
			}
			if(transactionalEventDispatcher == null)
			{
				throw new ArgumentNullException("transactionalEventDispatcher");
			}

			this.ugpExternalJackpotsCategory = ugpExternalJackpotsCategory;

			transactionalEventDispatcher.EventDispatchedEvent +=
				(sender, dispatchedEvent) => dispatchedEvent.RaiseWith(this, ExternalJackpotChanged);
        }

        #endregion

        #region IUgpExternalJackpots Implementation

		/// <inheritdoc/>
		public event EventHandler<ExternalJackpotChangedEventArgs> ExternalJackpotChanged;

		public ExternalJackpots GetExternalJackpots()
		{
			return ugpExternalJackpotsCategory.GetExternalJackpots();
		}
        
        #endregion
	}
}
