using System.Collections.Generic;
using System.Threading.Tasks;
using Midas.Core.General;
using Midas.Presentation.ExtensionMethods;
using Midas.Presentation.Game;
using UnityEngine;

namespace Midas.Presentation.Sequencing
{
	/// <summary>
	/// Base component that provides the ability to be activated as part of a sequence.
	/// </summary>
	public abstract class SequenceActivatorBase : MonoBehaviour
	{
		private readonly AutoUnregisterHelper autoUnregisterHelper = new AutoUnregisterHelper();
		private TaskCompletionSource<int> dependentTask;

		protected SequenceFinder SequenceFinder { get; private set; }

		public abstract IReadOnlyList<string> SequenceNames { get; }

		protected void SetAsDependency(TaskCompletionSource<int> awaiter)
		{
			dependentTask = awaiter;
		}

		protected void Finish()
		{
			dependentTask?.SetResult(0);
			dependentTask = null;
		}

		protected virtual void Awake()
		{
			SequenceFinder = GameBase.GameInstance.GetPresentationController<SequenceFinder>();
		}

		protected virtual void OnEnable()
		{
			foreach (var sequenceName in SequenceNames)
			{
				var matchingSequence = SequenceFinder.FindSequence(sequenceName);
				if (matchingSequence == null)
				{
					Log.Instance.Fatal($"No sequence with name {sequenceName} found in {this.GetPath()}");
				}

				RegisterSequenceEvents(autoUnregisterHelper, matchingSequence);
			}
		}

		protected virtual void OnDisable()
		{
			autoUnregisterHelper.UnRegisterAll();
		}

		protected abstract void RegisterSequenceEvents(AutoUnregisterHelper autoUnregisterHelper, Sequence sequence);
	}
}