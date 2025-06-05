using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Midas.Presentation.Data.PropertyReference
{
	[Serializable]
	public abstract class PropertyReference
	{
		private PropertyBinding binding;

		[FormerlySerializedAs("_path")]
		[PropertyPath("RequiredType")]
		[SerializeField]
		private string path;

		public event Action<PropertyReference, string> ValueChanged;

		public Type Type
		{
			get
			{
				Resolve();
				return binding.PropertyType;
			}
		}

		public string Path
		{
			get => path;
			set
			{
				if (path == null || !path.Equals(value))
				{
					path = value;
					Reset();
				}
			}
		}

		protected object ObjectValue
		{
			get
			{
				Resolve();
				return binding.Value;
			}

			set
			{
				Resolve();
				binding.Value = value;
			}
		}

		protected abstract Type RequiredType { get; }

		protected PropertyReference(string path = "")
		{
			this.path = path;
		}

		public virtual void Init()
		{
			Resolve();
		}

		public virtual void DeInit()
		{
			Reset();
		}

		protected virtual void OnPropertyChanged()
		{
			ValueChanged?.Invoke(this, path);
		}

		protected virtual void Reset()
		{
			if (binding != null)
			{
				binding.ValueChanged -= OnPropertyChanged;
				binding = null;
			}
		}

		private void Resolve()
		{
			if (binding == null)
			{
				DoResolve();
				if (binding == null)
				{
					throw new ArgumentException($"ItemReference '{path}' not resolved");
				}
			}
		}

		private void DoResolve()
		{
			binding = PropertyPathResolver.ResolveProperty(path);
			binding.ValueChanged += OnPropertyChanged;

			if (!RequiredType.IsAssignableFrom(binding.PropertyType))
			{
				throw new ArgumentException($"Required type '{RequiredType.FullName}' is not compatible to '{Type.FullName}' of '{path}'");
			}
		}

#if UNITY_EDITOR
		public virtual void ConfigureForMakeGame(string pathToConfigure)
		{
			path = pathToConfigure;
		}
#endif
	}
}