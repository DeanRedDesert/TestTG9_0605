namespace Midas.Presentation.StageHandling
{
	/// <summary>
	///     Stage ids used for handling different "scenes".
	/// </summary>
	[Stages("Midas")]
	public static class Stages
	{
		#region Public

		public static Stage Undefined { get; } = Stage.Undefined;
		public static Stage Gamble { get; } = new Stage(1);
		public const int GameSpecificStartId = 10000;

		#endregion
	}
}