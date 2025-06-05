using System;
using System.Collections.Generic;
using System.Linq;
using IGT.Game.Core.Communication.Cabinet;
using IGT.Game.Core.Communication.Cabinet.CSI.Schemas;
using IGT.Game.Core.Presentation;
using IGT.Game.SDKAssets.AscentBuildSettings;
using IgtUnityEngine;
using Midas.Core.Configuration;
using UnityEngine;
using static Midas.Ascent.Cabinet.AscentCabinet;
using UnityMonitorRole = IgtUnityEngine.MonitorRole;

namespace Midas.Ascent.Cabinet
{
	/// <summary>
	/// Class for controlling the windows and monitors that are configured through the CSI or the
	/// Universal Controller (UC).
	/// </summary>
	internal sealed partial class WindowController
	{
		#region Private

		// The refresh rate used for Unity Displays.
		private const int DisplayRefreshRate = 60;

		/// <summary>
		/// The private constructor.
		/// </summary>
		private WindowController() { }

		#region Nested Class

		/// <summary>
		/// Stores display settings for a CSI Window, including the associated Unity display index and IGT monitor.
		/// </summary>
		private sealed class WindowAssociation
		{
			#region Public

			/// <summary>
			/// Parameterized Constructor.
			/// </summary>
			/// <param name="windowId">The window Id assigned by CSI.</param>
			/// <param name="monitor">The monitor object.</param>
			/// <param name="displayIndex">The Unity display index.</param>
			public WindowAssociation(ulong windowId, Monitor monitor, int displayIndex)
			{
				WindowId = windowId;
				Monitor = monitor;
				DisplayIndex = displayIndex;
			}

			/// <summary>
			/// Gets or sets the CSI window ID.
			/// </summary>
			public ulong WindowId { get; set; }

			/// <summary>
			/// Gets the reference to the CSI monitor info <see cref="Monitor" />.
			/// </summary>
			public Monitor Monitor { get; }

			/// <summary>
			/// Gets the Unity display index.
			/// </summary>
			public int DisplayIndex { get; }

			#endregion
		}

		#endregion

		/// <summary>
		/// Priority of the cabinet.
		/// </summary>
		private Priority priority;

		/// <summary>
		/// The private instance of the singleton.
		/// </summary>
		private static WindowController instance;

		/// <summary>
		/// Gets the flag indicating if <see cref="AsyncConnect" /> is complete.
		/// </summary>
		private bool asyncConnectComplete;

		/// <summary>
		/// Maps a window ID to a display index.
		/// </summary>
		private readonly List<WindowAssociation> windowAssociations = new List<WindowAssociation>();

		/// <summary>
		/// The game requested windows status flag.
		/// True indicating game requested windows to show.
		/// False indicating game requested windows to hide.
		/// Null indicating there's no request.
		/// </summary>
		private bool? gameRequestedWindowStatus;

		/// <summary>
		/// The player speed adjuster that controls the player frame rate.
		/// </summary>
		private PlayerSpeedAdjuster playerSpeedAdjuster;

		#endregion

		#region Events and Properties

		/// <summary>
		/// Event when the CSI reports Game window is resized.
		/// </summary>
		public event EventHandler<WindowResizeEventArgs> CabinetWindowSizeChangedEvent;

		/// <summary>
		/// Gets the singleton instance.
		/// </summary>
		public static WindowController Instance => instance ??= new WindowController();

		/// <summary>
		/// Gets the flag indicating if the CSI had completed its initial sizing of the windows.
		/// </summary>
		public bool WindowsSized { get; private set; }

		#endregion

		#region Methods

		/// <summary>
		/// Initializes the window controller.
		/// </summary>
		/// <param name="clientPriority">The window priority.</param>
		public void Initialize(Priority clientPriority = Priority.Game)
		{
			priority = clientPriority;
			playerSpeedAdjuster = new PlayerSpeedAdjuster();
		}

		/// <summary>
		/// Initializes on a seperate thread to speed up performance.
		/// </summary>
		/// <remarks>
		/// This method follows the pattern defined by <see cref="IAsyncConnect.AsyncConnect" />
		/// </remarks>
		public void AsyncConnect()
		{
			CabinetLib.WindowResizeEvent -= WindowResizeHandler;
			CabinetLib.WindowResizeEvent += WindowResizeHandler;
			CabinetLib.MultiWindowResizeEvent -= MultiWindowResizeHandler;
			CabinetLib.MultiWindowResizeEvent += MultiWindowResizeHandler;
			CabinetLib.WindowZOrderEvent -= WindowZOrderHandler;
			CabinetLib.WindowZOrderEvent += WindowZOrderHandler;

			asyncConnectComplete = true;
		}

		/// <summary>
		/// Initializes on the main thread.
		/// This will be called after <see cref="AsyncConnect" />.
		/// </summary>
		/// <remarks>
		/// This method follows the pattern defined by <see cref="IAsyncConnect.PostConnect" />
		/// </remarks>
		/// <exception cref="AsyncConnectException">Thrown if <see cref="PostConnect" /> failed.</exception>
		public void PostConnect()
		{
			if (!asyncConnectComplete)
			{
				throw new AsyncConnectException("Post Connect cannot be called before Async Connect completes.");
			}

			// Force enabling all monitors needs to be done before attempting to modify any other window configurations.
			// This configuration flag is not applicable to the editor.
			if (!Application.isEditor
				&& AscentFoundation.GameParameters.ForceEnableAllMonitors)
			{
				foreach (UnityMonitorRole monitorRole in Enum.GetValues(typeof(UnityMonitorRole)))
				{
					Log.Instance.Info($"IgtScreen.SetMonitorEnabled role={monitorRole} displayIndex={(int)monitorRole}");
					IgtScreen.SetMonitorEnabled(monitorRole, true);
				}
			}

			// Game type specific set ups.
			if (AscentFoundation.GameParameters.Type == IgtGameParameters.GameType.Standard)
			{
				// Configure the cabinet based on current information from the CSI.
				ConfigureCabinetMonitors();
				// Initialize the window position.
				InitializeUnityWindows();
			}
			else if (!Application.isEditor)
			{
				InitializeUnityWindowsForStandalone();
			}
			else
			{
				//running in Editor mode
				InitializeUnityWindowsForEditor();
			}
		}

		/// <summary>
		/// Hides the windows immediately by updating the displays.
		/// </summary>
		public void HideWindowsImmediately()
		{
			foreach (var entry in windowAssociations)
			{
				SetDisplayParams(MonitorUtilities.GetMonitorRole(entry.Monitor), entry.DisplayIndex,
					0, 0, 0, 0, false);
			}

			// Though window controller hides the windows in advance, it should notify foundation as well.
			HideWindows();
		}

		/// <summary>
		/// Requests to hide the Unity windows.
		/// </summary>
		public void HideWindows()
		{
			// Only send out the hide requests if game had not requested already.
			if (!gameRequestedWindowStatus.HasValue || gameRequestedWindowStatus.Value)
			{
				foreach (var entry in windowAssociations)
				{
					Log.Instance.Info($"CabinetLib.RequestRepositionWindow displayIndex={entry.DisplayIndex}, windowId={entry.WindowId} size=0");
					CabinetLib.RequestRepositionWindow(entry.WindowId,
						new DesktopRectangle { x = 0, y = 0, w = 0, h = 0 },
						ViewportsInUse(entry.Monitor, 0f, 0f));
				}

				gameRequestedWindowStatus = false;
			}

			if (WindowsSized)
			{
				// Slows down player speed to prevent game running at full speed in the background.
				playerSpeedAdjuster.SlowDownPlayerSpeed();
			}
		}

		/// <summary>
		/// Requests to show the Unity windows.
		/// </summary>
		/// <param name="slowDownPlayerSpeed">
		/// Sets true if the player speed should be reduced.
		/// </param>
		public void ShowWindows(bool slowDownPlayerSpeed = false)
		{
			// Only send out the show requests if game had not requested already.
			// At present we only track the game requested status,
			// checks the actually display status if needs come up in the future.
			if (!gameRequestedWindowStatus.HasValue || !gameRequestedWindowStatus.Value)
			{
				foreach (var entry in windowAssociations)
				{
					// Only send out requests for windows that have been created.
					if (entry.WindowId != 0)
					{
						Log.Instance.Info($"CabinetLib.RequestRepositionWindow displayIndex={entry.DisplayIndex}, windowId={entry.WindowId} size=1");
						CabinetLib.RequestRepositionWindow(entry.WindowId,
							new DesktopRectangle { x = 0, y = 0, w = 1, h = 1 },
							ViewportsInUse(entry.Monitor, 1f, 1f));

						// Currently we only allow the user to show/hide windows altogether.
						// If needs come up in the future to show/hide a specific window,
						// we might need a flag for each window.
						gameRequestedWindowStatus = true;
					}
				}
			}

			if (slowDownPlayerSpeed)
			{
				// Slows down player speed to prevent game running at full speed when DisplayAsSuspended.
				playerSpeedAdjuster.SlowDownPlayerSpeed();
			}
			else
			{
				playerSpeedAdjuster.RestoreOriginalPlayerSpeed();
			}
		}

		/// <summary>
		/// Creates Window Ids and requests the initial position for the Unity game windows.
		/// </summary>
		private void InitializeUnityWindows()
		{
			foreach (var entry in windowAssociations)
			{
				var unityMonitorRole = MonitorUtilities.GetMonitorRole(entry.Monitor);
				var displayIndex = entry.DisplayIndex;

				Log.Instance.Info($"CabinetLib.CreatingWindow role={unityMonitorRole} displayIndex={displayIndex}");
				// Register window with CSI, and cache the window id.
				entry.WindowId = CabinetLib.CreateWindow(true, priority, new List<long> { (long)Display.displays[displayIndex].windowHandle }, true);
				Log.Instance.Info($"CabinetLib.CreatingWindow role={unityMonitorRole} displayIndex={displayIndex} windowId={entry.WindowId} done.");

				if (entry.WindowId == 0)
				{
					Log.Instance.Error("CSI: CreateWindow request failed");
				}
				else if (!gameRequestedWindowStatus.HasValue || gameRequestedWindowStatus.Value)
				{
					Log.Instance.Info($"CabinetLib.RequestRepositionWindow role={unityMonitorRole} displayIndex={displayIndex}, {entry.WindowId} size=1");

					// Only reposition the window if the ScreenBlanker has not hidden it
					// Otherwise, it's the ScreenBlanker's obligation to reposition it after showing up.
					CabinetLib.RequestRepositionWindow(entry.WindowId,
						new DesktopRectangle { x = 0, y = 0, w = 1, h = 1 },
						ViewportsInUse(entry.Monitor, 1f, 1f));
				}
			}
		}

		/// <summary>
		/// Gets the cabinet monitor configuration and cache monitor information.
		/// </summary>
		private void ConfigureCabinetMonitors()
		{
			if (MonitorConfigurations != null)
			{
				windowAssociations.Clear();

				// Sort the monitors by origin coordinates (left to right, top to bottom)
				// This is optimal because Unity sorts the monitors the same way.
				var sortedMonitors = MonitorConfigurations.OrderBy(monitor => monitor.DesktopCoordinates.x)
					.ThenBy(monitor => monitor.DesktopCoordinates.y);

				var configuredMonitors = new List<MonitorType>();
				foreach (var monitor in sortedMonitors)
				{
					var unityMonitorRole = MonitorUtilities.GetMonitorRole(monitor);
					var displayIndex = (int)unityMonitorRole;

					// Skip if this is the extended touch screen for Stepper cabinets.
					if (IgtScreen.IsMonitorEnabled(unityMonitorRole) &&
						monitor.Style == MonitorStyle.ExtendedTouchscreen)
					{
						Log.Instance.Error($"Skipping role={unityMonitorRole} displayIndex={displayIndex}");
						continue;
					}

					// Sanity check
					if (displayIndex >= Display.displays.Length)
					{
						Log.Instance.Error($"Invalid Unity display at index role={unityMonitorRole} displayIndex={displayIndex}");
					}
					else
					{
						var monitorType = MonitorUtilities.GetMonitorType(monitor);
						configuredMonitors.Add(monitorType);

						var displayWidth = monitor.DesktopCoordinates.w;
						var displayHeight = monitor.DesktopCoordinates.h;

						if (monitor.Style == MonitorStyle.Stereoscopic)
						{
							if (monitor.StereoscopicSettings.Frames.Count == 0)
							{
								throw new NotSupportedException("At least one stereoscopic frame must be defined.");
							}

							GetStereoscopicMonitorDimensions(monitor.StereoscopicSettings.Frames, out var width, out var height);
							// Assumes the window dimensions are the stereoscopic frames bounding box.
							displayWidth = width;
							displayHeight = height;
						}

						Log.Instance.Info($"Display.Activate role={unityMonitorRole} displayIndex={displayIndex} w={displayWidth} h={displayHeight}");
						// Activate Unity's display.
						Display.displays[displayIndex].Activate(displayWidth,
							displayHeight,
							DisplayRefreshRate);

						windowAssociations.Add(new WindowAssociation(0, monitor, displayIndex));
					}
				}

				ConfiguredMonitors = configuredMonitors;
			}
			else
			{
				Log.Instance.Error("Monitor configurations are not available.  Check CsiMonitor:GetConfigurationResponse message for information.");
			}
		}

		/// <summary>
		/// Hide all windows and reset the window IDs on the game side.
		/// This call must be followed by a CSI disconnection call so that
		/// the windows are released on the CSI Manager side as well.
		/// </summary>
		/// <remarks>
		/// When a client disconnects from the CSI Manager, CSI Manager would hide and release
		/// all the windows owned by the client.  Therefore, there is no need for the client to
		/// send the CSI messages for hiding and destroying windows before disconnection.
		/// This is to optimize the CSI performance.
		/// </remarks>
		public void DestroyUnityWindows()
		{
			foreach (var entry in windowAssociations)
			{
				SetDisplayParams(MonitorUtilities.GetMonitorRole(entry.Monitor), entry.DisplayIndex,
					0, 0, 0, 0, false);
				entry.WindowId = 0;
			}
		}

		/// <summary>
		/// Handler for processing the CSI window resize data and updating the viewports within Unity
		/// </summary>
		/// <param name="sender">Originator of the event.</param>
		/// <param name="windowSize">Argument containing the new window size.</param>
		private void WindowResizeHandler(object sender, WindowResizeEventArgs windowSize)
		{
			//GameLifeCycleTracing.Log.WindowSizeRequest((int) windowSize.RequestedWindow.WindowId, windowSize.RequestedWindow.Status);

			UpdateDisplay(windowSize.RequestedWindow);

			WindowsSized = true;

			// Post Window Resize Event
			CabinetWindowSizeChangedEvent?.Invoke(this, windowSize);
		}

		/// <summary>
		/// Handler for processing the CSI multi window resize data and updating the viewports within Unity.
		/// </summary>
		/// <param name="sender">Originator of the event.</param>
		/// <param name="multiWindowSize">Argument containing the multi window size.</param>
		private void MultiWindowResizeHandler(object sender, MultiWindowResizeEventArgs multiWindowSize)
		{
			foreach (var window in multiWindowSize.RequestedWindows)
			{
				//GameLifeCycleTracing.Log.WindowSizeRequest((int) window.WindowId, window.Status);
				UpdateDisplay(window);
			}

			WindowsSized = true;
		}

		/// <summary>
		/// Update display within Unity according to CSI window resize data.
		/// </summary>
		/// <param name="window">CSI window resize data.</param>
		private void UpdateDisplay(Window window)
		{
			Log.Instance.Info($"UpdateDisplay id='{window.WindowId}' status='{window.Status}'");

			var windowAssociation = windowAssociations.FirstOrDefault(entry => entry.WindowId == window.WindowId);
			if (windowAssociation == null)
			{
				Log.Instance.Error("CSI: Invalid monitor request.  Cannot find window of ID " + window.WindowId);
				return;
			}

			var unityMonitorRole = MonitorUtilities.GetMonitorRole(windowAssociation.Monitor);
			var displayIndex = windowAssociation.DisplayIndex;

			// Find the viewport for our display.
			var viewport = window.Viewports.Viewport.Find(vp => vp.ViewportId == (ushort)unityMonitorRole);
			if (viewport?.AdjustedViewportRectangle == null)
			{
				var message = viewport == null
					? "viewport is null"
					: "viewport.AdjustedViewportRectangle is null";

				Log.Instance.Error("CSI: Received invalid viewport: " + message);
				return;
			}

			if (windowAssociation.Monitor.Style == MonitorStyle.Stereoscopic)
			{
				if (windowAssociation.Monitor.StereoscopicSettings.Frames.Count == 0)
				{
					throw new NotSupportedException("At least one stereoscopic frame must be defined.");
				}

				GetStereoscopicMonitorDimensions(windowAssociation.Monitor.StereoscopicSettings.Frames, out var width,
					out var height);
				// Assumes the window dimensions are the stereoscopic frames bounding box.
				viewport.AdjustedViewportRectangle.w = width;
				viewport.AdjustedViewportRectangle.h = height;
				Display.displays[displayIndex].Activate(width, height, DisplayRefreshRate);
			}

			// Update window position and size
			SetDisplayParams(unityMonitorRole, displayIndex,
				viewport.AdjustedViewportRectangle.w,
				viewport.AdjustedViewportRectangle.h,
				viewport.AdjustedViewportRectangle.x,
				viewport.AdjustedViewportRectangle.y,
				window.Status);

			// Set the coordinate scale in case the window is scaled
			var scaleX = 1.0f;
			var scaleY = 1.0f;

			if (viewport.AdjustedViewportCoordinates != null)
			{
				scaleX = viewport.AdjustedViewportCoordinates.w > 0
					? 1.0f / viewport.AdjustedViewportCoordinates.w
					: 1.0f;
				scaleY = viewport.AdjustedViewportCoordinates.h > 0
					? 1.0f / viewport.AdjustedViewportCoordinates.h
					: 1.0f;
			}

			Log.Instance.Info($"Display.scale role={unityMonitorRole} displayIndex={displayIndex} x={scaleX}, y={scaleY}");
			Display.displays[displayIndex].scale = new Vector2(scaleX, scaleY);
		}

		/// <summary>
		/// Calculates the min/max bounds across all stereoscopic frames.
		/// </summary>
		private static void GetStereoscopicMonitorDimensions(IEnumerable<StereoscopicFrame> frames, out int width, out int height)
		{
			var xmin = int.MaxValue;
			var ymin = int.MaxValue;
			var xmax = int.MinValue;
			var ymax = int.MinValue;

			foreach (var frame in frames)
			{
				xmin = Math.Min(xmin, frame.X);
				ymin = Math.Min(ymin, frame.Y);
				xmax = Math.Max(xmax, frame.X + frame.Width);
				ymax = Math.Max(ymax, frame.Y + frame.Height);
			}

			width = xmax - xmin;
			height = ymax - ymin;
		}

		/// <summary>
		/// Handler for processing the CSI window Z order change request
		/// </summary>
		/// <param name="sender">Originator of the event</param>
		/// <param name="zOrder">Argument containing the z order that the window should be sent to.</param>
		private static void WindowZOrderHandler(object sender, WindowZOrderEventArgs zOrder)
		{
			// make a call into the engine to handle the z-order
			var moveToFront = zOrder.Position.AbsolutePosition == ChangeZOrderRequestAbsolutePosition.Front;

			foreach (var display in Display.displays)
			{
				display.SetWindowZOrder(moveToFront);
			}
		}

		/// <summary>
		/// Generates a list of viewports in use.
		/// </summary>
		/// <param name="monitorConfig">The <see cref="Monitor" /> object</param>
		/// .
		/// <param name='width'>Width of each viewport as a percentage (e.g. 0 is hidden, 1 is full-screen).</param>
		/// <param name='height'>Height of each viewport as a percentage (e.g. 0 is hidden, 1 is full-screen).</param>
		/// <returns>
		/// The CSI ViewportList.
		/// </returns>
		private static ViewportList ViewportsInUse(Monitor monitorConfig, float width, float height)
		{
			var monitorRole = MonitorUtilities.GetMonitorRole(monitorConfig);
			var viewports = new ViewportList();
			var viewport = new Viewport
			{
				DockPriority = 1,
				RequestedViewportCoordinates = new VirtualRectangle
				{
					w = width,
					h = height,
					x = monitorConfig.VirtualX,
					y = monitorConfig.VirtualY
				},
				ViewportId = (ushort)monitorRole,
				Dock = DockType.LeftTop,
			};

			viewports.Viewport.Add(viewport);

			return viewports;
		}

		private static void SetDisplayParams(UnityMonitorRole unityMonitorRole, int displayIndex,
			int w, int h, int x, int y, bool visible)
		{
			Log.Instance.Info($"Display.SetParams role={unityMonitorRole} displayIndex={displayIndex} w={w}, h={h}, x={x}, y={y}, visible={visible}");

			Display.displays[displayIndex].SetParams(w, h, x, y, visible);
		}

		#endregion
	}
}