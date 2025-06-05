namespace Midas.Gaff.ResultEditor
{
	public interface IEditableDecision
	{
		object UIState { get; }
		bool Changed { get; }
		void ResetChangedState();
	}
}