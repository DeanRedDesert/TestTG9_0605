using System.Collections.Generic;
using Midas.Core;
using Midas.Core.Coroutine;

namespace Midas.Presentation
{
	public static class FrameUpdateServiceExtensionMethods
	{
		private sealed class FrameUpdateCoroutine : Coroutine
		{
			private readonly FrameUpdateService parentFrameUpdate;
			private bool removed;

			public FrameUpdateCoroutine(FrameUpdateService parentFrameUpdate, IEnumerator<CoroutineInstruction> enumerator) : base(enumerator)
			{
				this.parentFrameUpdate = parentFrameUpdate;
				parentFrameUpdate.OnFrameUpdate += DoStep;
			}

			protected override void RemoveCoroutine()
			{
				parentFrameUpdate.OnFrameUpdate -= DoStep;
				removed = true;
			}

			private void DoStep()
			{
				if (!removed)
					DoStep(FrameTime.DeltaTime);
			}
		}

		/// <summary>
		/// Init a coroutine as a chile of a frame update. This kind of coroutine will get one step per frame in one of the
		/// four possible frame update stages (BeforeFrameUpdate, FrameUpdate, AfterFrameUpdate or
		/// FrameLateUpdate)
		/// </summary>
		/// <param name="parentFrameUpdate">The parent frame update service.</param>
		/// <param name="routine">The routine to run.</param>
		/// <returns>The new coroutine.</returns>
		public static Coroutine StartCoroutine(this FrameUpdateService parentFrameUpdate, IEnumerator<CoroutineInstruction> routine)
		{
			return new FrameUpdateCoroutine(parentFrameUpdate, routine);
		}
	}
}