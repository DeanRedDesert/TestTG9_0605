using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Midas.Utility.ResultPicker
{
	[CreateAssetMenu(menuName = "Midas/Utility Symbol Windows", order = 100)]
	public sealed class UtilitySymbolWindows : ScriptableObject
	{
		[SerializeField]
		private List<UtilitySymbolWindow> symbolWindows;

		public IReadOnlyList<UtilitySymbolWindow> GetSymbolWindows() => symbolWindows;

#if UNITY_EDITOR
		public void ConfigureForMakeGame(IReadOnlyList<UtilitySymbolWindow> newSymbolWindows)
		{
			symbolWindows = newSymbolWindows.ToList();
		}
#endif
	}

	[Serializable]
	public sealed class UtilitySymbolWindow
	{
		[SerializeField]
		public string stageName;

		[SerializeField]
		public string decisionName;

		[SerializeField]
		public string resultName;

		public string StageName { get { return stageName; } }
		public string DecisionName { get { return decisionName; } }
		public string ResultName { get { return resultName; } }

		public UtilitySymbolWindow(string stageName, string decisionName, string resultName)
		{
			this.stageName = stageName;
			this.decisionName = decisionName;
			this.resultName = resultName;
		}
	}
}