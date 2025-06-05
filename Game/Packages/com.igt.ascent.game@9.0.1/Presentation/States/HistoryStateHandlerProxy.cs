//-----------------------------------------------------------------------
// <copyright file = "HistoryStateHandlerProxy.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.States
{
    using System;
    using Communication.CommunicationLib;

    /// <summary>
    /// A proxy for a state handler that supports both the <see cref="IStateHandler"/> and 
    /// <see cref="IHistoryPresentationState"/> interfaces.
    /// </summary>
    public class HistoryStateHandlerProxy : StateHandlerProxy, IHistoryPresentationState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryStateHandlerProxy"/> class.
        /// </summary>
        /// <param name="handlerRegistry">
        /// The registry for the history state handler.  Note that the <see cref="StateHandlerRegistry.IsHistory"/>
        /// property must be <b>true</b>.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <see cref="StateHandlerRegistry.IsHistory"/> does not return true for the object passed via the
        /// <paramref name="handlerRegistry"/> parameter.
        /// </exception>
        public HistoryStateHandlerProxy(StateHandlerRegistry handlerRegistry)
            : base(handlerRegistry)
        {
            if(!handlerRegistry.IsHistory)
            {
                throw new ArgumentException("The registry's IsHistory property must be true.");
            }
        }

        /// <summary>
        /// Returns the <see cref="IHistoryPresentationState"/> instance for this proxy.
        /// </summary>
        /// <exception cref="InvalidCastException">
        /// Thrown if the registered state handler does not support the <see cref="IHistoryPresentationState"/>
        /// interface.
        /// </exception>
        protected IHistoryPresentationState HistoryStateHandler => (IHistoryPresentationState)StateHandler;

        #region Implementation of IHistoryPresentationState

        /// <inheritdoc/>
        public bool HistoryMode
        {
            get => HistoryStateHandler.HistoryMode;
            set => HistoryStateHandler.HistoryMode = value;
        }

        /// <inheritdoc/>
        public bool RecoveryMode
        {
            get => HistoryStateHandler.RecoveryMode;
            set => HistoryStateHandler.RecoveryMode = value;
        }

        /// <inheritdoc/>
        public HistoryNextStepTrigger HistoryNextStepTrigger => HistoryStateHandler.HistoryNextStepTrigger;

        /// <inheritdoc/>
        public override void OnEnter(DataItems stateData)
        {
            base.OnEnter(stateData);
            if(HistoryMode)
            {
                ShowHistoryState();
            }
        }

        /// <inheritdoc/>
        public override void OnExit()
        {
            base.OnExit();

            // Validate StateHandler first, because OnExit might be called after
            // the handler has been destroyed.
            if(HistoryStateHandler != null && HistoryMode)
            {
                HideHistoryState();
            }
        }

        /// <inheritdoc/>
        public void ShowHistoryState()
        {
            HistoryStateHandler.ShowHistoryState();
        }

        /// <inheritdoc/>
        public void HideHistoryState()
        {
            HistoryStateHandler.HideHistoryState();
        }

        /// <inheritdoc/>
        public void OnHistoryStepsChanged(object sender, HistoryStepsChangedEventArgs eventArgs)
        {
            HistoryStateHandler.OnHistoryStepsChanged(sender, eventArgs);
        }

        /// <inheritdoc/>
        public void SetHistoryPresentationCompleteDelegate(HistoryPresentationCompleteDelegate historyPresentationCompleteDelegate)
        {
            HistoryStateHandler.SetHistoryPresentationCompleteDelegate(historyPresentationCompleteDelegate);
        }

        #endregion
    }
}
