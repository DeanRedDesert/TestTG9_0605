// -----------------------------------------------------------------------
// <copyright file = "AscribedGameExtensionLib.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib.Standard
{
    using System;
    using ExtensionLib.Interfaces;
    using F2XLinks;
    using Interfaces;
    using Restricted.EventManagement.Interfaces;

    /// <summary>
    /// Implementation of <see cref="IAscribedGameExtensionLib"/> backed by F2X communications.
    /// </summary>
    internal sealed class AscribedGameExtensionLib : InnerLibBase, IAscribedGameExtensionLib
    {
        #region Private Fields

        private readonly IAscribedGameExtensionLink ascribedGameExtensionLink;

        private AscribedGameContext pendingContext;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="AscribedGameExtensionLib"/>.
        /// </summary>
        internal AscribedGameExtensionLib(IAscribedGameExtensionLink innerLink,
                                          IEventDispatcher transactionalEventDispatcher,
                                          IEventDispatcher nonTransactionalEventDispatcher)
            : base(innerLink,
                   transactionalEventDispatcher,
                   nonTransactionalEventDispatcher)
        {
            AscribedGameContext = new AscribedGameContext();

            ascribedGameExtensionLink = innerLink;

            // Create event table.
            CreateEventTable();
        }

        #endregion

        #region IAscribedGameExtensionLib Implementation

        /// <inheritdoc />
        public event EventHandler<ActivateInnerContextEventArgs<IAscribedGameContext>> ActivateContextEvent;

        /// <inheritdoc />
        public event EventHandler<InactivateInnerContextEventArgs<IAscribedGameContext>> InactivateContextEvent;

        /// <inheritdoc />
        public event EventHandler<SwitchInnerContextEventArgs<IAscribedGameContext>> SwitchContextEvent;

        /// <inheritdoc />
        public IAscribedGameContext AscribedGameContext { get; private set; }

        #endregion

        #region Private Methods

        #region Implementing Event Table

        /// <summary>
        /// Creates the event lookup table.
        /// </summary>
        private void CreateEventTable()
        {
            // Events should be listed in alphabetical order!

            EventTable[typeof(ActivateInnerContextEventArgs<IAscribedGameContext>)] =
                (s, e) => HandleBeforeExecute(s, e, PreHandleActivateContext, ActivateContextEvent);

            EventTable[typeof(InactivateInnerContextEventArgs<IAscribedGameContext>)] =
                (s, e) => HandleBeforeExecute(s, e, PreHandleInactivateContext, InactivateContextEvent);

            EventTable[typeof(NewInnerContextEventArgs<IAscribedGameContext>)] =
                (s, e) => HandleBeforeExecute(s, e, PreHandleNewContext, null);

            EventTable[typeof(SwitchInnerContextEventArgs<IAscribedGameContext>)] =
                (s, e) => HandleBeforeExecute(s, e, PreHandleSwitchContext, SwitchContextEvent);
        }

        #endregion

        #region Event Handlers

        private NewInnerContextEventArgs<IAscribedGameContext> PreHandleNewContext(EventArgs eventArgs)
        {
            if(eventArgs is NewInnerContextEventArgs<IAscribedGameContext> newContextEventArgs)
            {
                var ascribedGameEntity = ascribedGameExtensionLink.AscribedGameEntity;
                var ascribedGameImportedExtensions = ascribedGameExtensionLink.AscribedGameImportedExtensions;

                switch(ascribedGameEntity.AscribedGameType)
                {
                    case AscribedGameType.Theme:
                    {
                        pendingContext = new AscribedGameContext(newContextEventArgs.Context.AscribedGameMode,
                                                                 newContextEventArgs.Context.PaytableDenominationInfo,
                                                                 ascribedGameEntity,
                                                                 ascribedGameImportedExtensions);
                        break;
                    }
                    case AscribedGameType.Shell:
                    {
                        pendingContext = new AscribedGameContext(newContextEventArgs.Context.AscribedGameMode,
                                                                 null,
                                                                 ascribedGameEntity,
                                                                 ascribedGameImportedExtensions);
                        break;
                    }
                    default:
                    {
                        throw new NotSupportedException($"New Context Event for AscribedGameType {ascribedGameEntity.AscribedGameType} is not supported.");
                    }
                }
            }

            return null;
        }

        private ActivateInnerContextEventArgs<IAscribedGameContext> PreHandleActivateContext(EventArgs eventArgs)
        {
            AscribedGameContext = pendingContext;

            IsContextActive = true;

            return new ActivateInnerContextEventArgs<IAscribedGameContext>(AscribedGameContext);
        }

        private InactivateInnerContextEventArgs<IAscribedGameContext> PreHandleInactivateContext(EventArgs eventArgs)
        {
            pendingContext = (AscribedGameContext)AscribedGameContext;
            AscribedGameContext = new AscribedGameContext();

            IsContextActive = false;

            return new InactivateInnerContextEventArgs<IAscribedGameContext>(pendingContext);
        }

        private SwitchInnerContextEventArgs<IAscribedGameContext> PreHandleSwitchContext(EventArgs eventArgs)
        {
            if(eventArgs is SwitchInnerContextEventArgs<IAscribedGameContext> switchContextEventArgs)
            {
                var oldContext = (AscribedGameContext)AscribedGameContext;
                switch(oldContext.AscribedGameEntity.AscribedGameType)
                {
                    case AscribedGameType.Theme:
                    {
                        AscribedGameContext = new AscribedGameContext(oldContext.AscribedGameMode,
                                                                      switchContextEventArgs.NewContext.PaytableDenominationInfo,
                                                                      switchContextEventArgs.NewContext.AscribedGameEntity,
                                                                      ascribedGameExtensionLink.AscribedGameImportedExtensions);
                        break;
                    }
                    default:
                    {
                        throw new NotSupportedException($"Switch Context Event for AscribedGameType {oldContext.AscribedGameEntity.AscribedGameType} is not supported.");
                    }
                }
            }

            return new SwitchInnerContextEventArgs<IAscribedGameContext>(AscribedGameContext);
        }

        #endregion

        #endregion
    }
}