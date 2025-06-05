using System;
using System.Diagnostics;
using System.Linq;

namespace Midas.Presentation
{
	public sealed class FrameUpdateService
	{
		public static FrameUpdateService PreUpdate { get; private set; }

		public static FrameUpdateService Update { get; private set; }

		public static FrameUpdateService PostUpdate { get; private set; }

		public static FrameUpdateService LateUpdate { get; private set; }

		public event Action OnFrameUpdate;

		public event Action OnNextFrameUpdate;

		public static void Init()
		{
			PreUpdate = new FrameUpdateService();
			Update = new FrameUpdateService();
			PostUpdate = new FrameUpdateService();
			LateUpdate = new FrameUpdateService();
		}

		public static void DeInit()
		{
			PreUpdate = null;
			Update = null;
			PostUpdate = null;
			LateUpdate = null;
		}

		public void DoUpdate()
		{
			OnFrameUpdate?.Invoke();

			// execute single frame frame updates

			var action = OnNextFrameUpdate;
			OnNextFrameUpdate = null;
			action?.Invoke();
		}

		#region Registration Checking

		private Action finalRegistrationCheck;

		[Conditional("DEBUG")]
		public void EnableRegistrationCheck()
		{
			var preLoadInvocationList = OnFrameUpdate?.GetInvocationList() ?? Array.Empty<Delegate>();

			void DoCheck()
			{
				var postUnloadInvocationList = OnFrameUpdate?.GetInvocationList() ?? Array.Empty<Delegate>();

				foreach (var action in postUnloadInvocationList.Except(preLoadInvocationList))
					Log.Instance.Fatal($"Frame Update handler was not unregistered '{action.Target?.GetType().FullName}.{action.Method.Name}' action.Target='{action.Target}'");
				foreach (var action in preLoadInvocationList.Except(postUnloadInvocationList))
					Log.Instance.Fatal($"Frame Update handler was unregistered when it shouldn't be '{action.Target?.GetType().FullName}.{action.Method.Name}' action.Target='{action.Target}'");
			}

			finalRegistrationCheck = DoCheck;
		}

		[Conditional("DEBUG")]
		public void DoRegistrationCheck()
		{
			finalRegistrationCheck?.Invoke();
			finalRegistrationCheck = null;
		}

		#endregion
	}
}