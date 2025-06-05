using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core.Configuration;
using Midas.Core.General;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using UnityEngine;

namespace Midas.Presentation.General
{
	[Serializable]
	public sealed class MonitorContent
	{
		[SerializeField]
		[Tooltip("The monitor type that this content is designed for.")]
		private MonitorType monitorType;

		[SerializeField]
		[Tooltip("Content that you wish to be turned on or instantiated depending on the monitor type.")]
		private GameObject[] content;

		public MonitorType MonitorType => monitorType;
		public IReadOnlyList<GameObject> Content => content;
	}

	public sealed class MonitorConfigContentEnabler : MonoBehaviour
	{
		private readonly AutoUnregisterHelper autoUnregisterHelper = new AutoUnregisterHelper();

		[SerializeField]
		[Tooltip("Content that you wish to be turned on or instantiated depending on the monitor type.")]
		private MonitorContent[] content;

		private void OnEnable()
		{
			autoUnregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.ConfigurationStatus, nameof(ConfigurationStatus.ConfiguredMonitors), (_, __) => UpdateMonitorConfig(StatusDatabase.ConfigurationStatus.ConfiguredMonitors));

			var cm = StatusDatabase.ConfigurationStatus?.ConfiguredMonitors;
			if (cm != null)
				UpdateMonitorConfig(cm);
		}

		private void UpdateMonitorConfig(IReadOnlyList<MonitorType> monitorTypes)
		{
			foreach (var entry in content)
			{
				var en = monitorTypes?.Contains(entry.MonitorType) == true;

				foreach (var o in entry.Content)
				{
					o.SetActive(en);
				}
			}
		}

		private void OnDisable()
		{
			autoUnregisterHelper.UnRegisterAll();
			foreach (var entry in content)
			{
				foreach (var o in entry.Content)
					if (o)
						o.SetActive(false);
			}
		}

#if UNITY_EDITOR

		public void UpdateMonitorConfigForEditor(IReadOnlyList<MonitorType> monitorTypes)
		{
			UpdateMonitorConfig(monitorTypes);
		}

#endif
	}
}