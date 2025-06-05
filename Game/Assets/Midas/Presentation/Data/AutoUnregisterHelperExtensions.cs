using System;
using System.Collections.Generic;
using System.Reflection;
using Midas.Core.General;

namespace Midas.Presentation.Data
{
	public static class AutoUnregisterHelperExtensions
	{
		public static void RegisterPropertyChangedHandler<T>(this AutoUnregisterHelper autoUnregisterHelper, StatusBlock statusBlock, string propertyName, PropertyChangedHandler<T> handler)
		{
			if (statusBlock != null)
				autoUnregisterHelper.Register(new RegisterClassStatusProperty<T>(statusBlock, propertyName, handler));
		}

		public static void RegisterPropertyChangedHandler(this AutoUnregisterHelper autoUnregisterHelper, StatusBlock statusBlock, string propertyName, PropertyChangedHandler handler)
		{
			if (statusBlock != null)
				autoUnregisterHelper.Register(new RegisterClassStatusProperty(statusBlock, propertyName, handler));
		}

		public static void RegisterAnyPropertyChangedHandler(this AutoUnregisterHelper autoUnregisterHelper, StatusBlock statusBlock, PropertyChangedHandler handler)
		{
			if (statusBlock != null)
				autoUnregisterHelper.Register(new RegisterClassAnyProperty(statusBlock, handler));
		}

		public static void RegisterMultiplePropertyChangedHandler(this AutoUnregisterHelper autoUnregisterHelper, IReadOnlyList<(StatusBlock statusBlock, string propertyName)> items, StatusDatabase.MultiplePropertyChangedHandler handler)
		{
			autoUnregisterHelper.Register(new RegisterClassMultipleStatusProperties(items, handler));
		}

		public static void RegisterExpressionChangedHandler(this AutoUnregisterHelper autoUnregisterHelper, Type type, string propertyName, Action action)
		{
			autoUnregisterHelper.Register(new RegisterClassExpression(type, propertyName, action));
		}

		/// <summary>
		/// Registers for any property changes on a status block.
		/// </summary>
		private sealed class RegisterClassAnyProperty : IRegisterClass
		{
			private readonly StatusBlock statusBlock;
			private readonly PropertyChangedHandler handler;

			public RegisterClassAnyProperty(StatusBlock statusBlock, PropertyChangedHandler handler)
			{
				this.statusBlock = statusBlock;
				this.handler = handler;
			}

			public void Register() => statusBlock.AnyPropertyChanged += handler;
			public void UnRegister() => statusBlock.AnyPropertyChanged -= handler;
		}

		/// <summary>
		/// Registers for multiple property changes in a single status update (prevents duplicating effort when multiple changes happen that affect a single thing).
		/// </summary>
		private sealed class RegisterClassMultipleStatusProperties : IRegisterClass
		{
			private readonly IReadOnlyList<(StatusBlock statusBlock, string propertyName)> items;
			private readonly StatusDatabase.MultiplePropertyChangedHandler handler;

			public RegisterClassMultipleStatusProperties(IReadOnlyList<(StatusBlock statusBlock, string propertyName)> items, StatusDatabase.MultiplePropertyChangedHandler handler)
			{
				this.items = items;
				this.handler = handler;
			}

			public void Register() => StatusDatabase.RegisterMultiplePropertyChangedHandler(items, handler);
			public void UnRegister() => StatusDatabase.UnregisterMultiplePropertyChangedHandler(items, handler);
		}

		/// <summary>
		/// Registers for a change of a property on a status item.
		/// </summary>
		private sealed class RegisterClassStatusProperty : IRegisterClass
		{
			private readonly StatusBlock statusBlock;
			private readonly string propertyName;
			private readonly PropertyChangedHandler handler;

			public RegisterClassStatusProperty(StatusBlock statusBlock, string propertyName, PropertyChangedHandler handler)
			{
				this.statusBlock = statusBlock;
				this.propertyName = propertyName;
				this.handler = handler;
			}

			public void Register() => statusBlock.AddPropertyChangedHandler(propertyName, handler);
			public void UnRegister() => statusBlock.RemovePropertyChangedHandler(propertyName, handler);
		}

		/// <summary>
		/// Registers for a change of a property on a status item.
		/// </summary>
		/// <typeparam name="T">The game service type.</typeparam>
		private sealed class RegisterClassStatusProperty<T> : IRegisterClass
		{
			private readonly StatusBlock statusBlock;
			private readonly string propertyName;
			private readonly PropertyChangedHandler<T> handler;

			public RegisterClassStatusProperty(StatusBlock statusBlock, string propertyName, PropertyChangedHandler<T> handler)
			{
				this.statusBlock = statusBlock;
				this.propertyName = propertyName;
				this.handler = handler;
			}

			public void Register() => statusBlock.AddPropertyChangedHandler(propertyName, handler);
			public void UnRegister() => statusBlock.RemovePropertyChangedHandler(propertyName, handler);
		}

		private sealed class RegisterClassExpression : IRegisterClass
		{
			private readonly PropertyInfo property;
			private readonly Action action;

			public RegisterClassExpression(Type type, string propertyName, Action action)
			{
				property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
				if (property == null)
					Log.Instance.Fatal($"Could not register expression {type.Name}.{propertyName}: static property not found");
				this.action = action;
			}

			public void Register() => ExpressionManager.RegisterChangeEvent(property, action);

			public void UnRegister() => ExpressionManager.UnRegisterChangeEvent(property, action);
		}
	}
}