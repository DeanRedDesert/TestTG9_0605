namespace Midas.Tools.Editor.ToolbarExt
{
	public interface IToolbarExtension
	{
		bool IsDirty { get; }
		void OnGui();
	}
}