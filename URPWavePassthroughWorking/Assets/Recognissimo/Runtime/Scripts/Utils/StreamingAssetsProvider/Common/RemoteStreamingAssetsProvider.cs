using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Recognissimo.Utils.Network;

namespace Recognissimo.Utils.StreamingAssetsProvider.Common
{
    /// <summary>
    ///     Helper class to provide StreamingAssets files from the remote host.
    /// </summary>
    public class RemoteStreamingAssetsProvider : IStreamingAssetsProvider
    {
        public const string StreamingAssetsManifestName = "StreamingAssetsManifest.json";
        private const string ReplicatedStreamingAssetsDirectoryName = "StreamingAssetsReplica";

        private readonly DownloadManager _downloadManager;

        private readonly HashSet<string> _populatedEntries = new();
        private readonly string _remoteStreamingAssetsPath;
        private readonly string _replicatedStreamingAssetsPath;

        private RemoteFilesManifest _manifest;

        /// <summary>
        ///     Create a new instance of <see cref="RemoteStreamingAssetsProvider" />.
        /// </summary>
        /// <param name="remoteStreamingAssetsPath">Path to the remote StreamingAssets.</param>
        /// <param name="persistentStoragePath">Path to the persistent storage where downloads will be stored.</param>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="remoteStreamingAssetsPath" /> or
        ///     <paramref name="persistentStoragePath" /> is null or empty.
        /// </exception>
        public RemoteStreamingAssetsProvider(string remoteStreamingAssetsPath, string persistentStoragePath)
        {
            if (string.IsNullOrEmpty(remoteStreamingAssetsPath))
            {
                throw new ArgumentNullException(nameof(remoteStreamingAssetsPath));
            }

            if (string.IsNullOrEmpty(persistentStoragePath))
            {
                throw new ArgumentNullException(nameof(persistentStoragePath));
            }

            _remoteStreamingAssetsPath = remoteStreamingAssetsPath;

            _replicatedStreamingAssetsPath =
                Path.Combine(persistentStoragePath, ReplicatedStreamingAssetsDirectoryName);

            if (!Directory.Exists(_replicatedStreamingAssetsPath))
            {
                Directory.CreateDirectory(_replicatedStreamingAssetsPath);
            }

            _downloadManager = new DownloadManager(_replicatedStreamingAssetsPath);
        }

        /// <inheritdoc />
        public IEnumerator Initialize()
        {
            var manifestUrl = Path.Combine(_remoteStreamingAssetsPath, StreamingAssetsManifestName);

            yield return RemoteFilesManifestDownloader.Download(manifestUrl, manifest => _manifest = manifest);

            _downloadManager.RemoveDownloadsExcept(_manifest.content.Select(remote => new RemoteFile
                {url = Path.Combine(_remoteStreamingAssetsPath, remote.url), version = remote.version}));
        }

        /// <inheritdoc />
        public IEnumerator Populate(string streamingAssetsRelativePath, StreamingAssetsProvisionFailedCallback callback)
        {
            if (_populatedEntries.Contains(streamingAssetsRelativePath))
            {
                yield break;
            }

            if (_manifest.Equals(default))
            {
                callback(new InvalidOperationException("List of remote files not loaded or empty."));
                yield break;
            }

            var streamingAssetsItems =
                _manifest.content
                    .Where(remote => remote.url.StartsWith(streamingAssetsRelativePath));

            var success = true;

            void DownloadFailed(string reason)
            {
                success = false;
                callback(new IOException(reason));
            }

            foreach (var streamingAssetsItem in streamingAssetsItems)
            {
                var url = Path.Combine(_remoteStreamingAssetsPath, streamingAssetsItem.url);

                var remote = new RemoteFile {url = url, version = streamingAssetsItem.version};

                var prefix = Path.GetDirectoryName(streamingAssetsItem.url);

                yield return _downloadManager.DownloadFile(remote,
                    DownloadFailed, prefix);

                if (!success)
                {
                    yield break;
                }
            }

            _populatedEntries.Add(streamingAssetsRelativePath);
        }

        /// <inheritdoc />
        public string Provide(string streamingAssetsRelativePath, StreamingAssetsProvisionFailedCallback callback)
        {
            return Path.Combine(_replicatedStreamingAssetsPath, streamingAssetsRelativePath);
        }
    }
}