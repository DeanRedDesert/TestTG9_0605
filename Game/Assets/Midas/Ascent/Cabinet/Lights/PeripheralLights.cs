using IGT.Game.Core.Communication.Cabinet.CSI.Schemas;
using IGT.Game.Core.Presentation.CabinetServices;
using IGT.Game.Core.Presentation.PeripheralLights;
using IGT.Game.Core.Presentation.PeripheralLights.Devices;

namespace Midas.Ascent.Cabinet.Lights
{
	internal sealed class PeripheralLights : CabinetDevice
	{
		private IPeripheralLightService peripheralLightService;
		private UsbFacadeLight facade;
		private UsbButtonEdgeLight buttonEdgeLight;
		private UsbCrystalCoreLight lightRing;

		public PeripheralLights() : base(DeviceType.Light)
		{
		}

		public override void Init()
		{
			base.Init();
			peripheralLightService = CabinetServiceLocator.Instance.GetService<IPeripheralLightService>();
		}

		public override void Pause()
		{
			Log.Instance.Info("Pause");
			lightRing = null;
			facade = null;
			buttonEdgeLight = null;
			base.Pause();
		}

		public override void Resume()
		{
			Log.Instance.Info("Resume");
			base.Resume();
			lightRing = peripheralLightService.GetPeripheralLight(CrystalCoreLightHardware.DppLightRing);
			facade = peripheralLightService.GetPeripheralLight(FacadeHardware.Facade);
			buttonEdgeLight = peripheralLightService.GetPeripheralLight(ButtonHardware.ButtonEdge);
		}

		public override void DeInit()
		{
			peripheralLightService = null;
			base.DeInit();
		}

		public void SetColor(Color color)
		{
			lightRing.SetColor(color);
			facade.SetColor(UsbLightBase.AllGroups, color);
			buttonEdgeLight.SetColor(UsbButtonEdgeLight.Button.All, color);
		}
	}
}