//-----------------------------------------------------------------------
// <copyright file = "WindowManagementCategory.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using CSI.Schemas;
    using CSI.Schemas.Internal;
    using CsiTransport;
    using Foundation.Transport;

    /// <summary>
    /// Category which handles window management.
    /// </summary>
    internal class WindowManagementCategory : CategoryBase<CsiWindow>
    {
        #region Private Fields

        /// <summary>
        /// Dictionary containing request handlers for the window management category.
        /// </summary>
        private readonly Dictionary<Type, Action<object, ulong>> requestHandlers = new Dictionary<Type, Action<object, ulong>>();

        /// <summary>
        /// The data on resize requests that haven't been replied to yet.
        /// </summary>
        private readonly ConcurrentDictionary<ulong, WindowResizeEventArgs> pendingResizeRequestResponses
            = new ConcurrentDictionary<ulong, WindowResizeEventArgs>();

        /// <summary>
        /// The data on multiple windows resize requests that haven't been replied to yet.
        /// </summary>
        private readonly ConcurrentDictionary<ulong, MultiWindowResizeEventArgs> pendingMultiWindowResizeRequestResponses
            = new ConcurrentDictionary<ulong, MultiWindowResizeEventArgs>();

        /// <summary>
        /// The <see cref="FoundationTarget"/> that this <see cref="ResourceManagementCategory"/> was constructed with.
        /// </summary>
        private readonly FoundationTarget foundationTarget;

        #endregion Private Fields

        #region Private Properties

        /// <summary>
        /// Flag indicating if a foundation that supports multi-windows reposition.
        /// </summary>
        private bool MultiWindowRepositionSupported => VersionMajor == 1 && VersionMinor >= 9 || VersionMajor >= 2;

        #endregion Private Properties

        #region Events

        /// <summary>
        /// Event which is fired when the CSI manager requests we resize a window.
        /// </summary>
        public event EventHandler<WindowResizeEventArgs> WindowResizeEvent;

        /// <summary>
        /// Event which is fired when the CSI manager requests we resize multi-windows.
        /// </summary>
        public event EventHandler<MultiWindowResizeEventArgs> MultiWindowResizeEvent;

        /// <summary>
        /// Event which is fired when the CSI manager requests we change the window Z order.
        /// </summary>
        public event EventHandler<WindowZOrderEventArgs> ChangeZOrderEvent;

        #endregion Events

        #region Constructors

        /// <summary>
        /// Construct an instance of the category.
        /// </summary>
        /// <param name="target">The foundation target the client is running against.</param>
        public WindowManagementCategory(FoundationTarget target)
        {
            foundationTarget = target;

            requestHandlers[typeof(SizeRequest)] =
                (message, requestId) => HandleSizeRequest(message as SizeRequest, requestId);
            requestHandlers[typeof(MultiWindowSizeRequest)] =
                (message, requestId) => HandleMultiWindowSizeRequest(message as MultiWindowSizeRequest, requestId);
            requestHandlers[typeof(ChangeZOrderRequest)] =
                (message, requestId) => HandleChangeZOrderRequest(message as ChangeZOrderRequest, requestId);
        }

        #endregion Constructors

        /// <summary>
        /// Gets if a window resize request has been started or not.
        /// </summary>
        public bool WindowResizeRequestInProgress => pendingResizeRequestResponses.Count > 0;

        /// <summary>
        /// Gets if a multi-windows resize request has been started or not.
        /// </summary>
        public bool MultiWindowResizeRequestInProgress => pendingMultiWindowResizeRequestResponses.Count > 0;

        #region Overrides of CategoryBase<CsiWindow>

        /// <inheritdoc/>
        public override Category Category => Category.CsiWindow;

        /// <inheritdoc/>
        public override ushort VersionMajor => 1;

        /// <inheritdoc/>
        public override ushort VersionMinor
        {
            get
            {
                if(foundationTarget.IsEqualOrNewer(FoundationTarget.AscentN01Series))
                {
                    return 9;
                }
                if(foundationTarget.IsEqualOrNewer(FoundationTarget.AscentKSeriesCds))
                {
                    return 8;
                }
                return 7;
            }
        }

        /// <inheritdoc/>
        public override void HandleEvent(object message)
        {
            var windowManagementMessage = message as CsiWindow;

            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if(windowManagementMessage == null)
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiWindow), message.GetType()));
            }
            if(windowManagementMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Window management message contained no event, request or response.");
            }

            throw new UnhandledEventException("Event not handled: " + windowManagementMessage.Item.GetType());
        }

        /// <inheritdoc/>
        public override void HandleRequest(object message, ulong requestId)
        {
            var windowManagementMessage = message as CsiWindow;

            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if(windowManagementMessage == null)
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiWindow), message.GetType()));
            }
            if(windowManagementMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Window management message contained no event, request or response.");
            }

            var requestType = windowManagementMessage.Item.GetType();
            if(requestHandlers.ContainsKey(requestType))
            {
                requestHandlers[requestType](windowManagementMessage.Item, requestId);
            }
            else
            {
                throw new UnhandledRequestException("Request not handled: " + windowManagementMessage.Item.GetType());
            }
        }

        #endregion Overrides of CategoryBase<CsiWindow>

        #region Private Methods

        /// <summary>
        /// Wait for a specific response.
        /// </summary>
        /// <typeparam name="TResponse">The type of response to wait for.</typeparam>
        /// <returns>The requested response.</returns>
        /// <exception cref="UnexpectedReplyTypeException">
        /// Thrown when the response is not the type expected.
        /// </exception>
        private TResponse GetWindowResponse<TResponse>() where TResponse : class
        {
            var response = GetResponse();
            if(!(response.Item is TResponse innerResponse))
            {
                throw new UnexpectedReplyTypeException("Unexpected response.", typeof(TResponse), response.GetType());
            }

            return innerResponse;
        }

        /// <summary>
        /// Check the response to see if there are any errors.
        /// </summary>
        /// <param name="response">The response to check.</param>
        /// <exception cref="WindowManagementCategoryException">Thrown if the response indicates that there was an error.</exception>
        /// <remarks>Need to ask for the CSI to be updated to have different names for the responses.</remarks>
        private static void CheckResponse(WindowResponse response)
        {
            if(response.ErrorCode != WindowErrorCode.NONE)
            {
                throw new WindowManagementCategoryException(response.ErrorCode.ToString(), response.ErrorDescription);
            }
        }

        #endregion Private Methods

        #region Window Management Methods

        /// <summary>
        /// Inform the CSI Manager of a game window.
        /// </summary>
        /// <param name="canHandleMld">
        /// Flag indicating if the client understands MLD.
        /// </param>
        /// <param name="priority">
        /// The client priority.
        /// </param>
        /// <param name="windowHandles">
        /// List of window handles.
        /// </param>
        /// <param name="multiTouchNativelySupported">
        /// The flag indicating whether the client supports the native multi-touch implementation on all display devices,
        /// or if Foundation needs to interpret touches and generate Windows mouse events.
        /// </param>
        /// <returns>The window ID.</returns>
        /// <exception cref="ArgumentNullException">Thrown when windowHandles is null.</exception>
        public ulong CreateWindow(bool canHandleMld, Priority priority, List<long> windowHandles, bool multiTouchNativelySupported)
        {
            if(windowHandles == null)
            {
                throw new ArgumentNullException(nameof(windowHandles));
            }

            var request = new CsiWindow
            {
                Item = new CreatedRequest
                {
                    Flags = canHandleMld,
                    PriorityType = priority,
                    WindowHandle = windowHandles,
                    MultitouchSupportedSpecified = true,
                    MultitouchSupported = multiTouchNativelySupported,
                    SimulateMouseEventsOnDppSpecified = (VersionMajor > 1) || (VersionMinor >= 6),
                    SimulateMouseEventsOnDpp = !multiTouchNativelySupported
                }
            };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category.CsiWindow, request));
            var response = GetWindowResponse<CreatedResponse>();
            CheckResponse(response.WindowResponse);

            return response.WindowId;
        }

        /// <summary>
        /// Inform the CSI Manager that a window has been destroyed.
        /// </summary>
        /// <param name="windowId">The window ID of the destroyed window.</param>
        /// <returns>The ID of the destroyed window.</returns>
        public ulong DestroyWindowRequest(ulong windowId)
        {
            var request = new CsiWindow
            {
                Item = new DestroyedRequest
                {
                    WindowId = windowId
                }
            };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category.CsiWindow, request));
            var response = GetWindowResponse<DestroyedResponse>();
            CheckResponse(response.WindowResponse);

            return response.WindowId;
        }

        /// <summary>
        /// Request that the window be repositioned.
        /// </summary>
        /// <param name="window">Information about the window and its desired position.</param>
        public ulong RepositionWindowRequest(Window window)
        {
            var request = new CsiWindow
            {
                Item = new RepositionRequest
                {
                    Window = window
                }
            };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category.CsiWindow, request));
            var response = GetWindowResponse<RepositionResponse>();
            CheckResponse(response.WindowResponse);

            return response.WindowId;
        }

        /// <summary>
        /// Request to change the positions of multiple windows.
        /// If the given Foundation Target does not support multi window requests then send singular
        /// reposition requests for each window.
        /// </summary>
        /// <param name="windows">Information about the positions of multiple windows.</param>
        /// <returns>List of Ids for multiple windows.</returns>
        public IList<ulong> RepositionMultiWindowRequest(IList<Window> windows)
        {
            if(!MultiWindowRepositionSupported)
            {
                var responses = new List<ulong>();

                foreach(var window in windows)
                {
                    responses.Add(RepositionWindowRequest(window));
                }

                return responses;
            }

            var request = new CsiWindow
            {
                Item = new MultiWindowRepositionRequest
                {
                    Windows = windows.ToList()
                }
            };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category.CsiWindow, request));
            var response = GetWindowResponse<MultiWindowRepositionResponse>();
            foreach(var windowResponse in response.WindowResponse)
            {
                CheckResponse(windowResponse);
            }

            return response.WindowId;
        }

        #endregion Window Management Methods

        #region Window Management Requests

        /// <summary>
        /// Sends the size request response back to the server.
        /// </summary>
        /// <param name="requestId">
        /// The ID of the request that the complete message is for.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this method is called while no window resize request has been started.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="requestId"/> doesn't match the ID of any pending request messages that need to be replied to.
        /// </exception>
        public void SizeRequestComplete(ulong requestId)
        {
            if(!WindowResizeRequestInProgress)
            {
                throw new InvalidOperationException("The complete message cannot be sent while no window resize request is in progress.");
            }

            if(!pendingResizeRequestResponses.TryGetValue(requestId, out var windowResizeRequestEventArgs))
            {
                throw new ArgumentOutOfRangeException(nameof(requestId),
                    $"The request ID {requestId} was not found in the list of pending window resize requests.");
            }
            pendingResizeRequestResponses.TryRemove(requestId, out _);

            var response = new CsiWindow
            {
                Item = new SizeResponse
                {
                    WindowId = windowResizeRequestEventArgs.RequestedWindow.WindowId,
                    WindowIdSpecified = true
                }
            };

            Transport.SendResponse(MakeCsiMessageFromResponse(MessageSerializer, Category.CsiWindow, response), requestId);
        }

        /// <summary>
        /// Sends the multi-windows size request response back to the server.
        /// </summary>
        /// <param name="requestId">
        /// The ID of the request that the complete message is for.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this method is called while no multi-windows resize request has been started.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="requestId"/> doesn't match the ID of any pending request messages that need to be replied to.
        /// </exception>
        public void MultiWindowSizeRequestComplete(ulong requestId)
        {
            if(!MultiWindowResizeRequestInProgress)
            {
                throw new InvalidOperationException(
                    "The complete message cannot be sent while no multi-windows resize request is in progress.");
            }

            if(!pendingMultiWindowResizeRequestResponses.TryGetValue(requestId,
                    out var multiWindowResizeRequestEventArgs))
                {
                    throw new ArgumentOutOfRangeException(nameof(requestId),
                        $"The request ID {requestId} was not found in the list of pending multi-windows resize requests.");
                }

                pendingMultiWindowResizeRequestResponses.TryRemove(requestId, out _);

            var response = new CsiWindow
            {
                Item = new MultiWindowSizeResponse
                {
                    WindowId = multiWindowResizeRequestEventArgs.RequestedWindows
                        .Select(window => window.WindowId).ToList()
                }
            };

            Transport.SendResponse(MakeCsiMessageFromResponse(MessageSerializer, Category.CsiWindow, response),
                requestId);
        }

        /// <summary>
        /// Handle a request to resize the window.
        /// </summary>
        /// <param name="sizeRequest">The request to resize the window.</param>
        /// <param name="requestId">The ID of the request.</param>
        private void HandleSizeRequest(SizeRequest sizeRequest, ulong requestId)
        {
            var windowResizeRequestEventArgs = new WindowResizeEventArgs(sizeRequest.Window, requestId);
            pendingResizeRequestResponses.TryAdd(requestId, windowResizeRequestEventArgs);
            WindowResizeEvent?.Invoke(this, windowResizeRequestEventArgs);

            // This request does have a response that needs to be sent. However the response
            // should not be sent until the resize has actually occurred. This is how the
            // CSI server knows the window is ready for display.
        }

        /// <summary>
        /// Handle a request to resize multiple windows.
        /// </summary>
        /// <param name="sizeRequest">The request to resize multiple windows.</param>
        /// <param name="requestId">The ID of the request.</param>
        private void HandleMultiWindowSizeRequest(MultiWindowSizeRequest sizeRequest, ulong requestId)
        {
            var windowResizeRequestEventArgs = new MultiWindowResizeEventArgs(sizeRequest.Windows, requestId);
            pendingMultiWindowResizeRequestResponses.TryAdd(requestId, windowResizeRequestEventArgs);

            MultiWindowResizeEvent?.Invoke(this, windowResizeRequestEventArgs);

            // This request does have a response that needs to be sent. However the response
            // should not be sent until the resize has actually occurred. This is how the
            // CSI server knows the window is ready for display.
        }

        /// <summary>
        /// Handle a request to change the Z order.
        /// </summary>
        /// <param name="changeZOrderRequest">The request to change the Z order.</param>
        /// <param name="requestId">The ID of the request.</param>
        private void HandleChangeZOrderRequest(ChangeZOrderRequest changeZOrderRequest, ulong requestId)
        {
            ChangeZOrderEvent?.Invoke(this, new WindowZOrderEventArgs(changeZOrderRequest));

            var response = new CsiWindow
            {
                Item = new ChangeZOrderResponse
                {
                    WindowId = changeZOrderRequest.WindowId,
                    WindowIdSpecified = true
                }
            };

            Transport.SendResponse(MakeCsiMessageFromResponse(MessageSerializer, Category.CsiWindow, response), requestId);
        }

        #endregion Window Management Requests

        /// <summary>
        /// Clear any stored pending requests.
        /// </summary>
        public void ClearPendingRequests()
        {
            pendingResizeRequestResponses.Clear();
            pendingMultiWindowResizeRequestResponses.Clear();
        }
    }
}
