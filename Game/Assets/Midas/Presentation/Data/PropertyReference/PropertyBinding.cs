using System;
using System.Reflection;
using Midas.Core;

namespace Midas.Presentation.Data.PropertyReference
{
	public abstract class PropertyBinding
	{
		private readonly object rootObject;
		private readonly string path;
		private readonly ((PropertyInfo propertyInfo, int index)[] infos, Type returnType) propertyInfos;

		private object cachedValue;

		private event Action Changed;

		public event Action ValueChanged
		{
			add
			{
				var wasNull = Changed == null;
				Changed += value;
				if (wasNull)
					RegisterChangeEvent();
			}
			remove
			{
				Changed -= value;
				UnregisterChangeEvent();
			}
		}

		public Type PropertyType => propertyInfos.returnType;

		public object Value
		{
			get
			{
				if (cachedValue == null)
				{
					var resultVal = rootObject;

					for (var i = 0; i < propertyInfos.infos.Length; i++)
					{
						var p = propertyInfos.infos[i];

						if (resultVal == null && !p.propertyInfo.GetGetMethod().IsStatic)
							break;

						if (p.propertyInfo != null)
						{
							resultVal = p.propertyInfo.GetValue(resultVal);
						}
						else
						{
							if (resultVal.GetType().IsArray && resultVal is object[] valueAsArray)
							{
								if (p.index < valueAsArray.Length)
								{
									resultVal = valueAsArray[p.index];
								}
								else
								{
									Log.Instance.Debug($"{path}: array is to small for index={p.index}");
									return null;
								}
							}
							else
							{
								dynamic x = resultVal; //maybe its a struct[] like Credit[]
								int num = x.GetType().IsArray ? x.Length : x.Count;
								if (num > p.index)
								{
									var y = x[p.index];
									resultVal = y;
								}
								else
								{
									Log.Instance.Debug($"{path}: list is to small for index={p.index}");
									return null;
								}
							}
						}
					}

					cachedValue = resultVal;
				}

				return cachedValue;
			}

			set
			{
				if (propertyInfos.infos.Length == 1 && rootObject != null)
				{
					var p = propertyInfos.infos[0];
					if (p.propertyInfo != null)
					{
						p.propertyInfo.SetValue(rootObject, value);
					}
				}
			}
		}

		protected PropertyBinding(Type rootType, string path)
			: this(null, rootType, path)
		{
		}

		protected PropertyBinding(object rootObject, string path)
			: this(rootObject, rootObject.GetType(), path)
		{
		}

		private PropertyBinding(object rootObject, Type rootType, string path)
		{
			this.rootObject = rootObject;
			this.path = path;

			propertyInfos = ReflectionUtil.GetPropertyInfos(rootType, path);

			if (propertyInfos.infos.Length < 1)
				throw new ArgumentException($"Property path '{path}' is not correct");
		}

		protected void RaiseValueChanged()
		{
			cachedValue = null;
			Changed?.Invoke();
		}

		protected abstract void RegisterChangeEvent();
		protected abstract void UnregisterChangeEvent();
	}
}