using System.Xml.Serialization;
using IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.Types;
using IGT.Game.Core.Communication.Foundation.F2XTransport;

namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus
{
	/// <summary>
	/// Partial class to add <see cref="ICategoryMessage"/>.
	/// </summary>
	public partial class GameFunctionStatusMessage : ICategoryMessage
	{
	}

	/// <summary>
	/// Partial class to add <see cref="ICategory"/>.
	/// </summary>
	public partial class GameFunctionStatus : ICategory
	{
		/// <summary>
		/// Default constructor to initialize the interior message.
		/// </summary>
		public GameFunctionStatus()
		{
			Message = new GameFunctionStatusMessage();
		}

		#region ICategory Members

		/// <inheritdoc/>
		ICategoryMessage ICategory.Message => Message;

		/// <inheritdoc/>
		IVersion ICategory.Version
		{
			get => Version;
			set => Version = (Version)value;
		}

		#endregion

	}

}

namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus
{
	using Types;

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(AnonymousType = true, Namespace = "F2XGameFunctionStatusVer1.xsd")]
	[XmlRootAttribute(Namespace = "F2XGameFunctionStatusVer1.xsd", IsNullable = false)]
	public partial class GameFunctionStatus
	{
		private Version versionField;

		private GameFunctionStatusMessage messageField;

		/// <remarks/>
		public Version Version
		{
			get => versionField;
			set => versionField = value;
		}

		/// <remarks/>
		public GameFunctionStatusMessage Message
		{
			get => messageField;
			set => messageField = value;
		}
	}
}

namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus
{
	using System.Collections.Generic;

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(Namespace = "F2XGameFunctionStatusVer1.xsd")]
	public partial class GameButtonStatusList
	{
		private List<GameButtonBehaviorType> gameButtonsField = new List<GameButtonBehaviorType>();

		/// <remarks/>
		[XmlElementAttribute("GameButtons")]
		public List<GameButtonBehaviorType> GameButtons
		{
			get => gameButtonsField;
			set => gameButtonsField = value;
		}
	}
}

namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus
{
	using System.Collections.Generic;

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(Namespace = "F2XGameFunctionStatusVer1.xsd")]
	public partial class GameButtonBehaviorType
	{
		private GameButtonType gameButtonField;

		private GameButtonStatusType gameButtonStatusField;

		/// <remarks/>
		public GameButtonType GameButton
		{
			get => gameButtonField;
			set => gameButtonField = value;
		}

		/// <remarks/>
		public GameButtonStatusType GameButtonStatus
		{
			get => gameButtonStatusField;
			set => gameButtonStatusField = value;
		}
	}
}

namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus
{
	using System.Collections.Generic;

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "")]
	[System.SerializableAttribute()]
	[XmlTypeAttribute(Namespace = "F2XGameFunctionStatusVer1.xsd")]
	public enum GameButtonType
	{
		/// <remarks/>
		Collect,

		/// <remarks/>
		Volume,

		/// <remarks/>
		DenominationSelection,

		/// <remarks/>
		Bets,

		/// <remarks/>
		Reserve,

		/// <remarks/>
		Info,

		/// <remarks/>
		MoreGames
	}
}

namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus
{
	using System.Collections.Generic;

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "")]
	[System.SerializableAttribute()]
	[XmlTypeAttribute(Namespace = "F2XGameFunctionStatusVer1.xsd")]
	public enum GameButtonStatusType
	{
		/// <remarks/>
		Active,

		/// <remarks/>
		SoftDisabled,

		/// <remarks/>
		Hidden,

		/// <remarks/>
		Invalid
	}
}

namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus
{
	using System.Collections.Generic;

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(Namespace = "F2XGameFunctionStatusVer1.xsd")]
	public partial class DenominationStatusList
	{
		private List<DenominationPlayableStatusType> statusesField = new List<DenominationPlayableStatusType>();

		/// <remarks/>
		[XmlElementAttribute("Statuses")]
		public List<DenominationPlayableStatusType> Statuses
		{
			get => statusesField;
			set => statusesField = value;
		}
	}
}

namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus
{
	using System.Collections.Generic;

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(Namespace = "F2XGameFunctionStatusVer1.xsd")]
	public partial class DenominationPlayableStatusType
	{
		private uint denominationField;

		private GameButtonStatusType denominationButtonStatusField;

		/// <remarks/>
		public uint Denomination
		{
			get => denominationField;
			set => denominationField = value;
		}

		/// <remarks/>
		public GameButtonStatusType DenominationButtonStatus
		{
			get => denominationButtonStatusField;
			set => denominationButtonStatusField = value;
		}
	}
}

namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus
{
	using System.Collections.Generic;

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(Namespace = "F2XGameFunctionStatusVer1.xsd")]
	public partial class GetDenominationPlayableStatusReplyContent
	{
		private DenominationStatusList statusListField;

		/// <remarks/>
		public DenominationStatusList StatusList
		{
			get => statusListField;
			set => statusListField = value;
		}
	}
}

namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus
{
	using System.Collections.Generic;

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(Namespace = "F2XGameFunctionStatusVer1.xsd")]
	public partial class GetDenominationTimeoutReplyMessage
	{
		private uint denominationTimeoutField;

		private bool timeoutActiveField;

		/// <remarks/>
		public uint DenominationTimeout
		{
			get => denominationTimeoutField;
			set => denominationTimeoutField = value;
		}

		/// <remarks/>
		public bool TimeoutActive
		{
			get => timeoutActiveField;
			set => timeoutActiveField = value;
		}
	}
}

namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus {
    using System.Collections.Generic;
    using Types;
    using DiscoveryContextTypes;
    using LocalizationTypes;
    using AppTypes;
    using PropertyTypes;
    using AppDecorationTypes;
    using ExtensionProgressiveControllerTypes;

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="F2XGameFunctionStatusVer1.xsd")]
public partial class ButtonStatusChangedSend : SendTransactionalMessage {
    
    private List<GameButtonBehaviorType> gameButtonBehaviorField = new List<GameButtonBehaviorType>();
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("GameButtonBehavior")]
    public List<GameButtonBehaviorType> GameButtonBehavior {
        get {
            return this.gameButtonBehaviorField;
        }
        set {
            this.gameButtonBehaviorField = value;
        }
    }
}

}

namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus {
    using System.Collections.Generic;
    using Types;
    using DiscoveryContextTypes;
    using LocalizationTypes;
    using AppTypes;
    using PropertyTypes;
    using AppDecorationTypes;
    using ExtensionProgressiveControllerTypes;

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="F2XGameFunctionStatusVer1.xsd")]
public partial class GetGameButtonStatusSend : SendTransactionalMessage {
}

}

namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus {
    using System.Collections.Generic;
    using Types;
    using DiscoveryContextTypes;
    using LocalizationTypes;
    using AppTypes;
    using PropertyTypes;
    using AppDecorationTypes;
    using ExtensionProgressiveControllerTypes;

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="F2XGameFunctionStatusVer1.xsd")]
public partial class GetDenominationPlayableStatusSend : SendTransactionalMessage {
}

}

namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus {
    using System.Collections.Generic;
    using Types;
    using DiscoveryContextTypes;
    using LocalizationTypes;
    using AppTypes;
    using PropertyTypes;
    using AppDecorationTypes;
    using ExtensionProgressiveControllerTypes;

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="F2XGameFunctionStatusVer1.xsd")]
public partial class ChangeDenominationPlayableStatusSend : SendTransactionalMessage {
    
    private List<DenominationPlayableStatusType> denominationSelectionStatusField = new List<DenominationPlayableStatusType>();
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("DenominationSelectionStatus")]
    public List<DenominationPlayableStatusType> DenominationSelectionStatus {
        get {
            return this.denominationSelectionStatusField;
        }
        set {
            this.denominationSelectionStatusField = value;
        }
    }
}

}

namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus {
    using System.Collections.Generic;
    using Types;
    using DiscoveryContextTypes;
    using LocalizationTypes;
    using AppTypes;
    using PropertyTypes;
    using AppDecorationTypes;
    using ExtensionProgressiveControllerTypes;

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="F2XGameFunctionStatusVer1.xsd")]
public partial class GetDenominationTimeoutSend : SendTransactionalMessage {
}

}

namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus
{
	using System.Collections.Generic;
	using Types;
	using DiscoveryContextTypes;
	using LocalizationTypes;
	using AppTypes;
	using PropertyTypes;
	using AppDecorationTypes;
	using ExtensionProgressiveControllerTypes;

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "F2XGameFunctionStatusVer1.xsd")]
	public partial class SetDenominationTimeoutSend : SendTransactionalMessage
	{

		private uint denominationTimeoutField;

		private bool timeoutActiveField;

		/// <remarks/>
		public uint DenominationTimeout
		{
			get
			{
				return this.denominationTimeoutField;
			}
			set
			{
				this.denominationTimeoutField = value;
			}
		}

		/// <remarks/>
		public bool TimeoutActive
		{
			get
			{
				return this.timeoutActiveField;
			}
			set
			{
				this.timeoutActiveField = value;
			}
		}
	}

}

namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus
{
	using System.Collections.Generic;
	using Types;
	using DiscoveryContextTypes;
	using LocalizationTypes;
	using AppTypes;
	using PropertyTypes;
	using AppDecorationTypes;
	using ExtensionProgressiveControllerTypes;

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "F2XGameFunctionStatusVer1.xsd")]
	public partial class GameFunctionStatusMessage
	{

		private object itemField;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("ButtonStatusChangedReply", typeof(ButtonStatusChangedReply))]
		[System.Xml.Serialization.XmlElementAttribute("ButtonStatusChangedSend", typeof(ButtonStatusChangedSend))]
		[System.Xml.Serialization.XmlElementAttribute("ChangeDenominationPlayableStatusReply", typeof(ChangeDenominationPlayableStatusReply))]
		[System.Xml.Serialization.XmlElementAttribute("ChangeDenominationPlayableStatusSend", typeof(ChangeDenominationPlayableStatusSend))]
		[System.Xml.Serialization.XmlElementAttribute("GetDenominationPlayableStatusReply", typeof(GetDenominationPlayableStatusReply))]
		[System.Xml.Serialization.XmlElementAttribute("GetDenominationPlayableStatusSend", typeof(GetDenominationPlayableStatusSend))]
		[System.Xml.Serialization.XmlElementAttribute("GetDenominationTimeoutReply", typeof(GetDenominationTimeoutReply))]
		[System.Xml.Serialization.XmlElementAttribute("GetDenominationTimeoutSend", typeof(GetDenominationTimeoutSend))]
		[System.Xml.Serialization.XmlElementAttribute("GetGameButtonStatusReply", typeof(GetGameButtonStatusReply))]
		[System.Xml.Serialization.XmlElementAttribute("GetGameButtonStatusSend", typeof(GetGameButtonStatusSend))]
		[System.Xml.Serialization.XmlElementAttribute("SetDenominationTimeoutReply", typeof(SetDenominationTimeoutReply))]
		[System.Xml.Serialization.XmlElementAttribute("SetDenominationTimeoutSend", typeof(SetDenominationTimeoutSend))]
		public object Item
		{
			get
			{
				return this.itemField;
			}
			set
			{
				this.itemField = value;
			}
		}
	}

}

namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus {
    using System.Collections.Generic;
    using Types;
    using DiscoveryContextTypes;
    using LocalizationTypes;
    using AppTypes;
    using PropertyTypes;
    using AppDecorationTypes;
    using ExtensionProgressiveControllerTypes;

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="F2XGameFunctionStatusVer1.xsd")]
public partial class ButtonStatusChangedReply : ReplyMessage {
}

}

namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus {
    using System.Collections.Generic;
    using Types;
    using DiscoveryContextTypes;
    using LocalizationTypes;
    using AppTypes;
    using PropertyTypes;
    using AppDecorationTypes;
    using ExtensionProgressiveControllerTypes;

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="F2XGameFunctionStatusVer1.xsd")]
public partial class GetGameButtonStatusReply : ReplyMessage {
    
    private GameButtonStatusList contentField = new GameButtonStatusList();
    
    /// <remarks/>
    public GameButtonStatusList Content {
        get {
            return this.contentField;
        }
        set {
            this.contentField = value;
        }
    }
}

}

namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus {
    using System.Collections.Generic;
    using Types;
    using DiscoveryContextTypes;
    using LocalizationTypes;
    using AppTypes;
    using PropertyTypes;
    using AppDecorationTypes;
    using ExtensionProgressiveControllerTypes;

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="F2XGameFunctionStatusVer1.xsd")]
public partial class GetDenominationPlayableStatusReply : ReplyMessage {
    
    private GetDenominationPlayableStatusReplyContent contentField = new GetDenominationPlayableStatusReplyContent();
    
    /// <remarks/>
    public GetDenominationPlayableStatusReplyContent Content {
        get {
            return this.contentField;
        }
        set {
            this.contentField = value;
        }
    }
}

}

namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus {
    using System.Collections.Generic;
    using Types;
    using DiscoveryContextTypes;
    using LocalizationTypes;
    using AppTypes;
    using PropertyTypes;
    using AppDecorationTypes;
    using ExtensionProgressiveControllerTypes;

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="F2XGameFunctionStatusVer1.xsd")]
public partial class ChangeDenominationPlayableStatusReply : ReplyMessage {
}

}

namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus {
    using System.Collections.Generic;
    using Types;
    using DiscoveryContextTypes;
    using LocalizationTypes;
    using AppTypes;
    using PropertyTypes;
    using AppDecorationTypes;
    using ExtensionProgressiveControllerTypes;

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="F2XGameFunctionStatusVer1.xsd")]
public partial class GetDenominationTimeoutReply : ReplyMessage {
    
    private GetDenominationTimeoutReplyMessage contentField = new GetDenominationTimeoutReplyMessage();
    
    /// <remarks/>
    public GetDenominationTimeoutReplyMessage Content {
        get {
            return this.contentField;
        }
        set {
            this.contentField = value;
        }
    }
}

}

namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus {
    using System.Collections.Generic;
    using Types;
    using DiscoveryContextTypes;
    using LocalizationTypes;
    using AppTypes;
    using PropertyTypes;
    using AppDecorationTypes;
    using ExtensionProgressiveControllerTypes;

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="F2XGameFunctionStatusVer1.xsd")]
public partial class SetDenominationTimeoutReply : ReplyMessage {
}

}