//-----------------------------------------------------------------------
// <copyright file = "AutoPlayCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2LLink
{
    using System;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using F2L;
    using F2XTransport;

    /// <summary>
    /// This class is responsible for handling callbacks from the auto play category.
    /// </summary>
    internal class AutoPlayCallbackHandler : IAutoPlayCategoryCallbacks
    {
        /// <summary>
        /// The callback interface for handling events.
        /// </summary>
        private readonly IEventCallbacks callbackInterface;

        /// <summary>
        /// Construct an instance of the AutoPlayCallbackHandler.
        /// </summary>
        /// <param name="callbackHandler">The game lib call back handler used to post the auto play events.</param>
        public AutoPlayCallbackHandler(IEventCallbacks callbackHandler)
        {
            if(callbackHandler == null)
            {
                throw new ArgumentNullException("callbackHandler", "Parameter may not be null.");
            }
            callbackInterface = callbackHandler;
        }

        #region IAutoPlayCategoryCallbacks

        /// <inheritdoc />
        public bool ProcessAutoPlayOnRequest()
        {
            var autoPlayOnEventArgs = new AutoPlayOnRequestEventArgs();
            callbackInterface.PostEvent(autoPlayOnEventArgs);

            return autoPlayOnEventArgs.RequestAccepted;
        }

        /// <inheritdoc />
        public void ProcessAutoPlayOff()
        {
            callbackInterface.PostEvent(new AutoPlayOffEventArgs());
        }

        #endregion
    }
}
