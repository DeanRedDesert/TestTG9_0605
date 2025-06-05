using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core.ExtensionMethods;
using UnityEditor;

namespace Midas.Presentation.Editor.GameData
{
	public abstract class GameDataFactory
	{
		#region Static Fields

		private static GameDataFactory handler;

		private static string[] stageNames;
		private static Dictionary<string, IReadOnlyList<ReelInformation>> reelInfo;
		private static Dictionary<string, IReadOnlyList<string>> symbols;
		private static Dictionary<string, IReadOnlyList<LinePatternInformation>> linePatternInfo;

		#endregion

		#region Static Implementation

		private static GameDataFactory Handler
		{
			get
			{
				if (handler == null)
				{
					var assemblies = AppDomain.CurrentDomain.GetAssemblies();
					foreach (var a in assemblies)
					{
						var gdfType = a.GetTypes().SingleOrDefault(t => typeof(GameDataFactory).IsAssignableFrom(t) && !t.IsAbstract);
						if (gdfType != null)
						{
							handler = (GameDataFactory)Activator.CreateInstance(gdfType);
							break;
						}
					}
				}

				return handler;
			}
		}

		public static IReadOnlyList<string> GetAllStageNames()
		{
			return stageNames ??= Handler.GetStageNames().ToArray();
		}

		public static IReadOnlyList<ReelInformation> GetAllReelInformation(string stageName)
		{
			if (reelInfo == null)
				reelInfo = GetAllStageNames().ToDictionary(sn => sn, sn => Handler.GetReelInformation(sn) ?? Array.Empty<ReelInformation>());

			return reelInfo.TryGetValue(stageName, out var result)
				? result
				: Array.Empty<ReelInformation>();
		}

		public static IReadOnlyList<string> GetAllSymbols(string stageName)
		{
			if (symbols == null)
				symbols = GetAllStageNames().ToDictionary(sn => sn, sn => Handler.GetSymbols(sn) ?? Array.Empty<string>());

			return symbols.TryGetValue(stageName, out var result)
				? result
				: Array.Empty<string>();
		}

		public static IEnumerable<LinePatternInformation> GetLinePattens(string stageName)
		{
			if (linePatternInfo == null)
				linePatternInfo = GetAllStageNames().ToDictionary(sn => sn, sn => Handler.GetLinePatterns(sn) ?? Array.Empty<LinePatternInformation>());

			return linePatternInfo.TryGetValue(stageName, out var result)
				? result
				: Array.Empty<LinePatternInformation>();
		}

		#endregion

		protected abstract IReadOnlyList<string> GetStageNames();
		protected abstract IReadOnlyList<ReelInformation> GetReelInformation(string stageName);
		protected abstract IReadOnlyList<string> GetSymbols(string stageName);
		protected abstract IReadOnlyList<LinePatternInformation> GetLinePatterns(string stageName);

		public static string ShowStageSelector(string selectedStage)
		{
			var sn = (string[])GetAllStageNames();
			var stageIndex = sn.FindIndex(selectedStage);

			if (stageIndex == -1)
				stageIndex = 0;

			stageIndex = EditorGUILayout.Popup("Stage", stageIndex, sn);

			if (stageIndex >= 0 && stageIndex < sn.Length)
				return sn[stageIndex];

			return null;
		}

		public static ReelInformation ShowReelsSelector(string selectedStage, ReelInformation selectedReels)
		{
			var reels = GetAllReelInformation(selectedStage);
			var reelsNames = reels.Select(r => r.Name).ToArray();
			var reelsIndex = reels.FindIndex(selectedReels);

			if (reelsIndex == -1)
				reelsIndex = 0;

			reelsIndex = EditorGUILayout.Popup("Reels", reelsIndex, reelsNames);

			if (reelsIndex >= 0 && reelsIndex < reels.Count)
				return reels[reelsIndex];

			return null;
		}
	}
}