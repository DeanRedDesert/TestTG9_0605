namespace Midas.Core.StateMachine
{
	public sealed partial class Builder
	{
		public sealed class ExecExpression : InExpression
		{
			public ExecExpression(StateExecuteDelegate action, Builder builder, State[] states) : base(builder, states)
			{
				foreach (var state in states)
				{
					state.OnExecute += action;
				}
			}
		}
	}
}