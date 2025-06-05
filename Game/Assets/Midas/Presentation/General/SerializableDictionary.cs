using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Midas.Presentation.General
{
	[Serializable]
	public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
	{
		[FormerlySerializedAs("_keys")]
		[SerializeField]
		private TKey[] keys;

		[FormerlySerializedAs("_values")]
		[SerializeField]
		private TValue[] values;

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (keys != null && values != null && keys.Length == values.Length)
			{
				Clear();
				var n = keys.Length;
				for (var i = 0; i < n; ++i)
				{
					this[keys[i]] = values[i];
				}

				keys = null;
				values = null;
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			var n = Count;
			keys = new TKey[n];
			values = new TValue[n];

			var i = 0;
			foreach (var kvp in this)
			{
				keys[i] = kvp.Key;
				values[i] = kvp.Value;
				++i;
			}
		}
	}
}