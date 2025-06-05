using Midas.Core.General;

namespace Midas.Presentation.Sequencing
{
	public static class AutoUnregisterHelperExtensions
	{
		/// <summary>
		/// Registers for any property changes on a status block.
		/// </summary>
		private sealed class RegisterClassSequenceEventHandler : IRegisterClass
		{
			private readonly Sequence sequence;
			private readonly SequenceEventHandler eventHandler;
			private readonly string eventName;
			private int handle;

			public RegisterClassSequenceEventHandler(Sequence sequence, SequenceEventHandler eventHandler, string eventName)
			{
				this.sequence = sequence;
				this.eventHandler = eventHandler;
				this.eventName = eventName;
			}

			public void Register() => handle = sequence.RegisterEventHandler(eventName, eventHandler);
			public void UnRegister() => sequence.UnRegisterEventHandler(eventName, handle);
		}

		public static void RegisterSequenceEventHandler(this AutoUnregisterHelper autoUnregisterHelper, Sequence sequence, string eventName, SequenceEventHandler eventHandler)
		{
			autoUnregisterHelper.Register(new RegisterClassSequenceEventHandler(sequence, eventHandler, eventName));
		}
	}
}