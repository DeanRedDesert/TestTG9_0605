//-----------------------------------------------------------------------
// <copyright file = "Playlists.Extension.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization
{
    using System;
    using System.IO;
    using System.Xml.Serialization;

    public partial class Playlists
    {
        /// <summary>
        /// Loads a playlist file from disk.
        /// </summary>
        /// <param name="gameMountPoint">The mount point of the game.</param>
        /// <param name="filename">The path to the file to load.</param>
        /// <returns>The deserialized Playlists object.</returns>
        /// <exception cref="System.IO.FileNotFoundException">
        /// Thrown if the file path in <paramref name="filename"/> cannot be found on disk.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="gameMountPoint"/> is null.
        /// </exception>
        public static Playlists Load(string gameMountPoint, string filename)
        {
            if(gameMountPoint == null)
            {
                throw new ArgumentNullException(nameof(gameMountPoint));
            }

            if(!Path.IsPathRooted(filename))
            {
                filename = Path.Combine(gameMountPoint, filename);
            }

            if(!File.Exists(filename))
            {
                throw new FileNotFoundException("Unable to find playlist file.", filename);
            }

            Playlists playlistObject;

            var serializer = new XmlSerializer(typeof(Playlists));
            using(var reader = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                playlistObject = serializer.Deserialize(reader) as Playlists;
            }

            if(playlistObject?.Playlist != null)
            {
                foreach(var playlist in playlistObject.Playlist)
                {
                    if(playlist.Items != null)
                    {
                        foreach(var playlistEntry in playlist.Items)
                        {
                            playlistEntry.Initialize(gameMountPoint);
                        }
                    }
                }
            }

            return playlistObject;
        }
    }
}
