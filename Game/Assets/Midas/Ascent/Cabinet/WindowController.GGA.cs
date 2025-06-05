using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using IGT.Game.Core.Communication.Cabinet.CSI.Schemas;
using IgtUnityEngine;
using Midas.Core.Configuration;
using UnityEngine;
using UnityMonitorRole = IgtUnityEngine.MonitorRole;

// ReSharper disable InconsistentNaming - Keep inconsistent to match windows APIs.
// ReSharper disable UnusedMember.Local - Keep unused members for completeness.

namespace Midas.Ascent.Cabinet
{
	internal sealed partial class WindowController
	{
		#region Public

		public IReadOnlyList<MonitorType> ConfiguredMonitors { get; private set; }

		#endregion

		#region Private

		private enum SystemParametersInfoActions : uint
		{
			SPI_GETWORKAREA = 0x0030
		}

		[Flags]
		private enum WindowStyles : uint
		{
			WS_BORDER = 0x800000,
			WS_CAPTION = 0xc00000,
			WS_CHILD = 0x40000000,
			WS_CLIPCHILDREN = 0x2000000,
			WS_CLIPSIBLINGS = 0x4000000,
			WS_DISABLED = 0x8000000,
			WS_DLGFRAME = 0x400000,
			WS_GROUP = 0x20000,
			WS_HSCROLL = 0x100000,
			WS_MAXIMIZE = 0x1000000,
			WS_MAXIMIZEBOX = 0x10000,
			WS_MINIMIZE = 0x20000000,
			WS_MINIMIZEBOX = 0x20000,
			WS_OVERLAPPED = 0x0,
			WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_SIZEFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
			WS_POPUP = 0x80000000u,
			WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
			WS_SIZEFRAME = 0x40000,
			WS_SYSMENU = 0x80000,
			WS_TABSTOP = 0x10000,
			WS_VISIBLE = 0x10000000,
			WS_VSCROLL = 0x200000
		}

		private void InitializeUnityWindowsForEditor()
		{
			var configuredMonitors = new List<MonitorType>();
			var monitorConfigs = AscentCabinet.MonitorConfigurations;
			foreach (var monitor in monitorConfigs)
			{
				var monitorRole = MonitorUtilities.GetMonitorRole(monitor);
				if (monitor.Style == MonitorStyle.ExtendedTouchscreen)
				{
					continue;
				}

				IgtScreen.SetMonitorEnabled(monitorRole, true);
				var monitorType = MonitorUtilities.GetMonitorType(monitor);
				configuredMonitors.Add(monitorType);
			}

			ConfiguredMonitors = configuredMonitors;
		}

		/// <summary>
		///     Activates Unity displays in standalone.
		/// </summary>
		private void InitializeUnityWindowsForStandalone()
		{
			var gameParams = AscentFoundation.GameParameters;
			var monitors = AscentCabinet.MonitorConfigurations
				.ToDictionary(MonitorUtilities.GetMonitorRole, monitor => (monitor, new DesktopRectangle
				{
					h = monitor.DesktopCoordinates.h,
					w = monitor.DesktopCoordinates.w,
					x = monitor.DesktopCoordinates.x,
					y = monitor.DesktopCoordinates.y
				}));
			Log.Instance.Info($"FitToScreen={gameParams.FitToScreen}");
			// Are we fitting all monitors to main display?
			if (gameParams.FitToScreen)
			{
				// find the height and width of main screen
				var rcWorkArea = new RECT(0, 0, 0, 0);
				SystemParametersInfo(SystemParametersInfoActions.SPI_GETWORKAREA, 0, ref rcWorkArea, 0);
				Log.Instance.Info($"rcWorkArea={rcWorkArea}");
				var mainMonitorHeight = rcWorkArea.Height;
				var mainMonitorTop = rcWorkArea.Top;
				var mainMonitorLeft = rcWorkArea.Left;
				var totalMonitorsHeight = 0;
				var dpi = Screen.dpi;
				var rcFrame = new RECT(0, 0, 0, 0);
				AdjustWindowRectExForDpi(ref rcFrame, (int)WindowStyles.WS_OVERLAPPEDWINDOW, false, 0, (uint)dpi);
				Log.Instance.Info($"rcFrame={rcFrame}");
				var titleBarHeight = -rcFrame.Top;
				Log.Instance.Info($"dpi={dpi}, mainMonitorHeight={mainMonitorHeight}, titleBarHeight={titleBarHeight}");
				var monitorConfigs = AscentCabinet.MonitorConfigurations;
				var titleBarCounter = 0;
				foreach (var monitor in monitorConfigs)
				{
					var unityMonitorRole = MonitorUtilities.GetMonitorRole(monitor);
					IgtScreen.SetMonitorEnabled(unityMonitorRole, true);
					titleBarCounter++;
					totalMonitorsHeight += monitor.DesktopCoordinates.h;
				}

				// calculate oversize Ratio
				var oversizeRatio = (float)(mainMonitorHeight - titleBarCounter * titleBarHeight) / totalMonitorsHeight;

				foreach (var monitorKeyValuePair in monitors)
				{
					var unityMonitorRole = monitorKeyValuePair.Key;
					var desktopPosition = monitorKeyValuePair.Value.Item2;
					desktopPosition.h = (int)(desktopPosition.h * oversizeRatio);
					desktopPosition.w = (int)(desktopPosition.w * oversizeRatio);

					switch (unityMonitorRole)
					{
						case UnityMonitorRole.Main:
							desktopPosition.y = (int)(oversizeRatio * (GetHeight(monitors, UnityMonitorRole.Top) + GetHeight(monitors, UnityMonitorRole.Topper)));
							desktopPosition.y += monitors.ContainsKey(UnityMonitorRole.Top) ? titleBarHeight : 0;
							desktopPosition.y += monitors.ContainsKey(UnityMonitorRole.Topper) ? titleBarHeight : 0;
							break;

						case UnityMonitorRole.Top:
							desktopPosition.y = (int)(oversizeRatio * GetHeight(monitors, UnityMonitorRole.Topper));
							desktopPosition.y += monitors.ContainsKey(UnityMonitorRole.Topper) ? titleBarHeight : 0;
							break;

						case UnityMonitorRole.ButtonPanel:
							desktopPosition.y = (int)(oversizeRatio * (GetHeight(monitors, UnityMonitorRole.Main) + GetHeight(monitors, UnityMonitorRole.Top)
								+ GetHeight(monitors, UnityMonitorRole.Topper)));
							desktopPosition.y += monitors.ContainsKey(UnityMonitorRole.Main) ? titleBarHeight : 0;
							desktopPosition.y += monitors.ContainsKey(UnityMonitorRole.Top) ? titleBarHeight : 0;
							desktopPosition.y += monitors.ContainsKey(UnityMonitorRole.Topper) ? titleBarHeight : 0;
							break;

						case UnityMonitorRole.Topper:
							desktopPosition.y = 0;
							break;
					}

					desktopPosition.x = mainMonitorLeft;
					desktopPosition.y += mainMonitorTop;
				}
			}

			var configuredMonitors = new List<MonitorType>();
			// size and position monitors, we assume stacked layout in the following order from top to bottom:
			// Topper, Top, Main, PhysicalButton Panel.
			foreach (var monitorKeyValuePair in monitors)
			{
				var unityMonitorRole = monitorKeyValuePair.Key;
				var display = Display.displays[(int)unityMonitorRole];
				var desktopPosition = monitorKeyValuePair.Value.Item2;
				display.Activate(desktopPosition.w, desktopPosition.h, DisplayRefreshRate);
				var monitorType = MonitorUtilities.GetMonitorType(monitorKeyValuePair.Value.monitor);
				configuredMonitors.Add(monitorType);

				Log.Instance.Info(
					$"Position of display. unityMonitorRole={unityMonitorRole}, desktopPosition={desktopPosition.x},{desktopPosition.y}, {desktopPosition.w}x{desktopPosition.h}");
				display.SetParams(desktopPosition.w,
					desktopPosition.h,
					desktopPosition.x,
					desktopPosition.y);
			}

			ConfiguredMonitors = configuredMonitors;
		}

		private static int GetHeight(IReadOnlyDictionary<UnityMonitorRole, (Monitor monitor, DesktopRectangle)> monitorPairs, UnityMonitorRole unityMonitorRole)
		{
			return monitorPairs.TryGetValue(unityMonitorRole, out var p) ? p.Item1.DesktopCoordinates.h : 0;
		}

		[DllImport("user32.dll")]
		private static extern bool SystemParametersInfo(SystemParametersInfoActions uAction, uint uParam, ref RECT lpvParam, uint fuWinIni);

		[DllImport("user32")]
		private static extern bool AdjustWindowRectExForDpi(ref RECT lpRect, uint dwStyle, bool bMenu, uint dwExStyle, uint dpi);

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT
		{
			#region Public

			public RECT(int left, int top, int right, int bottom)
			{
				Left = left;
				Top = top;
				Right = right;
				Bottom = bottom;
			}

			public static bool operator ==(RECT r1, RECT r2)
			{
				return r1.Equals(r2);
			}

			public static bool operator !=(RECT r1, RECT r2)
			{
				return !r1.Equals(r2);
			}

			private bool Equals(RECT r)
			{
				return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
			}

			public override bool Equals(object obj)
			{
				if (obj is RECT rect)
				{
					return Equals(rect);
				}

				return false;
			}

			public override int GetHashCode()
			{
				return Left.GetHashCode() ^ (Right.GetHashCode() << 2) ^ (Top.GetHashCode() >> 2) ^ (Bottom.GetHashCode() >> 1);
			}

			public override string ToString()
			{
				return string.Format(CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
			}

			public int X
			{
				get => Left;
			}

			public int Y
			{
				get => Top;
			}

			public int Height
			{
				get => Bottom - Top;
			}

			public int Width
			{
				get => Right - Left;
			}

			public readonly int Left;
			public readonly int Top;
			public readonly int Right;
			public readonly int Bottom;

			#endregion
		}

		#endregion
	}
}