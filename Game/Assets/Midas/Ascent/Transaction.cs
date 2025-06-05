using IGT.Ascent.Communication.Platform.Interfaces;

namespace Midas.Ascent
{
	internal sealed class Transaction
	{
		private bool requiresClose;

		private Transaction(bool requiresClose)
		{
			this.requiresClose = requiresClose;
		}

		public void Close()
		{
			if (!requiresClose)
				return;

			CloseTransaction();
			requiresClose = false;
		}

		#region Static Methods

		public static bool IsGameTransactionOpen => AscentFoundation.GameLibRestricted.TransactionOpen;

		public static Transaction CreateTransient()
		{
			switch (OpenTransaction())
			{
				case ErrorCode.NoError:
					return new Transaction(true);
				case ErrorCode.OpenTransactionExisted:
					return new Transaction(false);
				// default:
				// 	Log.Instance.Fatal($"Failed open transaction ErrorCode={ErrorCode}");
				// 	break;
			}

			return null;
		}

		public static ErrorCode OpenTransaction()
		{
			ErrorCode errorCode;
			while (true)
			{
				// If we are in a callback from ProcessEvents then the foundation already has a transaction open for us.

				if (AscentFoundation.IsInFoundationCallback || IsGameTransactionOpen)
				{
					errorCode = ErrorCode.OpenTransactionExisted;
					break;
				}

				errorCode = AscentFoundation.GameLibRestricted.CreateTransaction();

				if (HandleCreateTransactionErrorCode(errorCode))
					break;
			}

			return errorCode;
		}

		public static void CloseTransaction()
		{
			AscentFoundation.GameLibRestricted.CloseTransaction();
		}

		private static bool HandleCreateTransactionErrorCode(ErrorCode errorCode)
		{
			switch (errorCode)
			{
				case ErrorCode.EventWaitingForProcess:
					ProcessEventsAndCheckExit();
					break;

				case ErrorCode.NoError:
					return true;

				case ErrorCode.OpenTransactionExisted:
					return true;

				default:
					ThrowExceptionIfWeShouldExit();
					break;
			}

			return false;
		}

		private static void ProcessEventsAndCheckExit()
		{
			ThrowExceptionIfWeShouldExit();

			AscentFoundation.ProcessEvents(0);

			ThrowExceptionIfWeShouldExit();
		}

		private static void ThrowExceptionIfWeShouldExit()
		{
			if (AscentGameEngine.ShouldGameEngineExit)
				throw new StopForcedException("DeInit forced during ProcessEvents");
		}

		#endregion
	}
}