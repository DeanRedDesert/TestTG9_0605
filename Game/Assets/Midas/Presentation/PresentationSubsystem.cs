using Midas.Core;
using Midas.Core.StateMachine;
using Midas.Presentation.Audio;
using Midas.Presentation.ButtonHandling;

namespace Midas.Presentation
{
	public sealed class PresentationSubsystem : IGamingSubsystem
	{
		public string Name => "Presentation";

		public void Init()
		{
			ButtonManager.Init();
			FrameUpdateService.Init();
			StateMachineService.Init();
		}

		public void OnStart()
		{
		}

		public void OnBeforeLoadGame()
		{
			FrameUpdateService.PreUpdate.EnableRegistrationCheck();
			FrameUpdateService.Update.EnableRegistrationCheck();
			FrameUpdateService.PostUpdate.EnableRegistrationCheck();
			FrameUpdateService.LateUpdate.EnableRegistrationCheck();

			AudioService.Init();
		}

		public void OnAfterUnloadGame()
		{
			AudioService.DeInit();

			FrameUpdateService.PreUpdate?.DoRegistrationCheck();
			FrameUpdateService.Update?.DoRegistrationCheck();
			FrameUpdateService.PostUpdate?.DoRegistrationCheck();
			FrameUpdateService.LateUpdate?.DoRegistrationCheck();
		}

		public void OnStop()
		{
		}

		public void DeInit()
		{
			StateMachineService.DeInit();
			FrameUpdateService.DeInit();
			ButtonManager.DeInit();
		}
	}
}