// ------------------------------------------------------------------------------
//  <auto-generated>
//    Generated by Xsd2Code++. Version 4.4.0.7
//    <NameSpace>IGT.Game.Core.Registries.Internal.F2X.F2XCdsBingoRegistryVer1</NameSpace><Collection>List</Collection><codeType>CSharp</codeType><EnableDataBinding>False</EnableDataBinding><GenerateCloneMethod>False</GenerateCloneMethod><GenerateDataContracts>False</GenerateDataContracts><DataMemberNameArg>OnlyIfDifferent</DataMemberNameArg><DataMemberOnXmlIgnore>False</DataMemberOnXmlIgnore><CodeBaseTag>Net35</CodeBaseTag><InitializeFields>Collections</InitializeFields><GenerateUnusedComplexTypes>True</GenerateUnusedComplexTypes><GenerateUnusedSimpleTypes>True</GenerateUnusedSimpleTypes><GenerateXMLAttributes>True</GenerateXMLAttributes><OrderXMLAttrib>False</OrderXMLAttrib><EnableLazyLoading>False</EnableLazyLoading><VirtualProp>False</VirtualProp><PascalCase>False</PascalCase><AutomaticProperties>False</AutomaticProperties><PropNameSpecified>Default</PropNameSpecified><PrivateFieldName>EndWithField</PrivateFieldName><PrivateFieldNamePrefix></PrivateFieldNamePrefix><EnableRestriction>False</EnableRestriction><RestrictionMaxLenght>False</RestrictionMaxLenght><RestrictionRegEx>False</RestrictionRegEx><RestrictionRange>False</RestrictionRange><ValidateProperty>False</ValidateProperty><ClassNamePrefix></ClassNamePrefix><ClassLevel>Public</ClassLevel><PartialClass>True</PartialClass><ClassesInSeparateFiles>False</ClassesInSeparateFiles><ClassesInSeparateFilesDir></ClassesInSeparateFilesDir><TrackingChangesEnable>False</TrackingChangesEnable><GenTrackingClasses>False</GenTrackingClasses><HidePrivateFieldInIDE>False</HidePrivateFieldInIDE><EnableSummaryComment>True</EnableSummaryComment><EnableAppInfoSettings>False</EnableAppInfoSettings><EnableExternalSchemasCache>True</EnableExternalSchemasCache><EnableDebug>True</EnableDebug><EnableWarn>False</EnableWarn><ExcludeImportedTypes>True</ExcludeImportedTypes><ExpandNesteadAttributeGroup>False</ExpandNesteadAttributeGroup><CleanupCode>True</CleanupCode><EnableXmlSerialization>False</EnableXmlSerialization><SerializeMethodName>Serialize</SerializeMethodName><DeserializeMethodName>Deserialize</DeserializeMethodName><SaveToFileMethodName>SaveToFile</SaveToFileMethodName><LoadFromFileMethodName>LoadFromFile</LoadFromFileMethodName><EnableEncoding>False</EnableEncoding><EnableXMLIndent>False</EnableXMLIndent><IndentChar>Indent2Space</IndentChar><NewLineAttr>False</NewLineAttr><OmitXML>False</OmitXML><Encoder>UTF8</Encoder><Serializer>XmlSerializer</Serializer><sspNullable>False</sspNullable><sspString>False</sspString><sspCollection>False</sspCollection><sspComplexType>False</sspComplexType><sspSimpleType>False</sspSimpleType><sspEnumType>False</sspEnumType><XmlSerializerEvent>False</XmlSerializerEvent><BaseClassName>EntityBase</BaseClassName><UseBaseClass>False</UseBaseClass><GenBaseClass>False</GenBaseClass><CustomUsings>IGT.Game.Core.Registries.Internal.F2X.F2XRegistryTypesVer1</CustomUsings><AttributesToExlude>System.ComponentModel.DefaultValueAttribute</AttributesToExlude>
//  </auto-generated>
// ------------------------------------------------------------------------------
#pragma warning disable
namespace IGT.Game.Core.Registries.Internal.F2X.F2XCdsBingoRegistryVer1
{
    using System;
    using System.Diagnostics;
    using System.Xml.Serialization;
    using System.Collections;
    using System.Xml.Schema;
    using System.ComponentModel;
    using IGT.Game.Core.Registries.Internal.F2X.F2XRegistryTypesVer1;
    using System.Xml;
    using System.Collections.Generic;
    
    
    /// <summary>
    /// Registry for describing the information to allow a theme to support the CDS Bingo environment.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "4.4.0.7")]
    [Serializable]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType=true, Namespace="F2XCdsBingoRegistryVer1.xsd")]
    [XmlRootAttribute(Namespace="F2XCdsBingoRegistryVer1.xsd", IsNullable=false)]
    public partial class CdsBingoRegistry
    {
        #region Private fields
        private string themeRegistryField;
        private ushort bingoGameThemeIDField;
        private CdsBingoRegistryExtendedDataFile extendedDataFileField;
        #endregion
        
        /// <summary>
        /// Relative path to the theme registry that is associated with this Bingo specific theme registry (i.e. this registry provides extended theme information related to Bingo).
        /// </summary>
        [XmlElement(DataType="anyURI")]
        public string ThemeRegistry
        {
            get
            {
                return themeRegistryField;
            }
            set
            {
                themeRegistryField = value;
            }
        }
        
        /// <summary>
        /// The Bingo protocol's "Game Theme ID" field.  This ID must be unique across ALL themes in ALL packages to satisfy uniqueness in the Bingo protocol domain.
        /// </summary>
        public ushort BingoGameThemeID
        {
            get
            {
                return bingoGameThemeIDField;
            }
            set
            {
                bingoGameThemeIDField = value;
            }
        }
        
        /// <summary>
        /// Relative path to a custom theme related file for use with the game theme.  The file at this path is not parsed by the Foundation.  This file path is passed to the Bingo Protocol executable extension to provide extended (protocol config) information related to the theme.
        /// </summary>
        public CdsBingoRegistryExtendedDataFile ExtendedDataFile
        {
            get
            {
                return extendedDataFileField;
            }
            set
            {
                extendedDataFileField = value;
            }
        }
    }
    
    /// <summary>
    /// Relative path to a custom theme related file for use with the game theme.  The file at this path is not parsed by the Foundation.  This file path is passed to the Bingo Protocol executable extension to provide extended (protocol config) information related to the theme.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "4.4.0.7")]
    [Serializable]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType=true, Namespace="F2XCdsBingoRegistryVer1.xsd")]
    public partial class CdsBingoRegistryExtendedDataFile
    {
        #region Private fields
        private string tagField;
        private string valueField;
        #endregion
        
        /// <summary>
        /// Custom theme related data string.  Passed to the Bingo Protocol executable extension.  Opaque to the Foundation.
        /// </summary>
        [XmlAttribute(Form=System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string Tag
        {
            get
            {
                return tagField;
            }
            set
            {
                tagField = value;
            }
        }
        
        [XmlTextAttribute(DataType="anyURI")]
        public string Value
        {
            get
            {
                return valueField;
            }
            set
            {
                valueField = value;
            }
        }
    }
}
#pragma warning restore
