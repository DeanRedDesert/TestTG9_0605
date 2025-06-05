namespace Midas.Core.StateMachine
{
	public sealed partial class Builder
	{
		public sealed class ExitExpression : InExpression
		{
			public ExitExpression(StateExitDelegate action, Builder builder, State[] states) : base(builder, states)
			{
				foreach (var state in states)
				{
					state.OnExit += action;
				}
			}
		}
	}
}