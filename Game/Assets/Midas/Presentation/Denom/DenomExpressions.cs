using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Midas.Core;
using Midas.Core.Configuration;
using Midas.Core.ExtensionMethods;
using Midas.Core.General;
using Midas.Presentation.Data;

namespace Midas.Presentation.Denom
{
	public sealed class DenomPlayableStatus
	{
		public Money Denom { get; }
		public GameButtonStatus Status { get; }

		public DenomPlayableStatus(Money denom, GameButtonStatus status)
		{
			Denom = denom;
			Status = status;
		}
	}

	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public static class DenomExpressions
	{
		private const string DenomCategoryName = "Denom";
		private static readonly AutoUnregisterHelper autoUnregisterHelper = new AutoUnregisterHelper();

		internal static void Init()
		{
			var props = new (StatusBlock, string)[]
			{
				(StatusDatabase.ConfigurationStatus, nameof(DenomConfig)),
				(StatusDatabase.GameFunctionStatus, nameof(DenominationPlayableStatus))
			};

			autoUnregisterHelper.RegisterMultiplePropertyChangedHandler(props, v => HandleUpdate(v));
		}

		private static void HandleUpdate(IReadOnlyList<(StatusBlock statusBlock, string propertyName)> changedProperties)
		{
			if (StatusDatabase.ConfigurationStatus.DenomConfig == null || StatusDatabase.ConfigurationStatus.DenomConfig.AvailableDenominations == null)
			{
				AllDenoms = Array.Empty<DenomPlayableStatus>();
				return;
			}

			AllDenoms = StatusDatabase.ConfigurationStatus.DenomConfig.AvailableDenominations.Select(ad => new DenomPlayableStatus(ad, GetStatus(ad))).ToList();
			VisibleDenoms = AllDenoms.Where(ad => ad.Status != GameButtonStatus.Hidden).ToList();
			ActiveDenoms = AllDenoms.Where(ad => ad.Status == GameButtonStatus.Active).ToList();

			GameButtonStatus GetStatus(Money denom)
			{
				if (StatusDatabase.GameFunctionStatus.DenominationPlayableStatus == null || StatusDatabase.GameFunctionStatus.DenominationPlayableStatus.Count == 0)
					return GameButtonStatus.Active;

				var index = StatusDatabase.GameFunctionStatus.DenominationPlayableStatus.FindIndex(dps => dps.Denomination == denom.AsMinorCurrency);
				return index == -1 ? GameButtonStatus.Active : StatusDatabase.GameFunctionStatus.DenominationPlayableStatus[index].ButtonStatus;
			}
		}

		internal static void DeInit()
		{
			autoUnregisterHelper.UnRegisterAll();
			AllDenoms = Array.Empty<DenomPlayableStatus>();
			VisibleDenoms = Array.Empty<DenomPlayableStatus>();
		}

		[Expression(DenomCategoryName)] public static bool IsMultiDenom => VisibleDenoms.Count > 1;

		[Expression(DenomCategoryName)] public static int NumOfVisibleDenoms => VisibleDenoms.Count;

		[Expression(DenomCategoryName)] public static IReadOnlyList<DenomPlayableStatus> AllDenoms { get; private set; } = Array.Empty<DenomPlayableStatus>();

		[Expression(DenomCategoryName)] public static IReadOnlyList<DenomPlayableStatus> VisibleDenoms { get; private set; } = Array.Empty<DenomPlayableStatus>();
		[Expression(DenomCategoryName)] public static IReadOnlyList<DenomPlayableStatus> ActiveDenoms { get; private set; } = Array.Empty<DenomPlayableStatus>();
	}
}