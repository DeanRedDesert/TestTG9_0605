using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Midas.Gaff.ResultEditor
{
	[CreateAssetMenu(menuName = "Midas/Tools/Result Ed. Settings")]
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public sealed class DecisionSettings : ScriptableObject
	{
		public int ItemDecisionHeight;
		public int LastItemExtraSpace;
		public int MultiItemDecisionHeight;
		public int BoolDecisionHeight;
		public int BaseDecisionWidth;
		public int MinimumItemDecisionWidth;
		public int ItemDecisionHCharacterWidth;

		public GameObject BoolDecisionCallPrefab;
		public GameObject SingleItemDecisionCallPrefab;
		public GameObject MultiItemsDecisionCallPrefab;
		public GameObject DecisionPrefab;
		public GameObject UnsupportedTypePrefab;
	}
}