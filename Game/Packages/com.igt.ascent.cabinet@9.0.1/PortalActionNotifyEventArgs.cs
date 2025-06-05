//-----------------------------------------------------------------------
// <copyright file = "PortalActionNotifyEventArgs.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Collections.Generic;
    using CSI.Schemas.Internal;

    /// <summary>
    /// Enumeration containing portal action statuses.
    /// </summary>
    public enum PortalActionStatus
    {
        /// <summary>
        /// Status indicating the portal action was invalid.
        /// </summary>
        InvalidPortalAction,

        /// <summary>
        /// Status indicating the portal was created.
        /// </summary>
        PortalCreated,

        /// <summary>
        /// Status indicating the portal was destroyed.
        /// </summary>
        PortalDestroyed,

        /// <summary>
        /// Status indicating the content was loaded on the portal.
        /// </summary>
        LoadContent,

        /// <summary>
        /// Status indicating the content was executed on the portal.
        /// </summary>
        ExecuteContent,

        /// <summary>
        /// Status indicating the content was released on the portal.
        /// </summary>
        ReleaseContent,

        /// <summary>
        /// Status indicating there was a content error on the portal.
        /// </summary>
        ContentError,

        /// <summary>
        /// Status indicating the portal was shown.
        /// </summary>
        ShowPortal,

        /// <summary>
        /// Status indicating the portal was hidden.
        /// </summary>
        HidePortal,

        /// <summary>
        /// Status indicating the EMDI socket has been opened.
        /// </summary>
        EmdiSocketOpened,

        /// <summary>
        /// Status indicating the EMDI socket has been closed.
        /// </summary>
        EmdiSocketClosed,

        /// <summary>
        /// Status indicating there was an EMDI socket error.
        /// </summary>
        EmdiSocketError
    }

    /// <summary>
    /// Enumeration containing portal action result statuses.
    /// </summary>
    public enum PortalActionResultStatus
    {
        /// <summary>
        /// Status indicating the portal action result was invalid.
        /// </summary>
        InvalidPortalActionResult,

        /// <summary>
        /// Status indicating the portal action result was successful.
        /// </summary>
        Success,

        /// <summary>
        /// Status indicating the portal action result failed.
        /// </summary>
        Failed
    }

    /// <summary>
    /// 
    /// </summary>
    public class PortalActionNotifyEventArgs : EventArgs
    {
        /// <summary>
        /// Dictionary for converting from CSI portal action enumeration to public (SDK) portal action enumeration.
        /// </summary>
        private readonly Dictionary<PortalAction, PortalActionStatus> toPublicPortalActionDictionary
            = new Dictionary<PortalAction, PortalActionStatus>
            {
                {PortalAction.INVALID_PORTAL_ACTION, PortalActionStatus.InvalidPortalAction},
                {PortalAction.PORTAL_CREATED, PortalActionStatus.PortalCreated},
                {PortalAction.PORTAL_DESTROYED, PortalActionStatus.PortalDestroyed},
                {PortalAction.LOAD_CONTENT, PortalActionStatus.LoadContent},
                {PortalAction.EXECUTE_CONTENT, PortalActionStatus.ExecuteContent},
                {PortalAction.RELEASE_CONTENT, PortalActionStatus.ReleaseContent},
                {PortalAction.CONTENT_ERROR, PortalActionStatus.ContentError},
                {PortalAction.SHOW_PORTAL, PortalActionStatus.ShowPortal},
                {PortalAction.HIDE_PORTAL, PortalActionStatus.HidePortal},
                {PortalAction.EMDI_SOCKET_OPENED, PortalActionStatus.EmdiSocketOpened},
                {PortalAction.EMDI_SOCKET_CLOSED, PortalActionStatus.EmdiSocketClosed},
                {PortalAction.EMDI_SOCKET_ERROR, PortalActionStatus.EmdiSocketError}
            };

        /// <summary>
        /// Dictionary for converting from CSI portal action result enumeration to public (SDK) portal action result enumeration.
        /// </summary>
        private readonly Dictionary<PortalActionResult, PortalActionResultStatus> toPublicPortalActionResultDictionary
            = new Dictionary<PortalActionResult, PortalActionResultStatus>
            {
                {PortalActionResult.INVALID_PORTAL_ACTION_RESULT, PortalActionResultStatus.InvalidPortalActionResult},
                {PortalActionResult.SUCCESS, PortalActionResultStatus.Success},
                {PortalActionResult.FAILED, PortalActionResultStatus.Failed}
            };

        /// <summary>
        /// The portal Id associated with the event.
        /// </summary>
        public readonly PortalId PortalId;

        /// <summary>
        /// The <see cref="PortalActionStatus"/> associated with the event.
        /// </summary>
        public readonly PortalActionStatus PortalActionStatus;

        /// <summary>
        /// The <see cref="PortalActionResultStatus"/> associated with the event.
        /// </summary>
        public readonly PortalActionResultStatus PortalActionResultStatus;

        /// <summary>
        /// Constuct the event arguments with the given parameters.
        /// </summary>
        /// <param name="portalId">The portal Id.</param>
        /// <param name="portalAction">The portal action.</param>
        /// <param name="portalActionResult">The portal action result.</param>
        /// <exception cref="ArgumentNullException">Thrown if the input portal id is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the either input enumeration is invalid.</exception>
        public PortalActionNotifyEventArgs(PortalId portalId, PortalAction portalAction, 
            PortalActionResult portalActionResult)
        {
            PortalId = portalId ?? throw new ArgumentNullException(nameof(portalId));
            PortalActionStatus = toPublicPortalActionDictionary[portalAction];
            PortalActionResultStatus = toPublicPortalActionResultDictionary[portalActionResult];
        }
    }
}