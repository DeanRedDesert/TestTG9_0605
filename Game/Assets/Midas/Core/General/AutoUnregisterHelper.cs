using System.Collections.Generic;

namespace Midas.Core.General
{
	/// <summary>
	/// Aids in registering and unregistering things related to the status database.
	/// </summary>
	public interface IRegisterClass
	{
		void Register();
		void UnRegister();
	}

	/// <summary>
	/// Helps register events for status items.
	/// </summary>
	public sealed class AutoUnregisterHelper
	{
		private readonly List<IRegisterClass> registeredClasses = new List<IRegisterClass>();

		public void UnRegisterAll()
		{
			foreach (var regClass in registeredClasses)
				regClass.UnRegister();

			registeredClasses.Clear();
		}

		public void Register(IRegisterClass regClass)
		{
			regClass.Register();
			registeredClasses.Add(regClass);
		}
	}
}