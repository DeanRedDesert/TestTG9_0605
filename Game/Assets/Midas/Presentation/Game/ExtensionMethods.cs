using System.Collections.Generic;

namespace Midas.Presentation.Game
{
	public static class ExtensionMethods
	{
		public static void Show(this IReadOnlyList<IPresentationNode> nodes)
		{
			foreach (var node in nodes)
			{
				node.Show();
			}
		}

		public static bool AreAllComplete(this IReadOnlyList<IPresentationNode> nodes)
		{
			foreach (var node in nodes)
				if (!node.IsMainActionComplete)
					return false;

			return true;
		}

		public static void ShowHistory(this IReadOnlyList<IHistoryPresentationNode> nodes)
		{
			foreach (var node in nodes)
				node.ShowHistory();
		}

		public static void HideHistory(this IReadOnlyList<IHistoryPresentationNode> nodes)
		{
			foreach (var node in nodes)
				node.HideHistory();
		}
	}
}