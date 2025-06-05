using IGT.Game.Core.Communication.Cabinet.CSI.Schemas;
using IGT.Game.Core.Presentation.CabinetServices;
using IGT.Game.Core.Presentation.PeripheralLights.Devices;

namespace Midas.Ascent.Cabinet
{
	internal sealed class CabinetServicesController : ICabinetController
	{
		private PeripheralLightService peripheralLightService;

		public void Init()
		{
			peripheralLightService = new PeripheralLightService(AscentFoundation.GameLib.GameMountPoint, Priority.Game);
			if (CabinetServiceLocator.Instance is ICabinetServiceLocatorRestricted instance)
				instance.AddService<IPeripheralLightService>(peripheralLightService);
			else
				Log.Instance.Fatal("Unable to get ICabinetServiceLocatorRestricted");
		}

		public void OnBeforeLoadGame()
		{
		}

		public void Resume()
		{
			peripheralLightService.AsyncConnect(AscentCabinet.CabinetLib);
			peripheralLightService.PostConnect();
		}

		public void Pause()
		{
			peripheralLightService.Disconnect();
		}

		public void OnAfterUnLoadGame()
		{
		}

		public void DeInit()
		{
			var instance = (ICabinetServiceLocatorRestricted)CabinetServiceLocator.Instance;
			instance.RemoveService<IPeripheralLightService>();
			peripheralLightService = null;
		}
	}
}