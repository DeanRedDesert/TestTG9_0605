//-----------------------------------------------------------------------
// <copyright file = "PaytableTransformer.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Resources;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Xsl;
    using Ascent.Build.EmbeddedResources;

    /// <summary>
    /// Uses XSLT scripts to transform a paytable document from one version to another.
    /// </summary>
    public class PaytableTransformer
    {
        /// <summary>
        /// A regular expression that will match any .xslt files stored in the Transforms resource folder.
        /// </summary>
        /// <example>
        /// If a transformation is defined from Ver1 to Ver2, it must be named "MigrateFromVer1ToVer2.xslt",
        /// and it must be placed in the Transforms folder and marked as an embedded resource.
        /// </example>
        private const string XsltPatternFormat = @".*\.Transforms\.MigrateFrom{0}To{1}\.xslt$";

        private const string CouldNotLocateTransformWithPatternMessageFormat = "Could not locate transform with pattern {0}";
        private const string ResourceStreamWasNullMessage = "The resource stream was null.";

        private readonly IList<TransformationStep> transformChain;
        
        private readonly Assembly resourceAssembly;

        /// <summary>
        /// Gets the <see cref="PaytableVersion"/> that this transformer will transform paytables from.
        /// </summary>
        public PaytableVersion FromVersion { get; }

        /// <summary>
        /// Gets the <see cref="PaytableVersion"/> that this transformer will transform paytables to.
        /// </summary>
        public PaytableVersion ToVersion { get; }

        /// <summary>
        /// Initializes an instance of the <see cref="PaytableTransformer"/> class with the given from and to
        /// versions using the default XSLT scripts.
        /// </summary>
        /// <param name="fromVersion">The <see cref="PaytableVersion"/> to transform paytables from.</param>
        /// <param name="toVersion">The <see cref="PaytableVersion"/> to transform paytables to.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="fromVersion"/> is equal to <paramref name="toVersion"/>.
        /// </exception>
        public PaytableTransformer(PaytableVersion fromVersion, PaytableVersion toVersion)
        {
            if(fromVersion == toVersion)
            {
                throw new ArgumentException("The fromVersion must be different from the toVersion.");
            }
            FromVersion = fromVersion;
            ToVersion = toVersion;
            resourceAssembly = ResourcesAssembly.GetCurrent();

            transformChain = new List<TransformationStep>();
            InitializeTransformChain();
        }

        /// <summary>
        /// Initializes an instance of the <see cref="PaytableTransformer"/> class with the given from and to
        /// versions.
        /// </summary>
        /// <param name="fromVersion">The <see cref="PaytableVersion"/> to transform paytables from.</param>
        /// <param name="toVersion">The <see cref="PaytableVersion"/> to transform paytables to.</param>
        /// <param name="resourceAssembly">The <see cref="Assembly"/> that contains the XSLT scripts.</param>
        /// <param name="unused">No longer used.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="fromVersion"/> is equal to <paramref name="toVersion"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="resourceAssembly"/> is null.
        /// </exception>
        [Obsolete("This constructor is deprecated and will be removed in SDK 7.1.0. " +
                  "Please call PaytableTransformer(PaytableVersion, PaytableVersion) instead.")]
        public PaytableTransformer(PaytableVersion fromVersion, PaytableVersion toVersion, Assembly resourceAssembly,
            // ReSharper disable once UnusedParameter.Local
            Type unused)
        {
            if(fromVersion == toVersion)
            {
                throw new ArgumentException("The fromVersion must be different from the toVersion.");
            }
            if(resourceAssembly == null)
            {
                throw new ArgumentNullException(nameof(resourceAssembly));
            }
            FromVersion = fromVersion;
            ToVersion = toVersion;
            this.resourceAssembly = resourceAssembly;
            transformChain = new List<TransformationStep>();
            InitializeTransformChain();
        }

        /// <summary>
        /// Transforms the given paytable document from the <see cref="FromVersion"/> to the <see cref="ToVersion"/>.
        /// </summary>
        /// <param name="paytableDocument">An <see cref="XDocument"/> containing the paytable to transform.</param>
        /// <returns>An <see cref="XDocument"/> containing the transformed document.</returns>
        public XDocument Transform(XDocument paytableDocument)
        {
            var readerDocument = new XDocument(paytableDocument);
            foreach(var transformationStep in transformChain)
            {
                var writerDocument = new XDocument();
                using(var reader = readerDocument.CreateReader())
                {
                    using(var writer = writerDocument.CreateWriter())
                    {
                        try
                        {
                            transformationStep.Apply(reader, writer);
                        }
                        catch(Exception exception)
                        {
                            throw new PaytableTransformationException(
                                transformationStep.FromVersion, transformationStep.ToVersion, exception);
                        }
                    }
                }
                readerDocument = writerDocument;
            }
            return readerDocument;
        }

        /// <summary>
        /// Initializes the transform chain with the <see cref="XslCompiledTransform"/> objects required to transform
        /// paytable files from the <see cref="FromVersion"/> to the <see cref="ToVersion"/>.
        /// </summary>
        private void InitializeTransformChain()
        {
            var initialVersionIndex = (int)FromVersion;
            var finalVersionIndex = (int)ToVersion;

            if(initialVersionIndex < finalVersionIndex)
            {
                BuildForwardTransformChain(initialVersionIndex, finalVersionIndex);
            }
            else
            {
                BuildReverseTransformChain(initialVersionIndex, finalVersionIndex);
            }
        }

        /// <summary>
        /// Builds the transform chain for a forward version transform.
        /// </summary>
        /// <param name="initialVersionIndex">The first 0-based index of the transform chain.</param>
        /// <param name="finalVersionIndex">The last 0-based index of the transform chain.</param>
        private void BuildForwardTransformChain(int initialVersionIndex, int finalVersionIndex)
        {
            for(var transformIndex = initialVersionIndex; transformIndex < finalVersionIndex; transformIndex++)
            {
                var fromVersion = (PaytableVersion)transformIndex;
                var toVersion = (PaytableVersion)(transformIndex + 1);
                var xsltProcessor = LoadXsltProcessor(fromVersion, toVersion, resourceAssembly);
                var transformStep = new TransformationStep(fromVersion, toVersion, xsltProcessor);
                transformChain.Add(transformStep);
            }
        }

        /// <summary>
        /// Builds the transform chain for a reverse version transform.
        /// </summary>
        /// <param name="initialVersionIndex">The first 0-based index of the transform chain.</param>
        /// <param name="finalVersionIndex">The last 0-based index of the transform chain.</param>
        private void BuildReverseTransformChain(int initialVersionIndex, int finalVersionIndex)
        {
            for(var transformIndex = initialVersionIndex; transformIndex > finalVersionIndex; transformIndex--)
            {
                var fromVersion = (PaytableVersion)transformIndex;
                var toVersion = (PaytableVersion)(transformIndex - 1);
                var xsltProcessor = LoadXsltProcessor(fromVersion, toVersion, resourceAssembly);
                var transformStep = new TransformationStep(fromVersion, toVersion, xsltProcessor);
                transformChain.Add(transformStep);
            }
        }

        /// <summary>
        /// Loads an XSLT processor that can migrate paytables from one version to another, from a string constant on
        /// the given embedded resources type.
        /// </summary>
        /// <param name="fromVersion">The version to migrate from.</param>
        /// <param name="toVersion">The version to migrate to.</param>
        /// <param name="embeddedResources">The type containing the string resources.</param>
        /// <returns>A <see cref="XslCompiledTransform"/> object containing the loaded script.</returns>
        /// <exception cref="MissingManifestResourceException">
        /// Thrown if the embedded resources type does not contain a string constant for the given versions.
        /// </exception>
        /// TODO: Unused private method
        // ReSharper disable once UnusedMember.Local
        private static XslCompiledTransform LoadXsltProcessor(PaytableVersion fromVersion, PaytableVersion toVersion,
            Type embeddedResources)
        {
            var resourceName = $"MigrateFrom{fromVersion}To{toVersion}Transforms";
            var resourceFieldInfo = embeddedResources.GetFields()
                .SingleOrDefault(x => string.CompareOrdinal(resourceName, x.Name) == 0);
            var resourceString = resourceFieldInfo?.GetRawConstantValue() as string;
            if(string.IsNullOrEmpty(resourceString))
            {
                throw new MissingManifestResourceException(
                    $"Could not locate embedded resource named {resourceName} on type {embeddedResources}");
            }

            using(var reader = XmlReader.Create(new StringReader(resourceString)))
            {
                var processor = new XslCompiledTransform();
                processor.Load(reader);
                return processor;
            }
        }

        /// <summary>
        /// Loads an XSLT processor that can migrate paytables from one version to another, from an embedded resource.
        /// </summary>
        /// <param name="resourceAssembly">
        /// The <see cref="Assembly"/> containing the XSLT script as an embedded resource.
        /// </param>
        /// <param name="fromVersion">The paytable version to migrate from.</param>
        /// <param name="toVersion">The paytable version to migrate to.</param>
        /// <returns>A <see cref="XslCompiledTransform"/> object containing the loaded script.</returns>
        /// <exception cref="MissingManifestResourceException">
        /// Thrown if the assembly does not contain an embedded resource for the given versions.
        /// </exception>
        private static XslCompiledTransform LoadXsltProcessor(PaytableVersion fromVersion, PaytableVersion toVersion, Assembly resourceAssembly)
        {
            var regexPattern = string.Format(
                CultureInfo.InvariantCulture,
                XsltPatternFormat,
                fromVersion,
                toVersion);
            var transformName = resourceAssembly.GetMatchingResourceNames(regexPattern).SingleOrDefault();

            if(transformName == null)
            {
                var message = string.Format(
                    CultureInfo.InvariantCulture, CouldNotLocateTransformWithPatternMessageFormat, regexPattern);
                throw new MissingManifestResourceException(message);
            }

            using(var transformStream = resourceAssembly.GetManifestResourceStream(transformName))
            {
                if(transformStream == null)
                {
                    throw new MissingManifestResourceException(ResourceStreamWasNullMessage);
                }
                var processor = new XslCompiledTransform();
                var reader = XmlReader.Create(transformStream);
                processor.Load(reader);
                return processor;
            }
        }

        /// <summary>
        /// Represents a single step in a chain of transformations.
        /// </summary>
        private class TransformationStep
        {
            private readonly XslCompiledTransform xsltProcessor;

            /// <summary>
            /// Gets the <see cref="PaytableVersion"/> being transformed from in this step.
            /// </summary>
            internal PaytableVersion FromVersion { get; }
            
            /// <summary>
            /// Gets the <see cref="PaytableVersion"/> being transformed to in this step.
            /// </summary>
            internal PaytableVersion ToVersion { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="TransformationStep"/> with the given to and from
            /// versions and the given <see cref="XslCompiledTransform"/> object.
            /// </summary>
            /// <param name="fromVersion">The <see cref="PaytableVersion"/> to transform from.</param>
            /// <param name="toVersion">The <see cref="PaytableVersion"/> to transform to.</param>
            /// <param name="xsltProcessor"></param>
            internal TransformationStep(PaytableVersion fromVersion, PaytableVersion toVersion, XslCompiledTransform xsltProcessor)
            {
                this.xsltProcessor = xsltProcessor;
                FromVersion = fromVersion;
                ToVersion = toVersion;
            }

            /// <summary>
            /// Applies this step of the transformation.
            /// </summary>
            /// <param name="reader">An <see cref="XmlReader"/> for the input to the transformation step.</param>
            /// <param name="writer">An <see cref="XmlWriter"/> for the output of the transformation step.</param>
            internal void Apply(XmlReader reader, XmlWriter writer)
            {
                xsltProcessor.Transform(reader, writer);
            }
        }
    }
}
