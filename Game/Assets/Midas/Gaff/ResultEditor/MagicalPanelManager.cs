using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Midas.Gaff.ResultEditor
{
	/// <summary>
	/// A script to control the searching and selection of an item from a list of sourceItems.
	/// </summary>
	public sealed class MagicalPanelManager : MonoBehaviour
	{
		#region Private Fields

		private static readonly List<KeyCode> relevantCharacters = new List<KeyCode>
		{
			KeyCode.A,
			KeyCode.B,
			KeyCode.C,
			KeyCode.D,
			KeyCode.E,
			KeyCode.F,
			KeyCode.G,
			KeyCode.H,
			KeyCode.I,
			KeyCode.J,
			KeyCode.K,
			KeyCode.L,
			KeyCode.M,
			KeyCode.N,
			KeyCode.O,
			KeyCode.P,
			KeyCode.Q,
			KeyCode.R,
			KeyCode.S,
			KeyCode.T,
			KeyCode.U,
			KeyCode.V,
			KeyCode.W,
			KeyCode.X,
			KeyCode.Y,
			KeyCode.Z,
			KeyCode.Alpha0,
			KeyCode.Alpha1,
			KeyCode.Alpha2,
			KeyCode.Alpha3,
			KeyCode.Alpha4,
			KeyCode.Alpha5,
			KeyCode.Alpha6,
			KeyCode.Alpha7,
			KeyCode.Alpha9,
			KeyCode.Alpha0
		};

		private ItemsDecisionCallView itemViewModel;
		private GameObject visibleChildren;
		private Slider indexSlider;
		private int framesToFade = -1;
		private readonly int maxFadingFrames = 30;
		private IReadOnlyList<string> sourceItems;

		#endregion

		#region Public Fields

		[SerializeField]
		private int visibleSymbols = 6;

		[SerializeField]
		private int cursorIndex = 2;

		[SerializeField]
		private GameObject itemPrefab;

		[SerializeField]
		private GameObject searchButtonPrefab;

		#endregion

		#region Unity Functions

		private void Update()
		{
			foreach (var letter in relevantCharacters)
			{
				if (Input.GetKeyUp(letter))
				{
					var searchString = letter.ToString();

					var nextIndex = SearchForNextMatch(searchString);

					if (nextIndex < 0)
						continue;

					indexSlider.value = nextIndex;

					if (framesToFade < 0)
						StartCoroutine(PlayHighlight());
					else
						framesToFade = maxFadingFrames;
				}
			}
		}

		#endregion

		#region Public Functions

		public void Initialize(ItemsDecisionCallView viewModel, int selectedItemIndex, IReadOnlyList<string> items)
		{
			itemViewModel = viewModel;
			sourceItems = items;

			var cancelButton = GetComponentsInChildren<Button>().Single(b => b.name == "CancelButton");
			var minus100 = GetComponentsInChildren<Button>().Single(b => b.name == "Minus100");
			var minus10 = GetComponentsInChildren<Button>().Single(b => b.name == "Minus10");
			var minus1 = GetComponentsInChildren<Button>().Single(b => b.name == "Minus1");
			var plus100 = GetComponentsInChildren<Button>().Single(b => b.name == "Plus100");
			var plus10 = GetComponentsInChildren<Button>().Single(b => b.name == "Plus10");
			var plus1 = GetComponentsInChildren<Button>().Single(b => b.name == "Plus1");
			var endIndex = GetComponentsInChildren<TextMeshProUGUI>().Single(b => b.name == "EndIndex");
			indexSlider = GetComponentInChildren<Slider>();

			if (sourceItems.Count < 300)
			{
				Destroy(plus100.gameObject);
				Destroy(minus100.gameObject);
			}
			else
			{
				minus100.onClick.AddListener(() => DecrementChangeHandler(indexSlider, 1, sourceItems.Count));
				plus100.onClick.AddListener(() => IncrementChangeHandler(indexSlider, 1, sourceItems.Count));
			}

			if (sourceItems.Count < 30)
			{
				Destroy(plus10.gameObject);
				Destroy(minus10.gameObject);
			}
			else
			{
				minus10.onClick.AddListener(() => DecrementChangeHandler(indexSlider, 1, sourceItems.Count));
				plus10.onClick.AddListener(() => IncrementChangeHandler(indexSlider, 1, sourceItems.Count));
			}

			int refreshIndex;

			if (sourceItems.Count <= visibleSymbols)
			{
				visibleSymbols = sourceItems.Count;
				cursorIndex = 0;
				refreshIndex = 0;
				Destroy(plus1.gameObject);
				Destroy(minus1.gameObject);

				var quickSearchChildren = transform.Find("QuickSearchButtons").gameObject;
				var quickSearchText = transform.Find("SymbolSearchText").gameObject;

				Destroy(quickSearchChildren);
				Destroy(quickSearchText);
				Destroy(indexSlider.gameObject);
				indexSlider = null;
			}
			else
			{
				refreshIndex = selectedItemIndex;

				var quickSearchChildren = transform.Find("QuickSearchButtons").gameObject;
				var quickSearchButtons = GenerateQuickSearchStrings(15);

				quickSearchButtons = quickSearchButtons.OrderBy(s => s).ToList();

				foreach (var searchString in quickSearchButtons)
				{
					var newItemContainer = Instantiate(searchButtonPrefab, quickSearchChildren.transform, false);
					newItemContainer.GetComponentInChildren<TextMeshProUGUI>().text = searchString;
					newItemContainer.GetComponent<Button>().onClick.AddListener(() => SearchButtonChangeHandler(searchString));
				}

				indexSlider.maxValue = sourceItems.Count - 1;
				indexSlider.minValue = 0;
				indexSlider.value = selectedItemIndex;
				indexSlider.onValueChanged.AddListener(v => { Refresh((int)v); });

				minus1.onClick.AddListener(() => DecrementChangeHandler(indexSlider, 1, sourceItems.Count));
				plus1.onClick.AddListener(() => IncrementChangeHandler(indexSlider, 1, sourceItems.Count));
			}

			visibleChildren = transform.Find("VisibleSymbols").gameObject;

			endIndex.text = "Index " + (sourceItems.Count - 1) + " =>";

			Refresh(refreshIndex);

			cancelButton.onClick.AddListener(() =>
			{
				transform.SetParent(null);
				Destroy(gameObject);
			});
		}

		#endregion

		#region Private Functions

		private IEnumerator PlayHighlight()
		{
			var chosenChild = visibleChildren.transform.GetChild(cursorIndex);
			var oldColour = chosenChild.GetComponent<Image>().color;
			framesToFade = maxFadingFrames;

			while (framesToFade >= 0)
			{
				chosenChild.GetComponent<Image>().color = Color.Lerp(Color.white, oldColour, 1 - framesToFade / (float)maxFadingFrames);
				framesToFade--;
				yield return new WaitForEndOfFrame();
			}

			chosenChild.GetComponent<Image>().color = oldColour;
		}

		private void SearchButtonChangeHandler(string searchString)
		{
			var nextIndex = SearchForNextMatch(searchString);

			if (nextIndex < 0)
				return;

			indexSlider.value = nextIndex;

			if (framesToFade < 0)
				StartCoroutine(PlayHighlight());
			else
				framesToFade = maxFadingFrames;
		}

		private List<string> GenerateQuickSearchStrings(int maxCount)
		{
			var set = new HashSet<string>();
			foreach (var item in sourceItems)
				set.Add(item);

			if (set.Count <= maxCount)
				return set.ToList();

			set = new HashSet<string>();

			foreach (var item in sourceItems)
				set.Add(item.Length <= 3 ? item : item.Substring(0, 3));

			if (set.Count <= maxCount)
				return set.ToList();

			foreach (var item in sourceItems)
				set.Add(item.Length <= 2 ? item : item.Substring(0, 2));

			if (set.Count <= maxCount)
				return set.ToList();

			foreach (var item in sourceItems)
				set.Add(item.Substring(0, 1));

			return set.ToList();
		}

		private int SearchForNextMatch(string searchString)
		{
			var newI = ((int)indexSlider.value + 1) % sourceItems.Count;

			for (var i = 0; i < sourceItems.Count; i++)
			{
				var symbol = sourceItems[(newI + i) % sourceItems.Count];

				if (symbol.ToUpper().StartsWith(searchString))
					return (newI + i) % sourceItems.Count;
			}

			return -1;
		}

		private void ConfirmButtonChangeHandler(Slider slider, int offset)
		{
			var sliderValue = slider == null ? 0 : (int)slider.value;
			var newI = (sourceItems.Count + sliderValue + offset - cursorIndex) % sourceItems.Count;
			itemViewModel.SetSelectedItemIndex(newI);
			transform.SetParent(null);
			Destroy(gameObject);
		}

		private static void DecrementChangeHandler(Slider slider, int offset, int stripLength)
		{
			var extra = (offset / stripLength + 1) * stripLength;
			var sliderValue = (int)slider.value;
			var newIndex = (extra + sliderValue - offset) % stripLength;

			slider.value = newIndex;
		}

		private static void IncrementChangeHandler(Slider slider, int offset, int stripLength)
		{
			var sliderValue = (int)slider.value;
			var newIndex = (sliderValue + offset) % stripLength;

			slider.value = newIndex;
		}

		private void Refresh(int index)
		{
			if (visibleChildren.transform.childCount != visibleSymbols)
			{
				foreach (Transform child in visibleChildren.transform)
					Destroy(child.gameObject);

				for (var i = 0; i < visibleSymbols; i++)
				{
					var localIndex = i;
					var newItemContainer = Instantiate(itemPrefab, visibleChildren.transform, false);
					newItemContainer.GetComponent<Button>().onClick.AddListener(() => ConfirmButtonChangeHandler(indexSlider, localIndex));
				}
			}

			for (var i = 0; i < visibleSymbols; i++)
			{
				var newI = (sourceItems.Count + index + i - cursorIndex) % sourceItems.Count;
				var child = visibleChildren.transform.GetChild(i);
				var indexText = child.Find("IndexText");
				var itemText = child.Find("ItemText");

				indexText.GetComponent<Text>().text = newI.ToString();
				itemText.GetComponent<Text>().text = sourceItems[newI];
			}
		}

		#endregion
	}
}