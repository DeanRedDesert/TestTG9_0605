using System.Collections.Generic;
using Midas.Core;
using Midas.Presentation.Data.PropertyReference;
using UnityEngine;

namespace Midas.Presentation.Reels.Ears
{
	public sealed class EarController : MonoBehaviour
	{
		private IStakeCombination lastStakeCombo;

		[SerializeField]
		private PropertyReference<IStakeCombination> stakeComboPropRef;

		[SerializeField]
		private List<Ear> earTemplates;

		private void OnEnable()
		{
			if (stakeComboPropRef == null)
				return;

			stakeComboPropRef.ValueChanged += OnStakeComboChanged;
			Refresh();
		}

		private void OnStakeComboChanged(PropertyReference propRef, string propName)
		{
			Refresh();
		}

		private void OnDisable()
		{
			stakeComboPropRef.ValueChanged -= OnStakeComboChanged;
			stakeComboPropRef.DeInit();
		}

		private void Refresh()
		{
			var stakeCombination = stakeComboPropRef.Value;

			if (lastStakeCombo == stakeCombination)
				return;

			lastStakeCombo = stakeCombination;

			if (stakeCombination == null)
				return;

			for (var i = 0; i < earTemplates.Count; i++)
			{
				var ear = earTemplates[i];
				ear.gameObject.SetActive(ear.UpdateEar(stakeCombination));
			}
		}
	}
}