using System.Collections.Generic;
using UnityEngine;

namespace Midas.Presentation.Paylines
{
	/// <summary>
	/// Asset for paylines. Contains all the meshes and position information for the lines and win boxes.
	/// </summary>
	[CreateAssetMenu(menuName = "Midas/Paylines/Standard Paylines")]
	public class StandardPaylines : ScriptableObject
	{
		/// <summary>
		/// The settings for the paylines. These are in a different asset so that the game programmer doesn't need to remember what the defaults are.
		/// </summary>
		public StandardPaylinesSettings Settings;

		/// <summary>
		/// The name of the processor in the stage model that contains the patterns that are used to generate these paylines.
		/// </summary>
		public string PatternFieldName = "";

		/// <summary>
		/// The generated paylines.
		/// </summary>
		public List<PaylineData> Paylines = new List<PaylineData>();

		/// <summary>
		/// The generated win boxes.
		/// </summary>
		public List<WinBoxData> WinBoxes = new List<WinBoxData>();
	}
}