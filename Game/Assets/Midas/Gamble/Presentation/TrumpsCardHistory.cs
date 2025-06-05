using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Gamble.LogicToPresentation;
using UnityEngine;

namespace Midas.Gamble.Presentation
{
	[RequireComponent(typeof(RectTransform))]
	public sealed class TrumpsCardHistory : MonoBehaviour
	{
		private readonly List<SpriteRenderer> cards = new List<SpriteRenderer>();
		private bool initialise;
		private Sprite[] lookup;
		private const int MaxHistoryObjectDisplay = 10;

		[SerializeField]
		private GameObject prefab;

		[SerializeField]
		private CardFace[] cardFaces;

		[SerializeField]
		private float cardSpacing = 0.01f;

		private void Awake()
		{
			Initialise();
		}

		private void OnDestroy()
		{
			foreach (var card in cards)
				Destroy(card.gameObject);

			cards.Clear();
		}

		public void UpdateHistory(IReadOnlyList<TrumpsSuit> cardHistory, IReadOnlyList<TrumpsCycleData> currentResults)
		{
			Initialise();
			var reversedHistory = cardHistory.Concat(currentResults.Select(r => r.Suit)).Reverse().ToArray();

			for (var i = 0; i < cards.Count; i++)
			{
				if (i < reversedHistory.Length)
				{
					cards[i].sprite = lookup[(int)reversedHistory[i]];
					cards[i].gameObject.SetActive(true);
				}
				else
				{
					cards[i].gameObject.SetActive(false);
				}
			}
		}

		private void Initialise()
		{
			if (initialise)
				return;

			initialise = true;
			lookup = new Sprite[Enum.GetValues(typeof(TrumpsSuit)).Length];

			foreach (var cf in cardFaces)
				lookup[(int)cf.suit] = cf.sprite;

			var rect = GetComponent<RectTransform>().rect;
			var spacing = cardFaces[0].sprite.bounds.size.x + cardSpacing;

			var pos = rect.center;
			pos.x -= spacing * (MaxHistoryObjectDisplay - 1) / 2.0f;

			for (var i = 0; i < MaxHistoryObjectDisplay; i++)
			{
				var go = Instantiate(prefab, gameObject.transform, false);
				go.transform.localPosition = new Vector3(pos.x, pos.y, 0);
				pos.x += spacing;
				go.SetActive(false);
				cards.Add(go.GetComponent<SpriteRenderer>());
			}
		}
	}
}