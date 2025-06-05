using System;
using System.Collections.Generic;
using Midas.Presentation.General;
using UnityEngine;

namespace Game.GameIdentity.Common
{
	#region Editor

#if UNITY_EDITOR

	using UnityEditor;
	using Midas.Presentation.Editor.General;

	[CustomPropertyDrawer(typeof(MultiwayMappingDictionary), true)]
	public class MultiwayMappingDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer
	{
	}

#endif

	#endregion

	[Serializable]
	public sealed class MultiwayMappingDictionary : SerializableDictionary<long, long>
	{
	}

	[CreateAssetMenu(menuName = "Midas/Multiway Ways Mapping", order = 100)]
	public sealed class MultiwayWaysMapping : ScriptableObject
	{
#pragma warning disable 649
		[SerializeField]
		private MultiwayMappingDictionary multiwayMapping;
#pragma warning restore 649

		public IReadOnlyDictionary<long, long> GetMultiwayMapping()
		{
			return multiwayMapping ?? new Dictionary<long, long>();
		}
	}
}