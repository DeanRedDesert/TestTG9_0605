using System;

namespace Midas.Presentation.Data.StatusBlocks
{
	[Flags]
	public enum Popup
	{
		Info = 1,
		Reserve = 2,
		DenomMenu = 4,
		CashoutConfirm = 8,
		Autoplay = 16,
		CreditPlayoff = 32,
		Volume = 64,
		Language = 128
	}

	public sealed class PopupStatus : StatusBlock
	{
		private StatusProperty<ulong> popupStatus;

		public PopupStatus() : base(nameof(PopupStatus))
		{
		}

		public ulong Status() => popupStatus.Value;

		[Expression("PopupStatus")] public static bool IsInfoOpen => StatusDatabase.PopupStatus.IsOpen(Popup.Info);
		[Expression("PopupStatus")] public static bool IsReserveOpen => StatusDatabase.PopupStatus.IsOpen(Popup.Reserve);
		[Expression("PopupStatus")] public static bool IsDenomMenuOpen => StatusDatabase.PopupStatus.IsOpen(Popup.DenomMenu);
		[Expression("PopupStatus")] public static bool IsCashoutConfirmOpen => StatusDatabase.PopupStatus.IsOpen(Popup.CashoutConfirm);
		[Expression("PopupStatus")] public static bool IsAutoplayOpen => StatusDatabase.PopupStatus.IsOpen(Popup.Autoplay);
		[Expression("PopupStatus")] public static bool IsCreditPlayoffOpen => StatusDatabase.PopupStatus.IsOpen(Popup.CreditPlayoff);
		[Expression("PopupStatus")] public static bool IsVolumeOpen => StatusDatabase.PopupStatus.IsOpen(Popup.Volume);
		[Expression("PopupStatus")] public static bool IsLanguageOpen => StatusDatabase.PopupStatus.IsOpen(Popup.Language);

		public void Set(Popup popup, bool value)
		{
			if (value)
				Open(popup);
			else
				Close(popup);
		}

		public void Open(Popup popup) => popupStatus.Value |= (ulong)popup;
		public void Close(Popup popup) => popupStatus.Value &= ~(ulong)popup;

		public bool IsOpen(Popup popup) => (popupStatus.Value & (ulong)popup) > 0;
		public bool AreOpen(Popup popup) => (popupStatus.Value & (ulong)popup) > 0;

		public bool AreAnyOpen() => popupStatus.Value > 0;

		protected override void DoResetProperties()
		{
			base.DoResetProperties();
			popupStatus = AddProperty(nameof(Status), 0UL);
		}
	}
}