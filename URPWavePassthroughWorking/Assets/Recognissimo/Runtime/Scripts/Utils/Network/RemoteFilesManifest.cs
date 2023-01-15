using System;
using System.Collections.Generic;
using System.Linq;

namespace Recognissimo.Utils.Network
{
    /// <summary>
    ///     Struct representing a remote file.
    /// </summary>
    [Serializable]
    public struct RemoteFile
    {
        /// <summary>
        ///     URL to the remote file
        /// </summary>
        public string url;

        /// <summary>
        ///     Remote file version used for file caching.
        ///     Files with the same URL but with different versions are treated as different files.
        ///     Null value makes file not cacheable.
        /// </summary>
        public string version;

        public RemoteFile(string url)
        {
            this.url = url;
            version = null;
        }

        public RemoteFile(string url, string version)
        {
            this.url = url;
            this.version = version;
        }

        public readonly bool Equals(RemoteFile other)
        {
            return version != null && url == other.url && version == other.version;
        }
    }

    /// <summary>
    ///     Struct representing a list of <see cref="RemoteFile" />s.
    /// </summary>
    [Serializable]
    public struct RemoteFilesManifest
    {
        public List<RemoteFile> content;

        public readonly bool Equals(RemoteFilesManifest other)
        {
            if (content == other.content)
            {
                return true;
            }

            if (content != null && other.content != null)
            {
                return content.SequenceEqual(other.content);
            }

            return false;
        }

        public readonly bool TryFindByUrl(string url, out RemoteFile remoteFile)
        {
            if (content == null)
            {
                remoteFile = default;
                return false;
            }

            remoteFile = content.SingleOrDefault(item => item.url == url);
            return !remoteFile.Equals(default);
        }
    }
}