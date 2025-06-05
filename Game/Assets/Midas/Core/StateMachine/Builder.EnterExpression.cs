namespace Midas.Core.StateMachine
{
	public sealed partial class Builder
	{
		public sealed class EnterExpression : InExpression
		{
			public EnterExpression(StateEnterDelegate action, Builder builder, State[] states) : base(builder, states)
			{
				foreach (var state in states)
				{
					state.OnEnter += action;
				}
			}
		}
	}
}