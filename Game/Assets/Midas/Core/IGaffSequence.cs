namespace Midas.Core
{
	public enum GaffType
	{
		Show,
		Development,
		History
	}

	public interface IGaffSequence
	{
		string Name { get; }
		GaffType GaffType { get; }
	}
}