using System.Collections.Generic;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Reels;
using UnityEngine;

namespace Midas.Presentation.Paylines
{
	/// <summary>
	/// Note: This payline container only supports rectangular reel shapes.
	/// </summary>
	public sealed class BeanPaylineContainer : PaylineContainer
	{
		private readonly List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
		private int? currentLineNumber;

		[SerializeField]
		private ReelContainer reelContainer;

		[SerializeField]
		private List<Sprite> sprites;

		[SerializeField]
		[Tooltip("To disable the line indicator, delete the lineIndicator game object from the scene")]
		private GameObject lineIndicator;

		public override void HideWins()
		{
			currentLineNumber = null;
			foreach (var spriteRenderer in spriteRenderers)
			{
				spriteRenderer.gameObject.SetActive(false);
			}

			if (lineIndicator)
				lineIndicator.SetActive(false);
		}

		public override void HighlightWin(IWinInfo win)
		{
			if (!win.IsLineWin())
			{
				HideWins();
				return;
			}

			if (currentLineNumber != win.LineNumber!.Value)
			{
				for (var i = 0; i < win.LinePattern.Count; i++)
				{
					Vector3 spritePosition;
					int delta;
					Sprite sprite;

					var currentCell = win.LinePattern[i];
					var currentLocalPosition = reelContainer.GetSymbolLocationByCell(currentCell.Row, currentCell.Column);

					if (i == 0)
					{
						if (!lineIndicator)
							continue;

						var nextCell = win.LinePattern[i + 1];
						var nextCellLocalPosition = reelContainer.GetSymbolLocationByCell(nextCell.Row, nextCell.Column);

						var xOffset = (nextCellLocalPosition.x - currentLocalPosition.x) / 2;

						spritePosition = new Vector3(currentLocalPosition.x - xOffset, currentLocalPosition.y, currentLocalPosition.z);
						delta = 0;
						sprite = sprites[0];

						var spriteWidth = sprite.textureRect.width / sprite.pixelsPerUnit;

						lineIndicator.transform.localPosition = new Vector3(spritePosition.x - spriteWidth / 2, spritePosition.y, spritePosition.y);
					}
					else
					{
						var previousCell = win.LinePattern[i - 1];
						var previousLocalPosition = reelContainer.GetSymbolLocationByCell(previousCell.Row, previousCell.Column);

						spritePosition = (previousLocalPosition + currentLocalPosition) / 2;
						delta = currentCell.Row - previousCell.Row;
						sprite = sprites[Mathf.Abs(delta)];
					}

					SpriteRenderer spriteRenderer = null;

					while (spriteRenderers.Count <= i)
					{
						var go = new GameObject("Sprite")
						{
							layer = gameObject.layer,
							hideFlags = HideFlags.DontSave
						};

						go.transform.SetParent(transform, false);

						spriteRenderer = go.AddComponent<SpriteRenderer>();

						spriteRenderers.Add(spriteRenderer);
					}

					spriteRenderer = spriteRenderers[i];
					spriteRenderer.transform.localPosition = spritePosition;
					spriteRenderer.sprite = sprite;
					spriteRenderer.flipX = delta > 0;
				}

				currentLineNumber = win.LineNumber.Value;
			}

			for (var i = 0; i < spriteRenderers.Count; i++)
			{
				var spriteRenderer = spriteRenderers[i];
				spriteRenderer.gameObject.SetActive(i > 0 || lineIndicator);
			}

			if (lineIndicator)
				lineIndicator.gameObject.SetActive(true);
		}

#if UNITY_EDITOR

		public void ConfigureForMakeGame(ReelContainer container)
		{
			reelContainer = container;
		}

#endif
	}
}