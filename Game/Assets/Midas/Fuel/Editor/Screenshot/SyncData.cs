namespace Midas.Fuel.Editor.Screenshot
{
	/// <summary>
	/// This class is passed down into coroutine fuel functions and the
	/// unity gui is updated by reading from it.
	/// </summary>
	public sealed class SyncData
	{
		/// <summary>
		/// What is the current status of the Process this is passed into
		/// </summary>
		public string Status { get; set; }

		/// <summary>
		/// What is the progress numerically
		/// </summary>
		public int Progress { get; set; }

		/// <summary>
		/// Flag for whether screenshots are currently being run.  This helps manage the state of Unity to produce reliable screenshots.
		/// </summary>
		public bool RunningScreenshots { get; set; }
	}
}