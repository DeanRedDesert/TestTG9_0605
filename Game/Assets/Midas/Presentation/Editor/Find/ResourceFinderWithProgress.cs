using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Midas.Presentation.Editor.Find
{
	public sealed class ResourceFinderWithProgress<TResourceType, TFoundDataType> where TResourceType : Object
	{
		private TResourceType[] allObjects;
		private int progressTime;

		public IList<TFoundDataType> Find(Func<TResourceType, IEnumerable<TFoundDataType>> finder)
		{
			var refs = new List<TFoundDataType>();
			allObjects = Resources.FindObjectsOfTypeAll<TResourceType>();
			progressTime = Environment.TickCount;
			var i = 0;
			foreach (var o in allObjects)
			{
				UpdateProgress(i++);
				refs.AddRange(finder(o));
			};

			EditorUtility.ClearProgressBar();

			return refs;
		}

		private void UpdateProgress(int idx)
		{
			if (Environment.TickCount - progressTime > 250)
			{
				progressTime = Environment.TickCount;
				EditorUtility.DisplayProgressBar($"Searching {typeof(TResourceType).Name}", $"Processing {idx} of {allObjects.Length}",
					idx / (float)allObjects.Length);
			}
		}
	}
}