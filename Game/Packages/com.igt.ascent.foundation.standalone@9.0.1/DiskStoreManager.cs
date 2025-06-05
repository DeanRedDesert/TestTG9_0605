//-----------------------------------------------------------------------
// <copyright file = "DiskStoreManager.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System.IO;

    /// <summary>
    /// XML file back implementation of a DiskStore.
    /// </summary>
    internal class DiskStoreManager : DiskStoreManagerBase
    {
        #region Constructors

        /// <summary>
        /// Parameterless constructor creates a DiskStoreManager
        /// which is not file backed. Data saved in such a disk store
        /// will not persist between game launches.
        /// </summary>
        public DiskStoreManager()
        {
        }

        /// <summary>
        /// File backed storage DiskStoreManager which provides basic safe
        /// storage. This storage does not guarantee data consistency.
        /// </summary>
        /// <param name="modifierPath">The modifier path to use.</param>
        /// <param name="committedPath">The committed path to use.</param>
        public DiskStoreManager(string modifierPath, string committedPath)
        {
            ModifierPath = modifierPath;
            CommittedPath = committedPath;

            using(var committedStream = new FileStream(committedPath, FileMode.OpenOrCreate))
            using(var modifierStream = new FileStream(modifierPath, FileMode.OpenOrCreate))
            {

                try
                {
                    TextReader committedReader = new StreamReader(committedStream);
                    ModifierStore = DiskStore.Load(committedReader);
                }
                catch
                {
                    try
                    {
                        // In order to support cumulative changes, usually modifier
                        // will be to read.
                        // This reads the modifier in case there was a failure
                        // swapping the buffers.
                        TextReader modifierReader = new StreamReader(modifierStream);
                        ModifierStore = DiskStore.Load(modifierReader);
                    }
                    catch
                    {
                        //Neither committed or the modifier could be read.
                        //Assume a cold start, the standalone GameLib doesn't
                        //need to ensure recovery.
                    }
                }
            }

            FileBackedStore = true;
        }

        #endregion

        /// <inheritdoc />
        public override void Commit()
        {
            if(FileBackedStore)
            {
                //Write the modifier store to the modifier file.
                using(var modifierStream = new FileStream(ModifierPath, FileMode.Truncate))
                {
                    modifierStream.Seek(0, SeekOrigin.Begin);

                    //Save the modifier
                    TextWriter textWriter = new StreamWriter(modifierStream);
                    DiskStore.Save(textWriter, ModifierStore);
                }

                //Copy the modifier file to the committed file.
                File.Copy(ModifierPath, CommittedPath, true);
            }

            SendTransactionCommittedEvent();
        }
    }
}