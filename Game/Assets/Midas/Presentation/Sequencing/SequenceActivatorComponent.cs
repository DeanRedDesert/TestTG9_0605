namespace Midas.Presentation.Sequencing
{
	public interface ISequenceActivatorComponent
	{
		public bool CanActivate(object o);
		public void StartComponent(SequenceComponent p, SequenceEventArgs eventArgs);
		public void StopComponent(SequenceComponent p, SequenceEventArgs eventArgs, bool reset);
		public bool TimeOutComponent(SequenceComponent p, SequenceEventArgs eventArgs, bool wcResetTimeout, bool finished);
	}

	public abstract class SequenceActivatorComponent<T> : ISequenceActivatorComponent where T : class
	{
		public bool CanActivate(object o) => o is T;
		public abstract void StartComponent(SequenceComponent p, SequenceEventArgs eventArgs);
		public abstract void StopComponent(SequenceComponent p, SequenceEventArgs eventArgs, bool reset);
		public abstract bool TimeOutComponent(SequenceComponent p, SequenceEventArgs eventArgs, bool wcResetTimeout, bool finished);
		protected T Unwrap(SequenceComponent p) => (T)(object)p.Comp;
	}
}