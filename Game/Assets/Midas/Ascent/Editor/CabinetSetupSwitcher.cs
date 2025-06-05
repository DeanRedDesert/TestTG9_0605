using System;
using System.Collections.Generic;
using System.Xml.Linq;
using IGT.Game.Core.Communication.Cabinet.CSI.Schemas;
using IgtUnityEditor;
using IgtUnityEngine;
using Midas.Presentation.General;
using UnityEditor;
using UnityEngine;
using MonitorRole = IgtUnityEngine.MonitorRole;
using MonitorType = Midas.Core.Configuration.MonitorType;

namespace Midas.Ascent.Editor
{
	public enum CabinetConfig
	{
		PeakDual,
		PeakCurve,
		CrystalDual,
		CrystalCurve
	}

	public static class CabinetSetupSwitcher
	{
		private const string CsiConfigFilename = "CsiConfig.xml";

		private const string PeakCurveMenuName = "Midas/Configuration/Cabinet Setup/Peak Curve";
		private const string PeakDualMenuName = "Midas/Configuration/Cabinet Setup/Peak Dual";
		private const string CrystalCurveMenuName = "Midas/Configuration/Cabinet Setup/Crystal Curve";
		private const string CrystalDualMenuName = "Midas/Configuration/Cabinet Setup/Crystal Dual";

		public static CabinetConfig GetCurrentCabinet()
		{
			// Right now, run with top monitor disabled means portrait.
			// Could be that somebody hand edits things and this may need to change.
			// Peak vs Curve is 16:9 vs 16:10 aspect DPP.

			var isDual = IgtPlayerSettings.GetMonitorEnabled(MonitorRole.Top);
			var refWidth = IgtPlayerSettings.GetMonitorReferenceResolutionWidth(MonitorRole.ButtonPanel);
			var refHeight = IgtPlayerSettings.GetMonitorReferenceResolutionHeight(MonitorRole.ButtonPanel);
			var ar = refWidth / (float)refHeight;

			return isDual switch
			{
				true when ar > 1.6f => CabinetConfig.PeakDual,
				true => CabinetConfig.CrystalDual,
				false when ar > 1.6f => CabinetConfig.PeakCurve,
				false => CabinetConfig.CrystalCurve
			};
		}

		public static void SetCabinet(CabinetConfig cabinetConfig)
		{
			switch (cabinetConfig)
			{
				case CabinetConfig.PeakCurve:
					SetPeakCurve();
					break;
				case CabinetConfig.PeakDual:
					SetPeakDual();
					break;
				case CabinetConfig.CrystalCurve:
					SetCrystalCurve();
					break;
				case CabinetConfig.CrystalDual:
					SetCrystalDual();
					break;
			}
		}

		[MenuItem(PeakCurveMenuName)]
		public static void SetPeakCurve() => SetCurve(true);

		[MenuItem(PeakDualMenuName)]
		public static void SetPeakDual() => SetDual(true);

		[MenuItem(CrystalCurveMenuName)]
		public static void SetCrystalCurve() => SetCurve(false);

		[MenuItem(CrystalDualMenuName)]
		public static void SetCrystalDual() => SetDual(false);

		private static void SetCurve(bool peak)
		{
			UpdateMonitorSetup(new[]
			{
				GenerateMonitor("Main", MonitorRole.Main, MonitorAspect.Portrait, 2160, 3840),
				GenerateMonitor("ButtonPanel", MonitorRole.ButtonPanel, MonitorAspect.Standard, peak ? 1920 : 1280, peak ? 1080 : 800),
				GenerateMonitor("Topper", MonitorRole.Topper, MonitorAspect.Standard, 1920, 1080)
			});

			SetHardwareButtons(peak);
			SendMonitorUpdate(new[] { MonitorType.MainPortrait, MonitorType.ButtonPanelStandard, MonitorType.TopperStandard });
		}

		[MenuItem(PeakCurveMenuName, true)]
		[MenuItem(PeakDualMenuName, true)]
		[MenuItem(CrystalCurveMenuName, true)]
		[MenuItem(CrystalDualMenuName, true)]
		private static bool SetPortraitValidate()
		{
			var monitorMode = GetCurrentCabinet();
			Menu.SetChecked(PeakCurveMenuName, monitorMode == CabinetConfig.PeakCurve);
			Menu.SetChecked(PeakDualMenuName, monitorMode == CabinetConfig.PeakDual);
			Menu.SetChecked(CrystalCurveMenuName, monitorMode == CabinetConfig.CrystalCurve);
			Menu.SetChecked(CrystalDualMenuName, monitorMode == CabinetConfig.CrystalDual);
			return !Application.isPlaying;
		}

		private static void SetDual(bool peak)
		{
			UpdateMonitorSetup(new[]
			{
				GenerateMonitor("Main", MonitorRole.Main, MonitorAspect.Standard, 1920, 1080),
				GenerateMonitor("Top", MonitorRole.Top, MonitorAspect.Standard, 1920, 1080),
				GenerateMonitor("ButtonPanel", MonitorRole.ButtonPanel, MonitorAspect.Standard, peak ? 1920 : 1280, peak ? 1080 : 800),
				GenerateMonitor("Topper", MonitorRole.Topper, MonitorAspect.Standard, 1920, 1080)
			});

			SetHardwareButtons(peak);
			SendMonitorUpdate(new[] { MonitorType.MainStandard, MonitorType.ButtonPanelStandard, MonitorType.TopStandard, MonitorType.TopperStandard });
		}

		private static void SendMonitorUpdate(IReadOnlyList<MonitorType> monitorTypes)
		{
			var monitors = Resources.FindObjectsOfTypeAll<MonitorConfigContentEnabler>();
			foreach (var monitorDetection in monitors)
			{
				if (!PrefabUtility.GetCorrespondingObjectFromSource(monitorDetection) && !PrefabUtility.GetPrefabInstanceHandle(monitorDetection.gameObject))
					monitorDetection.UpdateMonitorConfigForEditor(monitorTypes);
			}
		}

		private static void SetHardwareButtons(bool peak)
		{
			var csiConfigurations = XElement.Load(CsiConfigFilename);
			var serviceSettings = csiConfigurations.Element("ServiceSettings");
			if (serviceSettings == null)
			{
				serviceSettings = new XElement("ServiceSettings");
				csiConfigurations.Add(serviceSettings);
			}

			var promptPlayerEl = serviceSettings.Element("PromptPlayerOnCashout");
			serviceSettings.RemoveNodes();

			if (promptPlayerEl != null)
				serviceSettings.Add(promptPlayerEl);

			if (peak)
			{
				var emuButtons = new XElement("EmulatableButtons");
				emuButtons.Add(new XElement("EmulatableButton", "Cashout"));
				emuButtons.Add(new XElement("EmulatableButton", "Service"));
				serviceSettings.Add(emuButtons);
			}

			csiConfigurations.Save(CsiConfigFilename);
		}

		private static void UpdateMonitorSetup(XElement[] allMonitors)
		{
			IgtScreen.SetMonitorEnabled(MonitorRole.Main, false);
			IgtScreen.SetMonitorEnabled(MonitorRole.Top, false);
			IgtScreen.SetMonitorEnabled(MonitorRole.ButtonPanel, false);

			var csiConfigurations = XElement.Load(CsiConfigFilename);
			var monitorSettings = csiConfigurations.Element("MonitorSettings");
			var monitors = monitorSettings?.Element("Monitors");
			monitors?.RemoveNodes();

			foreach (var monitor in allMonitors)
			{
				Enum.TryParse(monitor.Element("Aspect")!.Value, out MonitorAspect monitorAspect);
				var desktopCoordinates = monitor.Element("DesktopCoordinates")!;
				int.TryParse(desktopCoordinates.Element("h")!.Value, out var height);
				int.TryParse(desktopCoordinates.Element("w")!.Value, out var width);

				Enum.TryParse(monitor.Element("Role")!.Value, out MonitorRole monitorRole);
				IgtScreen.SetMonitorEnabled(monitorRole, true);
				IgtScreen.SetMonitorReferenceResolutionWidth(monitorRole, width);
				IgtScreen.SetMonitorReferenceResolutionHeight(monitorRole, height);

				monitors?.Add(GenerateMonitor(monitor.Element("Role")!.Value, (MonitorRole)Enum.Parse(typeof(MonitorRole), monitor.Element("Role")!.Value), monitorAspect, width, height));
			}

			csiConfigurations.Save(CsiConfigFilename);
		}

		private static XElement GenerateMonitor(string deviceId, MonitorRole csiMonitorRole, MonitorAspect csiMonitorAspect, int w, int h)
		{
			var monitor = new XElement("Monitor");
			monitor.Add(new XElement("DeviceId", deviceId));
			monitor.Add(new XElement("Role", csiMonitorRole));
			monitor.Add(new XElement("Style", MonitorStyle.Normal));
			monitor.Add(new XElement("Aspect", csiMonitorAspect));

			var model = new XElement("Model");
			model.Add(new XElement("Manufacturer", "Midas"));
			model.Add(new XElement("Model", "Hand of Midas"));
			model.Add(new XElement("Version", "1.0"));
			monitor.Add(model);

			var desktopCoordinates = new XElement("DesktopCoordinates");
			desktopCoordinates.Add(new XElement("x", 0));
			desktopCoordinates.Add(new XElement("y", 0));
			desktopCoordinates.Add(new XElement("w", w));
			desktopCoordinates.Add(new XElement("h", h));
			monitor.Add(desktopCoordinates);

			monitor.Add(new XElement("VirtualX", 0));
			monitor.Add(new XElement("VirtualY", 0));
			monitor.Add(new XElement("ColorProfileId", 0));

			return monitor;
		}
	}
}