//-----------------------------------------------------------------------
// <copyright file = "IPortal.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface for the portal category.
    /// </summary>
    public interface IPortal
    {
        /// <summary>
        /// Event that represents a portal action that has occurred.
        /// </summary>
        // ReSharper disable once EventNeverSubscribedTo.Global
        event EventHandler<PortalActionNotifyEventArgs> PortalActionNotifyEvent;

        /// <summary>
        /// Configure a visibility group.
        /// </summary>
        /// <param name="portalClass">The portal class where the visibility group will reside.</param>
        /// <param name="visibilityGroup">The name of the visibility group.</param>
        /// <param name="portalIds">The list of portal Ids to be in the visibility group.</param>
        /// <exception cref="ConfigureVisibilityGroupErrorException">
        /// Thrown if visibility group is unable to be configured.
        /// </exception>
        void ConfigureVisibilityGroup(string portalClass, string visibilityGroup, List<PortalId> portalIds);

        /// <summary>
        /// Create a portal.
        /// </summary>
        /// <param name="portalInformation">The portal information used to create a portal.</param>
        /// <param name="portalClass">The portal class where the new portal will reside.</param>
        /// <param name="defaultEmdiAccessToken">The default EMDI access token.</param>
        /// <returns>The portal Id of the newly created portal.</returns>
        /// <exception cref="CreatePortalErrorException">
        /// Thrown if a portal is unable to be created.
        /// </exception>
        PortalId CreatePortal(PortalInformation portalInformation, string portalClass, out long defaultEmdiAccessToken);

        /// <summary>
        /// Destroy a portal.
        /// </summary>
        /// <param name="portalId">The portal Id to destroy.</param>
        /// <exception cref="DestroyPortalErrorException">
        /// Thrown if a portal is unable to be destroyed.
        /// </exception>
        void DestroyPortal(PortalId portalId);

        /// <summary>
        /// Execute content on the portal.
        /// </summary>
        /// <param name="portalId">The portal Id of the portal to execute content.</param>
        /// <exception cref="ExecuteContentErrorException">
        /// Thrown if content is unable to be executed on a portal.
        /// </exception>
        void ExecuteContent(PortalId portalId);

        /// <summary>
        /// Hide a portal.
        /// </summary>
        /// <param name="portalId">The portal Id of the portal to hide.</param>
        /// <exception cref="HidePortalErrorException">
        /// Thrown if a portal is unable to be hidden.
        /// </exception>
        void HidePortal(PortalId portalId);

        /// <summary>
        /// Show a portal.
        /// </summary>
        /// <param name="portalId">The portal id of the portal to show.</param>
        /// <exception cref="ShowPortalErrorException">
        /// Thrown if a portal is unable to be shown.
        /// </exception>
        void ShowPortal(PortalId portalId);

        /// <summary>
        /// Releases all content from a portal.
        /// </summary>
        /// <param name="portalId">The portal Id of the portal to release content from.</param>
        /// <param name="requestorName">The name of the entity requesting to release content.</param>
        /// <exception cref="ReleaseContentErrorException">
        /// Thrown if a content on a portal is unable to be released.
        /// </exception>
        void ReleaseContent(PortalId portalId, string requestorName);

        /// <summary>
        /// Load content on a portal. 
        /// </summary>
        /// <param name="portalId">The portal Id of the portal to load content.</param>
        /// <param name="contentUrl">The url of the content to load.</param>
        /// <param name="requestorName">The name of the requestor requesting content.</param>
        /// <param name="startAfterLoad">Whether to start after load or not.</param>
        /// <param name="emdiAccessToken">The EMDI access token.</param>
        /// <param name="authorizeDefaultEmdiCapabilities">
        /// Whether or not to authorize the default EMDI capabilities.
        /// </param>
        /// <param name="portalContentToken">
        /// Portals with the same, non-zero portal content token can participate
        /// in content-to-content messaging. A value of 0 for this parameter (or omitting it) 
        /// prohibits content-to-content messaging.
        /// </param>
        /// <param name="portalContentId">
        /// The Id of the content currently active in the portal. This value is only relevant
        /// to the Foundation if the <paramref name="portalContentToken"/> parameter is non-zero.
        /// </param>
        /// <exception cref="LoadContentErrorException">
        /// Thrown if content on a portal is unable to be loaded.
        /// </exception>
        void LoadContent(PortalId portalId, string contentUrl, string requestorName,
            bool startAfterLoad, long emdiAccessToken, bool authorizeDefaultEmdiCapabilities,
            long portalContentToken = 0, long portalContentId = 0);

        /// <summary>
        /// Returns content for a specific portal.
        /// </summary>
        /// <param name="portalId">The portal Id of the portal to get information about.</param>
        /// <returns>
        /// A <see cref="PortalInformation"/> object that contains the associated portal information.
        /// </returns>
        /// <exception cref="PortalInfoErrorException">
        /// Thrown if portal info is unable to be obtained.
        /// </exception>
        PortalInformation GetPortalInfo(PortalId portalId);

        /// <summary>
        /// Returns a list of portal Ids for a given portal class.
        /// </summary>
        /// <param name="portalClass">The name of the portal class.</param>
        /// <returns>A string of all portals Ids in the portal class.</returns>
        /// <exception cref="PortalListErrorException">
        /// Thrown if the list of portals is unable to be obtained.
        /// </exception>
        List<PortalId> GetPortalList(string portalClass);

        /// <summary>
        /// Gets the content state of a portal.
        /// </summary>
        /// <param name="portalId">The portal Id of the portal to get the content state from.</param>
        /// <returns>An enumerated value describing the state of the portal content.</returns>
        /// <exception cref="GetContentStateErrorException">
        /// Thrown if the portal content state was unable to be obtained.
        /// </exception>
        PortalContentStateOptions GetContentState(PortalId portalId);

        /// <summary>
        /// Gets the visibility state of a portal.
        /// </summary>
        /// <param name="portalId">The portal Id of the portal to get the visibility state from.</param>
        /// <returns>An enumerated value describing the visibility state of the portal.</returns>
        /// <exception cref="GetVisibilityStateErrorException">
        /// Thrown if the portal visibility state is unable to be obtained.
        /// </exception>
        PortalVisibilityStateOptions GetVisibilityState(PortalId portalId);

        /// <summary>
        /// Gets the current EMDI xml socket interface and EMDI websocket interface port numbers.
        /// </summary>
        /// <returns>
        /// An object of type <see cref="EmdiConfigInformation"/> containing the requested information.
        /// </returns>
        /// <exception cref="EmdiConfigInformationErrorException">
        /// Thrown if the EMDI port information is unable to be obtained.
        /// </exception>
        EmdiConfigInformation GetEmdiConfigInformation();

        /// <summary>
        /// Retrieves the portal id of the portal with the specified name.
        /// </summary>
        /// <param name="portalClass">The portal class that the specified portal resides in.</param>
        /// <param name="portalName">The name of the portal to look up.</param>
        /// <returns>The portal id of the requested portal.</returns>
        /// <exception cref="GetPortalIdByNameErrorException">
        /// Thrown if portal id is unable to be obtained.
        /// </exception>
        PortalId GetPortalIdByName(string portalClass, string portalName);
    }
}
