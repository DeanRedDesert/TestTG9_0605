using Midas.Core;
using Midas.Core.General;
using Midas.Presentation.Data;
using Midas.Presentation.Game;

namespace Game.GameIdentity.Common
{
	public abstract class GameIdentityConfig : IPresentationController
	{
		private readonly AutoUnregisterHelper autoUnregisterHelper = new AutoUnregisterHelper();
		private readonly GameIdentityType gameIdentityType;

		protected GameIdentityConfig(GameIdentityType gameIdentityType)
		{
			this.gameIdentityType = gameIdentityType;
		}

		public void Init()
		{
			if (gameIdentityType != StatusDatabase.ConfigurationStatus.GameIdentity)
				return;

			GiInit();
			RegisterForEvents(autoUnregisterHelper);
		}

		public void DeInit()
		{
			if (gameIdentityType != StatusDatabase.ConfigurationStatus.GameIdentity)
				return;

			autoUnregisterHelper.UnRegisterAll();
			GiDeInit();
		}

		public void Destroy()
		{
			GiDestroy();
		}

		protected virtual void GiInit() { }
		protected virtual void GiDeInit() { }
		protected virtual void GiDestroy() { }
		protected virtual void RegisterForEvents(AutoUnregisterHelper autoUnregister) { }
	}
}