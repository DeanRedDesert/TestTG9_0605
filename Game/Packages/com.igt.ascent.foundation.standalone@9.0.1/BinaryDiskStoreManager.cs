// -----------------------------------------------------------------------
// <copyright file = "BinaryDiskStoreManager.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// Binary file backed manager for a DiskStore.
    /// </summary>
    internal class BinaryDiskStoreManager : DiskStoreManagerBase
    {
        #region Constructors

        /// <summary>
        /// Parameterless constructor creates a BinaryDiskStoreManager
        /// which is not file backed. Data saved in such a disk store
        /// will not persist between game launches.
        /// </summary>
        public BinaryDiskStoreManager()
        {
        }

        /// <summary>
        /// File backed storage BinaryDiskStoreManager which provides basic safe
        /// storage. This storage does not guarantee data consistency.
        /// </summary>
        /// <param name="modifierPath">The modifier path to use.</param>
        /// <param name="committedPath">The committed path to use.</param>
        public BinaryDiskStoreManager(string modifierPath, string committedPath)
        {
            ModifierPath = modifierPath;
            CommittedPath = committedPath;

            using(var committedStream = new FileStream(committedPath, FileMode.OpenOrCreate))
            using(var modifierStream = new FileStream(modifierPath, FileMode.OpenOrCreate))
            {
                try
                {
                    ModifierStore = CompactSerializer.Deserialize<DiskStore>(committedStream);
                }
                catch
                {
                    try
                    {
                        // In order to support cumulative changes, usually modifier
                        // will be to read.
                        // This reads the modifier in case there was a failure
                        // swapping the buffers.
                        ModifierStore = CompactSerializer.Deserialize<DiskStore>(modifierStream);
                    }
                    catch
                    {
                        //Neither committed or the modifier could be read.
                        //Assume a cold start, the standalone GameLib doesn't
                        //need to ensure recovery.
                    }
                }
            }

            // If the modifier or committed paths are both empty start from a new DiskStore.
            if(ModifierStore == null)
            {
                ModifierStore = new DiskStore();
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
                    CompactSerializer.Serialize(modifierStream, ModifierStore);
                    //modifierStore.Serialize(modifierStream);
                }

                //Copy the modifier file to the committed file.
                File.Copy(ModifierPath, CommittedPath, true);
            }

            SendTransactionCommittedEvent();
        }
    }
}