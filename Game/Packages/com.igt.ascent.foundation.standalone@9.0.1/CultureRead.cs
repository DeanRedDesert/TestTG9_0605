// -----------------------------------------------------------------------
// <copyright file = "CultureRead.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Ascent.Communication.Platform.ExtensionLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Money;

    /// <summary>
    /// Implementation of <see cref="ILocalizationInformation"/> interface that provides
    /// the culture for a context.
    /// </summary>
    /// <remarks> 
    /// An internal setter method is available for standalone libs 
    /// to set the culture for a context.
    /// </remarks>
    internal class CultureRead : ICultureRead
    {
        private readonly ITransactionVerification transactionVerification;
        private readonly Dictionary<CultureContext, CultureInfo> cultures;

        /// <summary>
        /// Initializes a new instance of <see cref="CultureRead"/>.
        /// </summary>
        /// <param name="transactionVerification">
        /// The interface for verifying that a transaction is open.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="transactionVerification"/> is null.
        /// </exception>
        public CultureRead(ITransactionVerification transactionVerification)
        {
            this.transactionVerification = transactionVerification ?? throw new ArgumentNullException(nameof(transactionVerification));
            cultures = Enum.GetValues(typeof(CultureContext))
                           .OfType<CultureContext>()
                           .ToDictionary(context => context,
                                         context => new CultureInfo("en-US"));
        }

        /// <summary>
        /// Sets the current culture which is used to create a
        /// <see cref="CreditFormatter"/>.
        /// </summary>
        /// <param name="cultureContext">The context in which the specified culture is intended for.</param>
        /// <param name="cultureName">Culture to use.</param>
        /// <remarks>
        /// This is an internal method allowing standalone Libs to set the culture as needed.
        /// </remarks>
        internal void SetCulture(CultureContext cultureContext, string cultureName)
        {
            if(cultures[cultureContext].Name != cultureName)
            {
                cultures[cultureContext] = new CultureInfo(cultureName);

                CultureChangedEvent?.Invoke(this, new CultureChangedEventArgs(cultureContext, cultureName));
            }
        }

        #region ICultureRead Members

        /// <inheritdoc/>
        public string GetCulture(CultureContext cultureContext)
        {
            transactionVerification.MustHaveOpenTransaction();

            return cultures[cultureContext].Name;
        }

        /// <inheritdoc/>
        public event EventHandler<CultureChangedEventArgs> CultureChangedEvent;

        #endregion
    }
}