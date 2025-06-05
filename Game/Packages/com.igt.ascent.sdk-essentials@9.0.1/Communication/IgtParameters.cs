//-----------------------------------------------------------------------
// <copyright file = "IgtParameters.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using Logging;

    /// <summary>
    /// Abstract base class for that can serialize and deserialize parameter information
    /// for executables (ex. game, extension, etc).
    /// </summary>
    /// <remarks>
    /// This class uses a custom file format for saving the parameters. The custom format is used to reduce the
    /// number of dependencies this class has to only items available from the system assembly. Formats such as XML
    /// or JSON would introduce additional assembly dependencies.
    /// </remarks>
    [Serializable]
    public abstract class IgtParameters
    {
        #region Protected Nested Types

        /// <summary>
        /// Interface for type handlers. Type handlers manage the string serialization and deserialization of
        /// parameters elements.
        /// </summary>
        protected interface ITypeHandler
        {
            /// <summary>
            /// Function for deserializing an object from a string.
            /// </summary>
            Action<string> ReadString { get; }

            /// <summary>
            /// Function for serializing an object to a string.
            /// </summary>
            Func<string> WriteString { get; }

            /// <summary>
            /// Comment to place above the setting.
            /// </summary>
            string Comment { get; }
        }

        /// <summary>
        /// Class which manages the string serialization and deserialization of parameters fields.
        /// </summary>
        protected class TypeHandler : ITypeHandler
        {
            /// <summary>
            /// Construct an instance of the handler with the given write string and read string functions.
            /// </summary>
            /// <param name="readString">Method for reading the object from a string.</param>
            /// <param name="writeString">Method for writing the object to a string.</param>
            /// <param name="comment">Comment for the type.</param>
            public TypeHandler(Action<string> readString, Func<string> writeString, string comment)
            {
                ReadString = readString;
                WriteString = writeString;
                Comment = comment;
            }

            /// <inheritdoc />
            public Action<string> ReadString { get; }

            /// <inheritdoc />
            public Func<string> WriteString { get; }

            /// <inheritdoc />
            public string Comment { get; }
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// String constant for comment lines.
        /// </summary>
        private const string CommentStart = "//";

        /// <summary>
        /// Format to use when writing to the file.
        /// </summary>
        private const string WriteStringFormat = "\"{0}\" : \"{1}\"";

        /// <summary>
        /// Characters to trim from fields.
        /// </summary>
        private readonly char[] trimChars = {'\"', ' '};

        /// <summary>
        /// Default target to use for new parameters files or during upgrades.
        /// </summary>
        /// <devdoc>
        /// There is no <see cref="FoundationTarget"/> parameter member because derived
        /// classes may prefer fields (Unity Inspector) or properties (C# style).
        /// </devdoc>
        private readonly FoundationTarget defaultTarget;

        #endregion

        #region Protected Fields

        /// <summary>
        /// List of the supported types in the file and the methods for serializing and deserializing them.
        /// </summary>
        protected Dictionary<string, ITypeHandler> FileContent;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of <see cref="IgtParameters"/>.
        /// </summary>
        /// <param name="defaultTarget">
        /// The default <see cref="FoundationTarget"/> that will be used
        /// for new parameter files or during upgrades.
        /// </param>
        protected IgtParameters(FoundationTarget defaultTarget)
        {
            this.defaultTarget = defaultTarget;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads parameters from the given file.
        /// </summary>
        /// <param name="fileName">The name of the file to load.</param>
        /// <exception cref="ArgumentNullException">Thrown if the passed file name is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the passed file name is an empty path.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the parameters file cannot be found.</exception>
        /// <exception cref="IOException">Thrown if the parameters file cannot be loaded for any reason.</exception>
        public void Load(string fileName)
        {
            if(fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if(fileName == string.Empty)
            {
                throw new ArgumentException("Cannot load parameters from an empty path.", nameof(fileName));
            }

            try
            {
                using(var fileStream = new StreamReader(fileName))
                {
                    Load(fileStream);
                }
            }
            catch(FileNotFoundException fileNotFoundException)
            {
                throw new FileNotFoundException("The parameters file could not be found: " + fileName,
                                                fileNotFoundException);
            }
            catch(Exception exception)
            {
                throw new IOException("The parameters file could not be loaded: " + fileName,
                                      exception);
            }
        }

        /// <summary>
        /// Loads parameters from a <see cref="TextReader"/>.
        /// </summary>
        /// <param name="reader">TextReader to read parameters from.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="reader"/> is null.
        /// </exception>
        /// <exception cref="SerializationException">
        /// Thrown if the parameters object cannot be deserialized from <paramref name="reader"/>.
        /// </exception>
        public void Load(TextReader reader)
        {
            if(reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            string readLine;
            var exceptionString = "";

            while((readLine = reader.ReadLine()) != null && string.IsNullOrEmpty(exceptionString))
            {
                //Skip blank lines and comments
                if(readLine == string.Empty || readLine.StartsWith(CommentStart))
                {
                    continue;
                }

                var split = readLine.Split(new[] { ':' }, 2, StringSplitOptions.None);

                if(split.Length == 2)
                {
                    var fieldKey = split[0].Trim(trimChars);

                    if(FileContent.ContainsKey(fieldKey))
                    {
                        try
                        {
                            FileContent[fieldKey].ReadString(split[1].Trim(trimChars));
                        }
                        catch(Exception exception)
                        {
                            exceptionString = $"Error loading field: \"{fieldKey}\" Reason: {exception}";
                        }
                    }
                    else
                    {
                        exceptionString = $"Field: \"{fieldKey}\" is not a valid field for the parameters file.";
                    }
                }
                else
                {
                    exceptionString = string.Format(
                        "Parameters must be formatted as " + WriteStringFormat + ".",
                        "<field>",
                        "<value>");
                }
            }

            if(!string.IsNullOrEmpty(exceptionString))
            {
                throw new SerializationException(exceptionString);
            }
        }

        /// <summary>
        /// Saves parameters while retaining comments using the given file path.
        /// </summary>
        /// <param name="fileName">The name of the file including the desired path.</param>
        /// <exception cref="ArgumentNullException">Thrown if the passed file name is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the passed file name is an empty path.</exception>
        /// <exception cref="IOException">Thrown if the parameters file cannot be saved for any reason.</exception>
        public void Save(string fileName)
        {
            if(fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if(fileName == string.Empty)
            {
                throw new ArgumentException("Cannot save parameters file to an empty path.", nameof(fileName));
            }

            try
            {
                using(var file = new StreamWriter(fileName))
                {
                    Save(file, true);
                }
            }
            catch(Exception exception)
            {
                throw new IOException("The parameters file could not be saved: " + fileName,
                                      exception);
            }
        }

        /// <summary>
        /// Saves parameters to a TextWriter.
        /// </summary>
        /// <param name="writer">The text writer to write the parameters to.</param>
        /// <param name="includeComments">Flag indicating if comments should be written for each parameter.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="writer"/> is null.
        /// </exception>
        public void Save(TextWriter writer, bool includeComments)
        {
            if(writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            foreach(var field in FileContent)
            {
                if(includeComments)
                {
                    writer.WriteLine(CommentStart + field.Value.Comment);
                }
                writer.WriteLine(WriteStringFormat, field.Key, field.Value.WriteString());
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Parses a string containing a <see cref="FoundationTarget"/> and returns a <see cref="FoundationTarget"/>
        /// value.
        /// </summary>
        /// <param name="targetString">String containing a <see cref="FoundationTarget"/>.</param>
        /// <returns>A <see cref="FoundationTarget"/> represented by the string.</returns>
        /// <remarks>
        /// If the <paramref name="targetString"/> cannot be parsed, a default <see cref="FoundationTarget"/>
        /// value is return.
        /// </remarks>
        protected FoundationTarget ParseTarget(string targetString)
        {
            try
            {
                var target = (FoundationTarget)Enum.Parse(typeof(FoundationTarget), targetString);
                return target;
            }
            catch(Exception)
            {
                Log.WriteWarning("Upgrading to valid target. Old: " + targetString + " New: " +
                                 Enum.GetName(typeof(FoundationTarget), defaultTarget));

                return defaultTarget;
            }
        }

        #endregion
    }
}