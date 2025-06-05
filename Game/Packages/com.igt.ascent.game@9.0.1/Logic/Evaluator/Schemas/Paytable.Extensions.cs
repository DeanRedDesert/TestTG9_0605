//-----------------------------------------------------------------------
// <copyright file = "Paytable.Extensions.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;

    /// <summary>
    /// Extensions to the paytable classes.
    /// </summary>
    public partial class Paytable
    {
        /// <summary>
        /// Object for parsing paytable files.
        /// </summary>
        private static XmlSerializer paytableSerializer;

        /// <summary>
        /// Object to synchronize the access to <see cref="paytableSerializer"/>.
        /// </summary>
        private static readonly object PaytableSerializerLocker = new object();

        [NonSerialized]
        private List<PaytableSection> paytableSections;

        /// <summary>
        /// Gets a list containing all of the paytable sections in this paytable.
        /// </summary>
        [XmlIgnore]
        public IList<PaytableSection> PaytableSections
        {
            get
            {
                return paytableSections ??
                    (paytableSections =
                    SlotPaytableSection.OfType<PaytableSection>().Concat(
                    PickPaytableSection.OfType<PaytableSection>()).Concat(
                    PokerPaytableSection.OfType<PaytableSection>()).Concat(
                    KenoPaytableSection.OfType<PaytableSection>()).ToList());
            }
        }

        /// <summary>
        /// Object for parsing paytable files.
        /// </summary>
        private static XmlSerializer PaytableSerializer
        {
            get
            {
                if(paytableSerializer == null)
                {
                    lock(PaytableSerializerLocker)
                    {
                        if(paytableSerializer == null)
                        {
                            paytableSerializer = new XmlSerializer(typeof(Paytable));
                        }
                    }
                }
                return paytableSerializer;
            }
        }

        /// <summary>
        /// The default constructor, required for serialization.
        /// </summary>
        public Paytable()
        {
            Abstract = new PaytableAbstract();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Paytable"/> class with the given game ID.
        /// </summary>
        /// <param name="gameId">A <see cref="string"/> containing the game ID.</param>
        public Paytable(string gameId) : this()
        {
            Abstract.gameID = gameId;
        }

        /// <summary>
        /// Get the paytable section associated with the given name.
        /// </summary>
        /// <param name="sectionName">Name of the paytable section to get.</param>
        /// <returns>The requested paytable section.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the requested section is not in the paytable.</exception>
        public PaytableSection GetPaytableSection(string sectionName)
        {
            var returnSection =
                (from section in PaytableSections where section.name == sectionName select section).FirstOrDefault();

            if (returnSection == null)
            {
                throw new KeyNotFoundException("The section: " + sectionName + " could not be found in paytable: " +
                                               Abstract.gameID);
            }

            return returnSection;
        }

        /// <summary>
        /// Get the paytable section associated with the given name.
        /// </summary>
        /// <typeparam name="TSection">Type of section to get.</typeparam>
        /// <param name="sectionName">Name of section.</param>
        /// <returns>The requested paytable section.</returns>
        /// <exception cref="InvalidSectionTypeException">
        /// Thrown when the requested section does not match the desired type.
        /// </exception>
        public TSection GetPaytableSection<TSection>(string sectionName) where TSection : PaytableSection
        {
            var retrievedSection = GetPaytableSection(sectionName);
            var returnSection = retrievedSection as TSection;

            if (returnSection == null)
            {
                throw new InvalidSectionTypeException("The requested paytable section: " + sectionName +
                                                      " is not of requested type " + typeof(TSection) + " the type is " +
                                                      retrievedSection.GetType());
            }

            return returnSection;
        }

        /// <summary>
        /// Gets the paytable section that corresponds to the given feature index.
        /// </summary>
        /// <param name="featureIndex">A feature index from this paytable.</param>
        /// <returns>The <see cref="PaytableSection"/> with the given feature index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the feature index is greater than or equal to the number of paytable sections.
        /// </exception>
        public PaytableSection GetPaytableSection(uint featureIndex)
        {
            if (featureIndex >= PaytableSections.Count)
            {
                var message = string.Format("Index must be less than {0}.", PaytableSections.Count);
                throw new ArgumentOutOfRangeException("featureIndex", featureIndex, message);
            }
            return PaytableSections[(int)featureIndex];
        }

        /// <summary>
        /// Gets the paytable section associated with the given feature index.
        /// </summary>
        /// <typeparam name="TSection">The type of section to get.</typeparam>
        /// <param name="featureIndex">A feature index from this paytable.</param>
        /// <returns>The requested paytable section.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the feature index is greater than or equal to the number of paytable sections.
        /// </exception>
        /// <exception cref="InvalidSectionTypeException">
        /// Thrown when the requested section does not match the desired type.
        /// </exception>
        public TSection GetPaytableSection<TSection>(uint featureIndex) where TSection : PaytableSection
        {
            var retrievedSection = GetPaytableSection(featureIndex);
            var returnSection = retrievedSection as TSection;

            if (returnSection == null)
            {
                throw new InvalidSectionTypeException("The paytable section for feature index: " + featureIndex +
                                                      " is not of requested type " + typeof(TSection) + " the type is " +
                                                      retrievedSection.GetType());
            }
            return returnSection;
        }

        /// <summary>
        /// Gets the Feature Index of a paytable section.
        /// </summary>
        /// <param name="paytableSection">Section in the paytable.</param>
        /// <returns>
        /// Feature index of the paytable section.
        /// </returns>
        /// <remarks>KeyNotFoundException exception will be thrown if section does not exist in paytable.</remarks>
        public uint GetFeatureIndex(string paytableSection)
        {
            var section = GetPaytableSection(paytableSection);

            var featureIndex = (uint)PaytableSections.IndexOf(section);
            return featureIndex;
        }

        /// <summary>
        /// Load paytable from an xml paytable file.
        /// </summary>
        /// <remarks>
        /// This implementation assumes that there is only one paytable
        /// per paytable file.
        /// </remarks>
        /// <param name="paytableFile">
        /// The name of the xml paytable file, including the absolute
        /// or relative path, and the full file name with extension.
        /// </param>
        /// <returns>Paytable loaded.</returns>
        public static Paytable Load(string paytableFile)
        {
            Paytable result;
            try
            {
                using(var stream = new FileStream(paytableFile, FileMode.Open, FileAccess.Read))
                {
                    result = PaytableSerializer.Deserialize(stream) as Paytable;
                }
            }
            catch(FileNotFoundException)
            {
                // Don't wrap FileNotFoundExceptions so clients can differentiate between this and
                // a PaytableLoadException.
                throw;
            }
            catch(Exception exception)
            {
                throw new PaytableLoadException(paytableFile, exception);
            }
            return result;
        }
    }
}
