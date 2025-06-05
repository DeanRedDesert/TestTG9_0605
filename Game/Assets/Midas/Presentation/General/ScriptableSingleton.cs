using System;
using System.IO;
using UnityEngine;

namespace Midas.Presentation.General
{
	public class ScriptableSingleton<T> : ScriptableObject where T : ScriptableSingleton<T>
	{
		private static T instance;
		private static Action<T, string> creator;

		public static Action<T, string> Creator
		{
			set => creator = value;
		}

		public static T Instance
		{
			get
			{
				if (instance == null)
				{
					var insts = Resources.LoadAll<T>(typeof(T).Name);
					if (insts == null || insts.Length == 0)
					{
						Log.Instance.Info($"Please consider moving your {typeof(T).Name} file into a Resources/ folder");
						insts = Resources.LoadAll<T>("");
					}

					if (insts == null || insts.Length == 0)
					{
						var asset = CreateInstance<T>();
						var parentDir = "Assets/Resources/";
						if (!Directory.Exists(parentDir))
						{
							Directory.CreateDirectory(parentDir);
						}

						var path = parentDir + typeof(T).Name + ".asset";
						Log.Instance.Info($"No resource files of type {typeof(T).Name} found. Creating a new one at {path}");
						creator?.Invoke(asset, path);
						instance = asset;
					}
					else if (insts.Length > 1)
					{
						Log.Instance.Error($"Found {insts.Length} resource files of type {typeof(T).Name}. Only 1 is permitted.");
						instance = insts[0];
					}
					else
					{
						Log.Instance.Info($"Using resource file of type {typeof(T).Name} at {insts[0].name}");
						instance = insts[0];
					}
				}

				return instance;
			}
		}
	}
}