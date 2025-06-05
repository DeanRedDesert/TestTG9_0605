using System;
using System.Linq;
using System.Reflection;
using Midas.Core;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Midas.Tools.Editor
{
	public static class InitEditor
	{
		#region Public

		[InitializeOnLoadMethod, DidReloadScripts]
		public static void Init()
		{
			RegisterPlayModeChanged();

			if (!EditorApplication.isPlayingOrWillChangePlaymode)
			{
				InitRegisteredTypes();
			}
		}

		#endregion

		#region Private

		static InitEditor() { }

		private static void InitRegisteredTypes()
		{
			if (_initItems == null)
			{
				_initItems = ReflectionUtil.GetAllTypes()
					.Where(type => type.GetCustomAttributes(true).Any(a => a is InitForEditorAttribute))
					.Select(type => (type, type.GetCustomAttribute<InitForEditorAttribute>()))
					.OrderBy(ta => ta.Item2.Priority)
					.Select(ta => (
						(Action)ta.type.GetMethod("Init")?.CreateDelegate(typeof(Action)),
						(Action)ta.type.GetMethod("DeInit")?.CreateDelegate(typeof(Action))
					))
					.ToArray();

				// call all inits
				foreach (var valueTuple in _initItems)
				{
					valueTuple.Init?.Invoke();
				}
			}
		}

		private static void DeInitRegisteredTypes()
		{
			if (_initItems != null)
			{
				for (var i = _initItems.Length - 1; i >= 0; i--)
				{
					var valueTuple = _initItems[i];
					valueTuple.DeInit?.Invoke();
				}

				_initItems = null;
			}
		}

		private static void RegisterPlayModeChanged()
		{
			if (!_playModeChangedRegistered)
			{
				EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
				_playModeChangedRegistered = true;
			}
		}

		private static void UnRegisterPlayModeChanged()
		{
			if (_playModeChangedRegistered)
			{
				EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
				_playModeChangedRegistered = false;
			}
		}

		private static void OnPlayModeStateChanged(PlayModeStateChange mode)
		{
			if (mode == PlayModeStateChange.ExitingEditMode)
			{
				DeInitRegisteredTypes();
			}
			else if (mode == PlayModeStateChange.EnteredEditMode)
			{
				InitRegisteredTypes();
			}
		}

		private sealed class Destructor
		{
			#region Public

			~Destructor()
			{
				DeInitRegisteredTypes();
				UnRegisterPlayModeChanged();
			}

			#endregion
		}

		private static bool _playModeChangedRegistered;
		private static (Action Init, Action DeInit)[] _initItems;

		// ReSharper disable once UnusedMember.Local
		private static readonly Destructor _finalize = new Destructor();

		#endregion
	}
}