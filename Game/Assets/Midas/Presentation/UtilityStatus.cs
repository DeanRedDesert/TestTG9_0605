using System.Collections.Generic;
using Midas.Presentation.Data;

namespace Midas.Presentation
{
	public sealed class UtilityStatus : StatusBlock
	{
		private StatusProperty<bool> isUtilityActive;
		private StatusProperty<bool> isResultPickerActive;
		private StatusProperty<IReadOnlyList<string>> supportedThemes;
		private StatusProperty<IReadOnlyDictionary<KeyValuePair<string, string>, IReadOnlyList<long>>> registrySupportedDenominations;

		public bool IsResultPickerActive
		{
			get => isResultPickerActive.Value;
			set => isResultPickerActive.Value = value;
		}

		public bool IsUtilityModeEnabled
		{
			get => isUtilityActive.Value;
			set => isUtilityActive.Value = value;
		}

		public IReadOnlyList<string> SupportedThemes
		{
			get => supportedThemes.Value;
			set => supportedThemes.Value = value;
		}

		public IReadOnlyDictionary<KeyValuePair<string, string>, IReadOnlyList<long>> RegistrySupportedDenominations
		{
			get => registrySupportedDenominations.Value;
			set => registrySupportedDenominations.Value = value;
		}

		public UtilityStatus() : base(nameof(UtilityStatus))
		{
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();

			isResultPickerActive = AddProperty(nameof(IsResultPickerActive), false);
			isUtilityActive = AddProperty(nameof(IsUtilityModeEnabled), false);
			supportedThemes = AddProperty(nameof(SupportedThemes), (IReadOnlyList<string>)new List<string>());
			registrySupportedDenominations = AddProperty(nameof(RegistrySupportedDenominations), (IReadOnlyDictionary<KeyValuePair<string, string>, IReadOnlyList<long>>)new Dictionary<KeyValuePair<string, string>, IReadOnlyList<long>>());
		}
	}
}