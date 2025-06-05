using System;
using System.Threading.Tasks;

namespace Midas.Presentation.Sequencing
{
	public sealed class SequenceEventHandler
	{
		public object Owner { get; }
		public Action<SequenceEventArgs, TaskCompletionSource<int>> OnStateEnter { get; }
		public Action<SequenceEventArgs> OnStateExecute { get; }
		public Action<SequenceEventArgs> OnStateExit { get; }
		public Func<bool> Enabled { get; }
		public Action<SequenceEventArgs, bool> OnPause { get; }

		public SequenceEventHandler(object owner,
			Action<SequenceEventArgs, TaskCompletionSource<int>> onEnter,
			Action<SequenceEventArgs> onExecute = null,
			Action<SequenceEventArgs> onExit = null,
			Func<bool> enabled = null,
			Action<SequenceEventArgs, bool> onPause = null)
		{
			Owner = owner;
			OnStateEnter = onEnter;
			OnStateExecute = onExecute;
			OnStateExit = onExit;
			Enabled = enabled;
			OnPause = onPause;
		}
	}
}