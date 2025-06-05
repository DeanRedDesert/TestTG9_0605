using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core.LogicServices;

namespace Midas.LogicToPresentation.Data
{
	public class CompositeGameService : IGameService
	{
		private readonly List<IGameService> childServices = new List<IGameService>();

		protected virtual void CreateServices()
		{
		}

		protected internal void AddService(IGameService service, string serviceName)
		{
			service.Init($"{Name}.{serviceName}");
			childServices.Add(service);
		}

		protected internal void RemoveService(IGameService service)
		{
			childServices.Remove(service);
			service.DeInit();
		}

		protected internal T GetService<T>() where T : CompositeGameService
		{
			return (T)childServices.Single(s => s.GetType() == typeof(T));
		}

		#region Implementation of IGameService

		public string Name { get; private set; }

		public void Init(string name)
		{
			Name = name;
			CreateServices();
		}

		public void DeInit()
		{
			foreach (var service in childServices)
				service.DeInit();

			childServices.Clear();
		}

		bool IGameService.IsHistoryRequired(HistorySnapshotType snapshotType)
		{
			foreach (var service in childServices)
			{
				if (service.IsHistoryRequired(snapshotType))
					return true;
			}

			return false;
		}

		void IGameService.DeliverChange(object value)
		{
			throw new InvalidOperationException("We should never get here");
		}

		void IGameService.Refresh()
		{
			foreach (var service in childServices)
				service.Refresh();
		}

		object IGameService.GetHistoryData(HistorySnapshotType snapshotType)
		{
			var result = new List<object>(childServices.Count);

			foreach (var service in childServices)
			{
				if (service.IsHistoryRequired(snapshotType))
					result.Add(service.GetHistoryData(snapshotType));
			}

			return result;
		}

		void IGameService.RestoreHistoryData(HistorySnapshotType snapshotType, object o)
		{
			var array = (List<object>)o;
			var i = 0;
			foreach (var service in childServices)
			{
				if (service.IsHistoryRequired(snapshotType))
					service.RestoreHistoryData(snapshotType, array[i++]);
			}
		}

		#endregion
	}
}