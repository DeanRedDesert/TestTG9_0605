using System;
using System.Linq;
using IGT.Game.Core.Communication.Cabinet.CSI.Schemas;
using IGT.Game.SDKAssets.AscentBuildSettings;
using IgtUnityEngine;
using Midas.Core.Configuration;
using UnityEngine;
using CsiMonitorRole = IGT.Game.Core.Communication.Cabinet.CSI.Schemas.MonitorRole;
using UnityMonitorRole = IgtUnityEngine.MonitorRole;

namespace Midas.Ascent.Cabinet
{
	/// <summary>
	/// Utilities for monitor.
	/// </summary>
	internal static class MonitorUtilities
	{
		/// <summary>
		/// Maps a Unity monitor role to a <see cref="IGT.Game.Core.Communication.Cabinet.CSI.Schemas.MonitorRole" />.
		/// </summary>
		/// <param name="unityMonitor">The Unity monitor role.</param>
		/// <returns>The corresponding CSI monitor role.</returns>
		/// <exception cref="ApplicationException">
		/// Thrown if there is no matching CSI monitor for <paramref name="unityMonitor" />.
		/// </exception>
		public static CsiMonitorRole ToCsiMonitorRole(this UnityMonitorRole unityMonitor)
		{
			CsiMonitorRole returnValue;

			switch (unityMonitor)
			{
				case UnityMonitorRole.Main:
					returnValue = CsiMonitorRole.Main;
					break;

				case UnityMonitorRole.Top:
					returnValue = CsiMonitorRole.Top;
					break;

				case UnityMonitorRole.ButtonPanel:
					returnValue = CsiMonitorRole.ButtonPanel;
					break;

				case UnityMonitorRole.Topper:
					returnValue = CsiMonitorRole.Topper;
					break;

				default:
					// Throw an exception when Unity Engine defined a new monitor role that is unknown to SDK.
					throw new ApplicationException($"Cannot map {unityMonitor} to a CSI monitor.");
			}

			return returnValue;
		}

		/// <summary>
		/// Returns a <see cref="UnityMonitorRole" /> given a <see cref="Monitor" /> object.
		/// </summary>
		/// <param name="monitor">The <see cref="Monitor" /> object.</param>
		/// <returns>The corresponding <see cref="UnityMonitorRole" />.</returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="monitor" /> is null.
		/// </exception>
		/// <exception cref="ApplicationException">
		/// Thrown if there is no matching monitor role for <paramref name="monitor" />.
		/// </exception>
		public static UnityMonitorRole GetMonitorRole(Monitor monitor)
		{
			if (monitor == null)
			{
				throw new ArgumentNullException(nameof(monitor));
			}

			UnityMonitorRole returnValue;

			switch (monitor.Role)
			{
				case CsiMonitorRole.Main:
				{
					returnValue = UnityMonitorRole.Main;
					break;
				}

				case CsiMonitorRole.Top:
					returnValue = UnityMonitorRole.Top;
					break;

				case CsiMonitorRole.ButtonPanel:
					returnValue = UnityMonitorRole.ButtonPanel;
					break;

				case CsiMonitorRole.Topper:
					returnValue = UnityMonitorRole.Topper;
					break;

				default:
					// Throw an exception when CSI defined a new monitor that is unknown to Unity Engine.
					throw new ApplicationException($"Cannot map {monitor.Role} + {monitor.Style} to a Unity monitor role.");
			}

			return returnValue;
		}

		/// <summary>
		/// Returns the resolution of  monitor .
		/// </summary>
		/// <remarks>
		/// Editor: Always returns the reference resolution.
		/// Player:
		/// 1. The monitor resolution if the monitor is available.
		/// 2. The reference resolution if the monitor is not available.
		/// </remarks>
		/// <param name="monitorRole">The monitor role for inquiry.</param>
		/// <returns>
		/// The width and height of the specified monitor's resolution.
		/// </returns>
		private static SizeRect GetMonitorResolution(UnityMonitorRole monitorRole)
		{
			if (!Application.isEditor)
			{
				var monitorConfig = GetMonitorConfiguration(monitorRole);

				if (monitorConfig != null)
				{
					return new SizeRect(monitorConfig.DesktopCoordinates.w,
						monitorConfig.DesktopCoordinates.h);
				}
			}

			return new SizeRect(IgtScreen.GetMonitorReferenceResolutionWidth(monitorRole),
				IgtScreen.GetMonitorReferenceResolutionHeight(monitorRole));
		}

		/// <summary>
		/// Gets the aspect ratio for the specified monitor based on the monitor resolution.
		/// </summary>
		/// <remarks>
		/// The aspect ratio is calculated by the return value of <see cref="GetMonitorResolution" />.
		/// </remarks>
		/// <param name="monitorRole">The monitor role for inquiry.</param>
		/// <returns>
		/// The aspect ratio for the specified monitor.
		/// 0 if the aspect ratio cannot be calculated due to a divisor of 0.
		/// </returns>
		public static float GetMonitorAspect(UnityMonitorRole monitorRole)
		{
			var result = 0.0f;

			var monitorSize = GetMonitorResolution(monitorRole);

			if (monitorSize.Height > 0) // Prevent dividing 0.
			{
				result = (float)monitorSize.Width / monitorSize.Height;
			}

			return result;
		}

		/// <summary>
		/// Returns the current size of the display on the specified monitor.
		/// </summary>
		/// <remarks>
		/// Editor: Always returns the reference resolution.
		/// Player:
		/// 1. The display size at runtime if the monitor is available.
		/// 2. The reference resolution if the monitor is not available.
		/// Note that the display size will be 0 if the window is currently hidden.
		/// </remarks>
		/// <param name="monitorRole">The monitor role for inquiry.</param>
		/// <returns>
		/// The width and height of the current display on the specified monitor.
		/// </returns>
		private static SizeRect GetDisplaySize(UnityMonitorRole monitorRole)
		{
			if (!Application.isEditor)
			{
				// Find the Unity display corresponding to this monitor
				var display = Display.displays[(int)monitorRole];
				if (display != null && display.renderingHeight != 0)
				{
					return new SizeRect(display.renderingWidth, display.renderingHeight);
				}
			}

			return new SizeRect(IgtScreen.GetMonitorReferenceResolutionWidth(monitorRole),
				IgtScreen.GetMonitorReferenceResolutionHeight(monitorRole));
		}

		/// <summary>
		/// Gets the aspect ratio of current display on the specified monitor.
		/// </summary>
		/// <remarks>
		/// The aspect ratio is calculated by the return value of <see cref="GetDisplaySize" />.
		/// </remarks>
		/// <param name="monitorRole">The monitor role for inquiry.</param>
		/// <returns>
		/// The aspect ratio of current display on the specified monitor.
		/// 0 if the aspect ratio cannot be calculated due to a divisor of 0.
		/// </returns>
		public static float GetDisplayAspect(UnityMonitorRole monitorRole)
		{
			var result = 0.0f;

			var displaySize = GetDisplaySize(monitorRole);

			if (displaySize.Height > 0) // Prevent dividing 0.
			{
				result = (float)displaySize.Width / displaySize.Height;
			}

			return result;
		}

		/// <summary>
		/// Checks if the specified monitor supports stereoscopic display.
		/// </summary>
		/// <param name="monitorRole">The monitor role for inquiry.</param>
		/// <returns>True if the specified monitor is stereoscopic, false otherwise.</returns>
		public static bool IsMonitorStereoscopic(UnityMonitorRole monitorRole)
		{
			var monitorConfig = GetMonitorConfiguration(monitorRole);

			return monitorConfig is { Style: MonitorStyle.Stereoscopic };
		}

		/// <summary>
		/// Checks if the specified monitor is of the MLD type.
		/// </summary>
		/// <param name="monitorRole">The monitor role for inquiry.</param>
		/// <returns>True if the specified monitor is MLD, false otherwise.</returns>
		public static bool IsMonitorMld(UnityMonitorRole monitorRole)
		{
			var monitorConfig = GetMonitorConfiguration(monitorRole);

			return monitorConfig is { Style: MonitorStyle.PureDepth };
		}

		/// <summary>
		/// Checks if the specified monitor is a portrait monitor.
		/// </summary>
		/// <param name="monitorRole">The monitor role for inquiry.</param>
		/// <returns>True if the specified monitor is portrait, false otherwise.</returns>
		public static bool IsMonitorPortrait(UnityMonitorRole monitorRole)
		{
			var monitorConfig = GetMonitorConfiguration(monitorRole);

			return monitorConfig is { Aspect: MonitorAspect.Portrait };
		}

		/// <summary>
		/// Determines if the specified monitor is available or not.
		/// </summary>
		/// <remarks>
		/// Editor returns true if the monitor is enabled in the player settings, and false otherwise.
		/// Player:
		/// 1. Running with foundation: True if monitor reported by CSI AND enabled in player settings, false otherwise.
		/// 2. Running in standalone: True if monitor is enabled in Player Settings.
		/// </remarks>
		/// <param name="monitorRole">The Unity monitor role.</param>
		/// <returns>True if the monitor is available.</returns>
		public static bool IsMonitorAvailable(UnityMonitorRole monitorRole)
		{
			if (Application.isEditor)
			{
				// IgtScreen.IsMonitorEnabled(monitorRole) returns the player settings value which is
				// the same value as IgtPlayerSettings.GetMonitorEnabled(monitorRole) when running in the Editor.
				// The gameEntry.GameType is unknown and a null reference exception is thrown
				// if it is called when the application is not running.
				return IgtScreen.IsMonitorEnabled(monitorRole);
			}

			bool result;
			if (AscentFoundation.GameParameters.Type == IgtGameParameters.GameType.Standard)
			{
				result = GetMonitorConfiguration(monitorRole) != null && IgtScreen.IsMonitorEnabled(monitorRole);
			}
			else
			{
				result = IgtScreen.IsMonitorEnabled(monitorRole);
			}

			return result;
		}

		public static MonitorType GetMonitorType(Monitor monitor)
		{
			if (monitor == null)
			{
				throw new ArgumentNullException(nameof(monitor));
			}

			switch (monitor.Role)
			{
				case CsiMonitorRole.Main:
					return monitor.Aspect == MonitorAspect.Portrait ? MonitorType.MainPortrait : MonitorType.MainStandard;
				case CsiMonitorRole.Top:
					return monitor.Aspect == MonitorAspect.Portrait ? MonitorType.TopPortrait : MonitorType.TopStandard;
				case CsiMonitorRole.ButtonPanel:
					return monitor.Aspect == MonitorAspect.Portrait ? MonitorType.ButtonPanelPortrait : MonitorType.ButtonPanelStandard;
				case CsiMonitorRole.Topper:
					return monitor.Aspect == MonitorAspect.Portrait ? MonitorType.TopperPortrait : MonitorType.TopperStandard;
				default:
					// Throw an exception when CSI defined a new monitor that is unknown to GGA.
					throw new ApplicationException($"Cannot map {monitor.Role} + {monitor.Aspect} to a GGA MonitorType.");
			}
		}

		/// <summary>
		/// Gets the configuration of specified monitor.
		/// </summary>
		/// <param name="monitorRole">The monitor role for inquiry.</param>
		/// <returns>The monitor configuration.  Null if the specified monitor is not available.</returns>
		private static Monitor GetMonitorConfiguration(UnityMonitorRole monitorRole)
		{
			Monitor result = null;

			if (AscentCabinet.MonitorConfigurations != null)
			{
				result = AscentCabinet.MonitorConfigurations.FirstOrDefault(monitor => GetMonitorRole(monitor) == monitorRole);
			}

			return result;
		}
	}
}