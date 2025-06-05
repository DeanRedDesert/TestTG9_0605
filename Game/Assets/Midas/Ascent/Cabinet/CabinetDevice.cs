using System;
using System.Collections.Generic;
using System.Linq;
using IGT.Game.Core.Communication.Cabinet;
using IGT.Game.Core.Communication.Cabinet.CSI.Schemas;
using static Midas.Ascent.Cabinet.AscentCabinet;

namespace Midas.Ascent.Cabinet
{
	internal abstract class CabinetDevice : ICabinetController
	{
		private readonly DeviceType deviceType;
		private readonly List<(string deviceId, bool acquired)> deviceStatus = new List<(string, bool)>();

		public event Action<DeviceType, string, bool> DeviceAcquired;

		protected CabinetDevice(DeviceType deviceType)
		{
			this.deviceType = deviceType;
		}

		protected void RequestAcquireDevice(string deviceId)
		{
			if (!IsAcquired(deviceId))
			{
				// RequestAcquireDevice() needs to be called so that we can receive the ButtonPressedEvent

				var result = CabinetLib.RequestAcquireDevice(deviceType, deviceId, Priority.Game);
				SetAcquired(deviceId, result.Acquired);

				try
				{
					CabinetLib.RequestEventRegistration(deviceType, deviceId);
				}
				catch (ResourceManagementCategoryException)
				{
					Log.Instance.Info($"Event registration not supported: '{deviceId}'");
				}
			}
		}

		#region ICabinetController Members

		public virtual void Init()
		{
			CabinetLib.DeviceConnectedEvent += OnDeviceConnectedEvent;
			CabinetLib.DeviceRemovedEvent += OnDeviceRemovedEvent;
			CabinetLib.DeviceAcquiredEvent += OnDeviceAcquiredEvent;
			CabinetLib.DeviceReleasedEvent += OnDeviceReleasedEvent;
		}

		public virtual void OnBeforeLoadGame()
		{
		}

		public virtual void Resume()
		{
			deviceStatus.Clear();

			foreach (var device in CabinetLib.GetConnectedDevices())
			{
				if (device.DeviceType == deviceType)
				{
					deviceStatus.Add((device.DeviceId, false));
					SetConnected(device.DeviceId, true);
				}
			}
		}

		public virtual void Pause()
		{
			foreach (var (deviceId, acquired) in deviceStatus)
			{
				try
				{
					CabinetLib.ReleaseEventRegistration(deviceType, deviceId);
				}
				catch (ResourceManagementCategoryException)
				{
					Log.Instance.Info($"Event registration not supported: '{deviceId}'");
				}

				if (acquired)
				{
					CabinetLib.ReleaseDevice(deviceType, deviceId);
				}
			}

			deviceStatus.Clear();
		}

		public virtual void OnAfterUnLoadGame()
		{
		}

		public virtual void DeInit()
		{
			CabinetLib.DeviceConnectedEvent -= OnDeviceConnectedEvent;
			CabinetLib.DeviceRemovedEvent -= OnDeviceRemovedEvent;
			CabinetLib.DeviceAcquiredEvent -= OnDeviceAcquiredEvent;
			CabinetLib.DeviceReleasedEvent -= OnDeviceReleasedEvent;
		}

		#endregion

		#region Private Methods

		private void OnDeviceConnectedEvent(object sender, DeviceConnectedEventArgs e)
		{
			if (e.DeviceName == deviceType)
			{
				SetConnected(e.DeviceId, true);
			}
		}

		private void OnDeviceRemovedEvent(object sender, DeviceRemovedEventArgs e)
		{
			if (e.DeviceName == deviceType)
			{
				SetConnected(e.DeviceId, false);
			}
		}

		private void OnDeviceAcquiredEvent(object sender, DeviceAcquiredEventArgs e)
		{
			if (e.DeviceName == deviceType)
			{
				SetAcquired(e.DeviceId, true);
			}
		}

		private void OnDeviceReleasedEvent(object sender, DeviceReleasedEventArgs e)
		{
			if (e.DeviceName == deviceType)
			{
				SetAcquired(e.DeviceId, false);
			}
		}

		private void SetConnected(string deviceId, bool connected)
		{
			Log.Instance.Info($"DeviceId: {deviceId} , connected: {connected}");
			if (connected)
			{
				if (!IsAcquired(deviceId))
				{
					RequestAcquireDevice(deviceId);
				}
			}
			else
			{
				if (IsAcquired(deviceId))
				{
					SetAcquired(deviceId, false);
				}
			}
		}

		private void SetAcquired(string deviceId, bool acquired)
		{
			Log.Instance.Info($"DeviceId: {deviceId} , acquired: {acquired}");

			var found = false;
			for (var index = 0; index < deviceStatus.Count; index++)
			{
				if (deviceStatus[index].deviceId.Equals(deviceId))
				{
					deviceStatus[index] = (deviceId, acquired);
					found = true;
					break;
				}
			}

			if (!found)
			{
				deviceStatus.Add((deviceId, acquired));
			}

			DeviceAcquired?.Invoke(deviceType, deviceId, acquired);
		}

		private bool IsAcquired(string deviceId)
		{
			var foundStatus = deviceStatus.FirstOrDefault(d => d.deviceId.Equals(deviceId));
			return foundStatus.acquired;
		}

		#endregion
	}
}