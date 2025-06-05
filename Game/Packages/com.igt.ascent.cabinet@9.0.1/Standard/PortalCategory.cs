//-----------------------------------------------------------------------
// <copyright file = "PortalCategory.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using CSI.Schemas.Internal;
    using CsiTransport;
    using Foundation.Transport;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Class for handling the CSI portal category.
    /// </summary>
    internal class PortalCategory : CategoryBase<CsiPortal>, IPortal, ICabinetUpdate
    {
        #region Constructor

        /// <summary>
        /// Conditionally create an instance of the Portal category.
        /// </summary>
        /// <param name="currentFoundationTarget">The current foundation target.</param>
        /// <returns>An instance of the Portal category if J Series or greater, null otherwise.</returns>
        public static PortalCategory CreateInstance(FoundationTarget currentFoundationTarget)
        {
            return currentFoundationTarget.IsEqualOrNewer(FoundationTarget.AscentJSeriesMps) ? new PortalCategory(currentFoundationTarget) : null;
        }

        /// <summary>
        /// Instantiates a PortalCategory instance using the specified <see cref="FoundationTarget"/>.
        /// </summary>
        /// <param name="currentFoundationTarget">The foundation target used at time of object creation.</param>
        private PortalCategory(FoundationTarget currentFoundationTarget)
        {
            // Cache the Foundation target as it is needed to return the correct category version.
            foundationTarget = currentFoundationTarget;

            // Populate event handlers dictionary with all possible events in the CSIPortal category.
            eventHandlers[typeof(PortalActionNotifyEvent)] =
                message => HandlePortalActionNotifyEvent(message as PortalActionNotifyEvent);
        }

        #endregion Constructor

        #region Private Fields

        /// <summary>
        /// Stores the current foundation target and is used to return the correct versioning information.
        /// </summary>
        private readonly FoundationTarget foundationTarget;

        /// <summary>
        /// List of event handlers in the CSIPortal category.
        /// </summary>
        private readonly Dictionary<Type, Action<object>> eventHandlers = new Dictionary<Type, Action<object>>();

        /// <summary>
        /// The list of pending events in the CSIPortal category.
        /// </summary>
        private readonly List<CsiPortal> pendingEvents = new List<CsiPortal>();

        #endregion Private Fields

        #region CategoryBase<CsiPortal> Implementation

        /// <inheritdoc/>
        public override Category Category => Category.CsiPortal;

        /// <inheritdoc/>
        public override ushort VersionMajor => 1;

        /// <inheritdoc/>
        public override ushort VersionMinor => (ushort)(foundationTarget.IsEqualOrNewer(FoundationTarget.AscentKSeriesCds) ? 1 : 0);

        /// <inheritdoc/>
        public override void HandleEvent(object message)
        {
            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if(!(message is CsiPortal portalMessage))
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiPortal),
                                                                message.GetType()));
            }

            if(portalMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Portal message contained no event, request, or response.");
            }

            if(eventHandlers.ContainsKey(portalMessage.Item.GetType()))
            {
                lock(pendingEvents)
                {
                    pendingEvents.Add(portalMessage);
                }
            }
            else
            {
                throw new UnhandledEventException("Event not handled: " + portalMessage.Item.GetType());
            }
        }

        /// <inheritdoc/>
        public override void HandleRequest(object message, ulong requestId)
        {
            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if(!(message is CsiPortal portalMessage))
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiPortal),
                                                                message.GetType()));
            }

            if(portalMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Portal message contained no event, request or response.");
            }

            throw new UnhandledRequestException("Request not handled: " + portalMessage.Item.GetType());
        }

        #endregion CategoryBase<CsiPortal> Implementation

        #region IPortal Implementation

        /// <inheritdoc/>
        public event EventHandler<PortalActionNotifyEventArgs> PortalActionNotifyEvent;

        /// <inheritdoc/>
        public void ConfigureVisibilityGroup(string portalClass, string visibilityGroup, List<PortalId> portalIds)
        {
            var request = new CsiPortal
                          {
                              Item = new ConfigureVisibilityGroupRequest
                                     {
                                         PortalClass = portalClass,
                                         VisibilityGroupName = visibilityGroup,
                                         PortalIds = portalIds.Select(portalId => portalId.Id).ToList()
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetPortalResponse<ConfigureVisibilityGroupResponse>();
            CheckResponse(response.PortalResponse);
        }

        /// <inheritdoc/>
        public PortalId CreatePortal(PortalInformation portalInformation, string portalClass, out long defaultEmdiAccessToken)
        {
            var request = new CsiPortal
                          {
                              Item = new CreatePortalRequest
                                     {
                                         PortalConfig = portalInformation.ToCsi(),
                                         PortalClass = portalClass
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetPortalResponse<CreatePortalResponse>();
            CheckResponse(response.PortalResponse);

            defaultEmdiAccessToken = response.DefaultEMDIAccessToken;
            return new PortalId(response.PortalId);
        }

        /// <inheritdoc/>
        public void DestroyPortal(PortalId portalId)
        {
            var request = new CsiPortal
                          {
                              Item = new DestroyPortalRequest
                                     {
                                         PortalId = portalId.Id
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetPortalResponse<DestroyPortalResponse>();
            CheckResponse(response.PortalResponse);
        }

        /// <inheritdoc/>
        public void ExecuteContent(PortalId portalId)
        {
            var request = new CsiPortal
                          {
                              Item = new ExecuteContentRequest
                                     {
                                         PortalId = portalId.Id
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetPortalResponse<ExecuteContentResponse>();
            CheckResponse(response.PortalResponse);
        }

        /// <inheritdoc/>
        public PortalInformation GetPortalInfo(PortalId portalId)
        {
            var request = new CsiPortal
                          {
                              Item = new PortalInfoRequest
                                     {
                                         PortalId = portalId.Id
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetPortalResponse<PortalInfoResponse>();
            CheckResponse(response.PortalResponse);

            return response.PortalInfo.ToPublic();
        }

        /// <inheritdoc/>
        public List<PortalId> GetPortalList(string portalClass)
        {
            var request = new CsiPortal
                          {
                              Item = new PortalListRequest
                                     {
                                         PortalClass = portalClass
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetPortalResponse<PortalListResponse>();
            CheckResponse(response.PortalResponse);

            return response.PortalIds != null
                       ? response.PortalIds.PortalId.Select(portalId => new PortalId(portalId)).ToList()
                       : new List<PortalId>();
        }

        /// <inheritdoc/>
        public void HidePortal(PortalId portalId)
        {
            var request = new CsiPortal
                          {
                              Item = new HidePortalRequest
                                     {
                                         PortalId = portalId.Id
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetPortalResponse<HidePortalResponse>();
            CheckResponse(response.PortalResponse);
        }

        /// <inheritdoc/>
        public void LoadContent(PortalId portalId,
                                string contentUrl,
                                string requestorName,
                                bool startAfterLoad,
                                long emdiAccessToken,
                                bool authorizeDefaultEmdiCapabilities,
                                long portalContentToken = 0,
                                long portalContentId = 0)
        {
            var request = new CsiPortal
                          {
                              Item = new LoadContentRequest
                                     {
                                         PortalId = portalId.Id,
                                         ContentURL = contentUrl,
                                         RequestorName = requestorName,
                                         StartAfterLoad = startAfterLoad,
                                         EMDIAccessToken = emdiAccessToken,
                                         AuthorizeDefaultEMDICapabilities = authorizeDefaultEmdiCapabilities,
                                         PortalContentToken = portalContentToken,
                                         PortalContentId = portalContentId,
                                         PortalContentIdSpecified = (VersionMajor > 1) || (VersionMinor >= 1) // Supported in 1.1 and higher.
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetPortalResponse<LoadContentResponse>();
            CheckResponse(response.PortalResponse);
        }

        /// <inheritdoc/>
        public void ReleaseContent(PortalId portalId, string requestorName)
        {
            var request = new CsiPortal
                          {
                              Item = new ReleaseContentRequest
                                     {
                                         PortalId = portalId.Id,
                                         RequestorName = requestorName
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetPortalResponse<ReleaseContentResponse>();
            CheckResponse(response.PortalResponse);
        }

        /// <inheritdoc/>
        public void ShowPortal(PortalId portalId)
        {
            var request = new CsiPortal
                          {
                              Item = new ShowPortalRequest
                                     {
                                         PortalId = portalId.Id
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetPortalResponse<ShowPortalResponse>();
            CheckResponse(response.PortalResponse);
        }

        /// <inheritdoc/>
        public PortalContentStateOptions GetContentState(PortalId portalId)
        {
            var request = new CsiPortal
                          {
                              Item = new GetContentStateRequest
                                     {
                                         PortalId = portalId.Id
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetPortalResponse<GetContentStateResponse>();
            CheckResponse(response.PortalResponse);

            return PortalContent.ToPublic(response.ContentState);
        }

        /// <inheritdoc/>
        public PortalVisibilityStateOptions GetVisibilityState(PortalId portalId)
        {
            var request = new CsiPortal
                          {
                              Item = new GetVisibilityStateRequest
                                     {
                                         PortalId = portalId.Id
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetPortalResponse<GetVisibilityStateResponse>();
            CheckResponse(response.PortalResponse);

            return PortalVisibility.ToPublic(response.VisibilityState);
        }

        /// <inheritdoc/>
        public EmdiConfigInformation GetEmdiConfigInformation()
        {
            var request = new CsiPortal
                          {
                              Item = new GetEMDIConfigRequest()
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetPortalResponse<GetEMDIConfigResponse>();
            CheckResponse(response.PortalResponse);

            return new EmdiConfigInformation(response.XMLSocketEMDIPort, response.WebsocketEMDIPort);
        }

        /// <inheritdoc/>
        public PortalId GetPortalIdByName(string portalClass, string portalName)
        {
            var request = new CsiPortal
                          {
                              Item = new GetPortalIdByNameRequest
                                     {
                                         PortalClass = portalClass,
                                         PortalName = portalName
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetPortalResponse<GetPortalIdByNameResponse>();
            CheckResponse(response.PortalResponse);
            return new PortalId(response.PortalId);
        }

        #endregion IPortal Implementation

        #region ICabinetUpdate Implementation

        /// <inheritdoc />
        public void Update()
        {
            var tempEvents = new List<CsiPortal>();

            lock(pendingEvents)
            {
                tempEvents.AddRange(pendingEvents);
                pendingEvents.Clear();
            }

            foreach(var pendingEvent in tempEvents)
            {
                // Events should only be placed in the queue if they are handled.
                eventHandlers[pendingEvent.Item.GetType()](pendingEvent.Item);
            }
        }

        #endregion ICabinetUpdate Implementation

        #region PrivateMethods

        /// <summary>
        /// Handle content events from the portal category.
        /// </summary>
        /// <param name="portalActionNotifyEvent">The content event to handle.</param>
        private void HandlePortalActionNotifyEvent(PortalActionNotifyEvent portalActionNotifyEvent)
        {
            PortalActionNotifyEvent?.Invoke(this,
                                            new PortalActionNotifyEventArgs(new PortalId(portalActionNotifyEvent.PortalId),
                                                                            portalActionNotifyEvent.Action,
                                                                            portalActionNotifyEvent.Result));
        }

        /// <summary>
        /// Check the response to see if there are any errors.
        /// </summary>
        /// <param name="response">The response to check.</param>
        /// <exception cref="PortalCategoryException">
        /// Thrown if the response indicates that there was an error.
        /// </exception>
        private static void CheckResponse(PortalResponse response)
        {
            if(response.ErrorCode.Item.GetType() == typeof(ConfigureVisibilityGroupErrorCode))
            {
                var specificEnum = (ConfigureVisibilityGroupErrorCode)response.ErrorCode.Item;
                switch(specificEnum)
                {
                    case ConfigureVisibilityGroupErrorCode.NONE:
                        // No exception should be thrown.
                        break;

                    case ConfigureVisibilityGroupErrorCode.OTHER_ERROR:
                        throw new ConfigureVisibilityGroupErrorException(ConfigureVisibilityGroupError.OtherError,
                                                                         response.ErrorDescription);

                    case ConfigureVisibilityGroupErrorCode.INVALID_PORTAL_ID:
                        throw new ConfigureVisibilityGroupErrorException(ConfigureVisibilityGroupError.InvalidPortalId,
                                                                         response.ErrorDescription);

                    case ConfigureVisibilityGroupErrorCode.TOO_MANY_PORTALS:
                        throw new ConfigureVisibilityGroupErrorException(ConfigureVisibilityGroupError.TooManyPortals,
                                                                         response.ErrorDescription);

                    case ConfigureVisibilityGroupErrorCode.INVALID_GROUP_NAME:
                        throw new ConfigureVisibilityGroupErrorException(ConfigureVisibilityGroupError.InvalidGroupName,
                                                                         response.ErrorDescription);

                    case ConfigureVisibilityGroupErrorCode.PORTALS_NOT_ALL_SAME_CLASS:
                        throw new ConfigureVisibilityGroupErrorException(ConfigureVisibilityGroupError.PortalsNotAllSameClass,
                                                                         response.ErrorDescription);

                    case ConfigureVisibilityGroupErrorCode.CLIENT_DOES_NOT_OWN_RESOURCE:
                        // Ignore
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
            else if(response.ErrorCode.Item.GetType() == typeof(CreatePortalErrorCode))
            {
                var specificEnum = (CreatePortalErrorCode)response.ErrorCode.Item;
                switch(specificEnum)
                {
                    case CreatePortalErrorCode.NONE:
                        // No exception should be thrown.
                        break;

                    case CreatePortalErrorCode.OTHER_ERROR:
                        throw new CreatePortalErrorException(CreatePortalError.OtherError,
                                                             response.ErrorDescription);

                    case CreatePortalErrorCode.PORTAL_EXISTS:
                        throw new CreatePortalErrorException(CreatePortalError.PortalExists,
                                                             response.ErrorDescription);

                    case CreatePortalErrorCode.PORTAL_OUT_OF_BOUNDS:
                        throw new CreatePortalErrorException(CreatePortalError.PortalOutOfBounds,
                                                             response.ErrorDescription);

                    case CreatePortalErrorCode.INVALID_PRIORITY:
                        throw new CreatePortalErrorException(CreatePortalError.InvalidPriority,
                                                             response.ErrorDescription);

                    case CreatePortalErrorCode.INVALID_MONITOR:
                        throw new CreatePortalErrorException(CreatePortalError.InvalidMonitor,
                                                             response.ErrorDescription);

                    case CreatePortalErrorCode.INVALID_PORTAL_NAME:
                        throw new CreatePortalErrorException(CreatePortalError.InvalidPortalName,
                                                             response.ErrorDescription);

                    case CreatePortalErrorCode.INVALID_PORTAL_CLASS:
                        throw new CreatePortalErrorException(CreatePortalError.InvalidPortalClass,
                                                             response.ErrorDescription);

                    case CreatePortalErrorCode.INVALID_DEFAULT_EMDI_TOKEN:
                        throw new CreatePortalErrorException(CreatePortalError.InvalidDefaultEMDIToken,
                                                             response.ErrorDescription);

                    case CreatePortalErrorCode.CLIENT_DOES_NOT_OWN_RESOURCE:
                        throw new CreatePortalErrorException(CreatePortalError.ClientDoesNotOwnResource,
                            response.ErrorDescription);

                    default:
                        throw new NotSupportedException();
                }
            }
            else if(response.ErrorCode.Item.GetType() == typeof(DestroyPortalErrorCode))
            {
                var specificEnum = (DestroyPortalErrorCode)response.ErrorCode.Item;
                switch(specificEnum)
                {
                    case DestroyPortalErrorCode.NONE:
                        // No exception should be thrown.
                        break;

                    case DestroyPortalErrorCode.OTHER_ERROR:
                        throw new DestroyPortalErrorException(DestroyPortalError.OtherError,
                                                              response.ErrorDescription);

                    case DestroyPortalErrorCode.INVALID_PORTAL_ID:
                        throw new DestroyPortalErrorException(DestroyPortalError.InvalidPortalId,
                                                              response.ErrorDescription);

                    case DestroyPortalErrorCode.CANNOT_DESTROY_PORTAL:
                        throw new DestroyPortalErrorException(DestroyPortalError.CannotDestroyPortal,
                                                              response.ErrorDescription);

                    case DestroyPortalErrorCode.CLIENT_DOES_NOT_OWN_RESOURCE:
                        // Ignore
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
            else if(response.ErrorCode.Item.GetType() == typeof(ExecuteContentErrorCode))
            {
                var specificEnum = (ExecuteContentErrorCode)response.ErrorCode.Item;
                switch(specificEnum)
                {
                    case ExecuteContentErrorCode.NONE:
                        // No exception should be thrown.
                        break;

                    case ExecuteContentErrorCode.OTHER_ERROR:
                        throw new ExecuteContentErrorException(ExecuteContentError.OtherError,
                                                               response.ErrorDescription);

                    case ExecuteContentErrorCode.INVALID_PORTAL_ID:
                        throw new ExecuteContentErrorException(ExecuteContentError.InvalidPortalId,
                                                               response.ErrorDescription);

                    case ExecuteContentErrorCode.CONTENT_NOT_LOADED:
                        throw new ExecuteContentErrorException(ExecuteContentError.ContentNotLoaded,
                                                               response.ErrorDescription);

                    case ExecuteContentErrorCode.CLIENT_DOES_NOT_OWN_RESOURCE:
                        // Ignore
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
            else if(response.ErrorCode.Item.GetType() == typeof(GetContentStateErrorCode))
            {
                var specificEnum = (GetContentStateErrorCode)response.ErrorCode.Item;
                switch(specificEnum)
                {
                    case GetContentStateErrorCode.NONE:
                        // No exception should be thrown.
                        break;

                    case GetContentStateErrorCode.OTHER_ERROR:
                        throw new GetContentStateErrorException(GetContentStateError.OtherError,
                                                                response.ErrorDescription);

                    case GetContentStateErrorCode.INVALID_PORTAL_ID:
                        throw new GetContentStateErrorException(GetContentStateError.InvalidPortalId,
                                                                response.ErrorDescription);

                    default:
                        throw new NotSupportedException();
                }
            }
            else if(response.ErrorCode.Item.GetType() == typeof(GetEMDIConfigErrorCode))
            {
                var specificEnum = (GetEMDIConfigErrorCode)response.ErrorCode.Item;
                switch(specificEnum)
                {
                    case GetEMDIConfigErrorCode.NONE:
                        // No exception should be thrown.
                        break;

                    case GetEMDIConfigErrorCode.OTHER_ERROR:
                        throw new EmdiConfigInformationErrorException(EMDIConfigInformationError.OtherError,
                                                                      response.ErrorDescription);

                    default:
                        throw new NotSupportedException();
                }
            }
            else if(response.ErrorCode.Item.GetType() == typeof(GetPortalIdByNameErrorCode))
            {
                var specificEnum = (GetPortalIdByNameErrorCode)response.ErrorCode.Item;
                switch(specificEnum)
                {
                    case GetPortalIdByNameErrorCode.NONE:
                        // No exception should be thrown.
                        break;

                    case GetPortalIdByNameErrorCode.OTHER_ERROR:
                    case GetPortalIdByNameErrorCode.INVALID_PORTAL_NAME:
                    case GetPortalIdByNameErrorCode.INVALID_PORTAL_CLASS:
                        throw new PortalCategoryException(specificEnum.ToString(),
                                                          response.ErrorDescription);

                    default:
                        throw new NotSupportedException();
                }
            }
            else if(response.ErrorCode.Item.GetType() == typeof(GetVisibilityStateErrorCode))
            {
                var specificEnum = (GetVisibilityStateErrorCode)response.ErrorCode.Item;
                switch(specificEnum)
                {
                    case GetVisibilityStateErrorCode.NONE:
                        // No exception should be thrown.
                        break;

                    case GetVisibilityStateErrorCode.OTHER_ERROR:
                        throw new GetVisibilityStateErrorException(GetVisibilityStateError.OtherError,
                                                                   response.ErrorDescription);

                    case GetVisibilityStateErrorCode.INVALID_PORTAL_ID:
                        throw new GetVisibilityStateErrorException(GetVisibilityStateError.InvalidPortalId,
                                                                   response.ErrorDescription);

                    default:
                        throw new NotSupportedException();
                }
            }
            else if(response.ErrorCode.Item.GetType() == typeof(HidePortalErrorCode))
            {
                var specificEnum = (HidePortalErrorCode)response.ErrorCode.Item;
                switch(specificEnum)
                {
                    case HidePortalErrorCode.NONE:
                        // No exception should be thrown.
                        break;

                    case HidePortalErrorCode.OTHER_ERROR:
                        throw new HidePortalErrorException(HidePortalError.OtherError,
                                                           response.ErrorDescription);

                    case HidePortalErrorCode.INVALID_PORTAL_ID:
                        throw new HidePortalErrorException(HidePortalError.InvalidPortalId,
                                                           response.ErrorDescription);

                    case HidePortalErrorCode.CANNOT_HIDE_PORTAL:
                        throw new HidePortalErrorException(HidePortalError.CannotHidePortal,
                                                           response.ErrorDescription);

                    case HidePortalErrorCode.CLIENT_DOES_NOT_OWN_RESOURCE:
                        // Ignore
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
            else if(response.ErrorCode.Item.GetType() == typeof(LoadContentErrorCode))
            {
                var specificEnum = (LoadContentErrorCode)response.ErrorCode.Item;
                switch(specificEnum)
                {
                    case LoadContentErrorCode.NONE:
                        // No exception should be thrown.
                        break;

                    case LoadContentErrorCode.OTHER_ERROR:
                        throw new LoadContentErrorException(LoadContentError.OtherError,
                                                            response.ErrorDescription);

                    case LoadContentErrorCode.INVALID_PORTAL_ID:
                        throw new LoadContentErrorException(LoadContentError.InvalidPortalId,
                                                            response.ErrorDescription);

                    case LoadContentErrorCode.MALFORMED_URL:
                        throw new LoadContentErrorException(LoadContentError.MalformedUrl,
                                                            response.ErrorDescription);

                    case LoadContentErrorCode.CLIENT_DOES_NOT_OWN_RESOURCE:
                        // Ignore
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
            else if(response.ErrorCode.Item.GetType() == typeof(PortalInfoErrorCode))
            {
                var specificEnum = (PortalInfoErrorCode)response.ErrorCode.Item;
                switch(specificEnum)
                {
                    case PortalInfoErrorCode.NONE:
                        // No exception should be thrown.
                        break;

                    case PortalInfoErrorCode.OTHER_ERROR:
                        throw new PortalInfoErrorException(PortalInfoError.OtherError,
                                                           response.ErrorDescription);

                    case PortalInfoErrorCode.INVALID_PORTAL_ID:
                        throw new PortalInfoErrorException(PortalInfoError.InvalidPortalId,
                                                           response.ErrorDescription);

                    default:
                        throw new NotSupportedException();
                }
            }
            else if(response.ErrorCode.Item.GetType() == typeof(PortalListErrorCode))
            {
                //var specificEnum =(PortalListErrorCode) response.ErrorCode.Item;
                var specificEnum = (PortalListErrorCode)response.ErrorCode.Item;
                switch(specificEnum)
                {
                    case PortalListErrorCode.NONE:
                        // No exception should be thrown.
                        break;

                    case PortalListErrorCode.OTHER_ERROR:
                        throw new PortalListErrorException(PortalListError.OtherError,
                                                           response.ErrorDescription);

                    case PortalListErrorCode.INVALID_PORTAL_CLASS:
                        throw new PortalListErrorException(PortalListError.InvalidPortalClass,
                                                           response.ErrorDescription);

                    default:
                        throw new NotSupportedException();
                }
            }
            else if(response.ErrorCode.Item.GetType() == typeof(ReleaseContentErrorCode))
            {
                var specificEnum = (ReleaseContentErrorCode)response.ErrorCode.Item;
                switch(specificEnum)
                {
                    case ReleaseContentErrorCode.NONE:
                        // No exception should be thrown.
                        break;

                    case ReleaseContentErrorCode.OTHER_ERROR:
                        throw new ReleaseContentErrorException(ReleaseContentError.OtherError,
                                                               response.ErrorDescription);

                    case ReleaseContentErrorCode.INVALID_PORTAL_ID:
                        throw new ReleaseContentErrorException(ReleaseContentError.InvalidPortalId,
                                                               response.ErrorDescription);

                    case ReleaseContentErrorCode.CONTENT_NOT_LOADED:
                        throw new ReleaseContentErrorException(ReleaseContentError.ContentNotLoaded,
                                                               response.ErrorDescription);

                    case ReleaseContentErrorCode.CLIENT_DOES_NOT_OWN_RESOURCE:
                        // Ignore
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
            else if(response.ErrorCode.Item.GetType() == typeof(ShowPortalErrorCode))
            {
                var specificEnum = (ShowPortalErrorCode)response.ErrorCode.Item;
                switch(specificEnum)
                {
                    case ShowPortalErrorCode.NONE:
                        // No exception should be thrown.
                        break;

                    case ShowPortalErrorCode.OTHER_ERROR:
                        throw new ShowPortalErrorException(ShowPortalError.OtherError,
                                                           response.ErrorDescription);

                    case ShowPortalErrorCode.INVALID_PORTAL_ID:
                        throw new ShowPortalErrorException(ShowPortalError.InvalidPortalId,
                                                           response.ErrorDescription);

                    case ShowPortalErrorCode.CANNOT_SHOW_PORTAL:
                        throw new ShowPortalErrorException(ShowPortalError.CantShowPortal,
                                                           response.ErrorDescription);

                    case ShowPortalErrorCode.CLIENT_DOES_NOT_OWN_RESOURCE:
                        // Ignore
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        /// Wait for a specific response.
        /// </summary>
        /// <typeparam name="TResponse">The type of response to wait for.</typeparam>
        /// <returns>The requested response.</returns>
        /// <exception cref="UnexpectedReplyTypeException">
        /// Thrown when the response is not the type expected.
        /// </exception>
        private TResponse GetPortalResponse<TResponse>() where TResponse : class
        {
            var response = GetResponse();

            if(!(response.Item is TResponse innerResponse))
            {
                throw new UnexpectedReplyTypeException("Unexpected response.", typeof(TResponse), response.GetType());
            }

            return innerResponse;
        }

        #endregion PrivateMethods
    }
}