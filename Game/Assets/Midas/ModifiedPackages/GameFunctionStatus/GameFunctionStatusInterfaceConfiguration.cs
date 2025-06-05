using System;
using System.Collections.Generic;
using IGT.Game.Core.Communication.Foundation.F2X;
using IGT.Game.Core.Communication.Foundation.F2XTransport;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces;

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.GameFunctionStatus
{
	/// <summary>
	/// Provides configuration information for <see cref="IGameFunctionStatus"/>
	/// </summary>
	public class GameFunctionStatusInterfaceConfiguration : F2XInterfaceExtensionConfigurationBase
	{
		#region Fields

		private readonly bool required;
		private static readonly CategoryVersionInformation gameFunctionStatus1P0 = new CategoryVersionInformation((int)1041, 1, 0);

		#endregion

		#region Constructors

		/// <summary>
		/// Provides configuration information for <see cref="IGameFunctionStatus"/>
		/// </summary>
		/// <param name="required">If the <see cref="IGameFunctionStatusCategory"/> is required</param>
		public GameFunctionStatusInterfaceConfiguration(bool required)
		{
			this.required = required;
		}

		#endregion

		#region F2XInterfaceExtensionConfigurationBase overrides

		///<inheritdoc />
		public override IInterfaceExtension CreateInterfaceExtension(IInterfaceExtensionDependencies dependencies)
		{
			if (dependencies is F2XInterfaceExtensionDependencies f2x)
			{
				return new F2XGameFunctionStatus(dependencies.TransactionalEventDispatcher, f2x.Category as IGameFunctionStatusCategory);
			}

			var standalone = (dependencies as StandaloneInterfaceExtensionDependencies) ?? throw new InterfaceExtensionDependencyException("No valid dependencies could be located");

			return new StandaloneGameFunctionStatus(standalone.EventPoster, standalone.TransactionalEventDispatcher);
		}

		///<inheritdoc />
		public override Type InterfaceType => typeof(IGameFunctionStatus);

		///<inheritdoc />
		public override IEnumerable<CategoryRequest> GetCategoryRequests()
		{
			return new List<CategoryRequest>
			{
				new CategoryRequest(gameFunctionStatus1P0, required, FoundationTarget.AllAscent,
					dependencies=> new GameFunctionStatusCategory(dependencies.Transport, new GameFunctionStatusCallbackHandler(dependencies.EventCallbacks)))
			};
		}

		#endregion
	}
}
