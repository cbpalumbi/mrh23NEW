using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Recognissimo.Utils.Network
{
    public delegate void DownloadFailedCallback(string description);

    public class DownloadManager
    {
        private const string CacheFileName = "DownloadManagerCache.json";

        private readonly string _cacheFilePath;

        private readonly string _downloadsDirectoryPath;

        private DownloadsCache _downloadsCache;

        /// <summary>
        ///     Construct instance and specify its working directory.
        /// </summary>
        /// <param name="downloadsDirectoryPath">Directory where configuration file and downloads will be stored.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="downloadsDirectoryPath" /> is empty.</exception>
        public DownloadManager(string downloadsDirectoryPath)
        {
            if (string.IsNullOrEmpty(downloadsDirectoryPath))
            {
                throw new ArgumentNullException(nameof(downloadsDirectoryPath));
            }

            _downloadsDirectoryPath = downloadsDirectoryPath;

            if (!Directory.Exists(downloadsDirectoryPath))
            {
                Directory.CreateDirectory(downloadsDirectoryPath);
            }

            _cacheFilePath = Path.Combine(_downloadsDirectoryPath, CacheFileName);

            _downloadsCache = LoadCacheFromFile(_cacheFilePath);
        }

        /// <summary>
        ///     Download file and store it in the downloads directory at path <i>downloads/fileName</i>.
        /// </summary>
        /// <param name="remote">Remote file description.</param>
        /// <param name="downloadFailedCallback">Fail callback.</param>
        /// <param name="prefix">If specified, file will be saved at <i>downloads/prefix/fileName</i>.</param>
        /// <returns>Enumerator to run coroutine on.</returns>
        public IEnumerator DownloadFile(RemoteFile remote, DownloadFailedCallback downloadFailedCallback,
            string prefix = null)
        {
            if (IsDownloaded(remote, true))
            {
                yield break;
            }

            var fileName = Path.GetFileName(remote.url);

            var saveFilePath = Path.Combine(_downloadsDirectoryPath, prefix ?? string.Empty, fileName);

            using var downloadHandler = new DownloadHandlerFile(saveFilePath);

            downloadHandler.removeFileOnAbort = true;

            var didDownloadFail = false;

            yield return Download(remote, downloadHandler, failReason =>
            {
                didDownloadFail = true;
                downloadFailedCallback(failReason);
            });

            if (didDownloadFail)
            {
                yield break;
            }

            AddToCache(remote, saveFilePath);
        }

        /// <summary>
        ///     Download zip and extract its content into the downloads directory at path <i>downloads/archive-name</i>
        /// </summary>
        /// <param name="remote">Remote file description.</param>
        /// <param name="downloadFailedCallback">Fail callback.</param>
        /// <returns>Enumerator to run coroutine on.</returns>
        public IEnumerator DownloadAndExtractZip(RemoteFile remote, DownloadFailedCallback downloadFailedCallback)
        {
            if (IsDownloaded(remote, true))
            {
                yield break;
            }

            var fileName = Path.GetFileNameWithoutExtension(remote.url);

            var extractPath = Path.Combine(_downloadsDirectoryPath, fileName);

            using var downloadHandler = new DownloadHandlerBuffer();

            var didDownloadFail = false;

            yield return Download(remote, downloadHandler, failReason =>
            {
                didDownloadFail = true;
                downloadFailedCallback(failReason);
            });

            if (didDownloadFail)
            {
                yield break;
            }

            using var stream = new MemoryStream(downloadHandler.data);

            using var archive = new ZipArchive(stream);

            yield return ZipExtractor.Extract(archive, extractPath);

            AddToCache(remote, extractPath);
        }

        /// <summary>
        ///     Check if the remote resource is loaded.
        /// </summary>
        /// <param name="remote">Remote resource.</param>
        /// <param name="checkLocalFiles">Whether to check local files existence.</param>
        /// <returns>Boolean indicating whether the file is loaded.</returns>
        public bool IsDownloaded(RemoteFile remote, bool checkLocalFiles)
        {
            return _downloadsCache.TryGetByRemote(remote, out var downloaded)
                   && (!checkLocalFiles || File.Exists(downloaded.localPath) || Directory.Exists(downloaded.localPath));
        }

        /// <summary>
        ///     Get local path of downloaded resource.
        /// </summary>
        /// <param name="remote">Remote resource.</param>
        /// <returns>Local path of downloaded <paramref name="remote" />.</returns>
        /// <exception cref="InvalidOperationException">If <paramref name="remote" /> has not been downloaded.</exception>
        public string GetDownloadedItemPath(RemoteFile remote)
        {
            if (!_downloadsCache.TryGetByRemote(remote, out var downloadedFileDescription))
            {
                throw new InvalidOperationException("Unable to get the path to the item that has not been downloaded.");
            }

            return downloadedFileDescription.localPath;
        }

        public IEnumerable<RemoteFile> GetDownloads()
        {
            return _downloadsCache.content.Select(downloaded => downloaded.source);
        }

        /// <summary>
        ///     Remove downloaded files
        /// </summary>
        public void RemoveDownloads()
        {
            RemoveDownloadsExcept(null);
        }

        /// <summary>
        ///     Remove specified downloads.
        /// </summary>
        /// <param name="remote">Remote resource that should be removed.</param>
        public void RemoveDownload(RemoteFile remote)
        {
            foreach (var downloaded in _downloadsCache.content.Where(downloaded => downloaded.source.Equals(remote)))
            {
                if (File.Exists(downloaded.localPath))
                {
                    Debug.LogWarning($"Delete file {downloaded.localPath}");
                    File.Delete(downloaded.localPath);
                }
                else if (Directory.Exists(downloaded.localPath))
                {
                    Debug.LogWarning($"Delete directory {downloaded.localPath}");
                    Directory.Delete(downloaded.localPath, true);
                }

                SaveCacheToFile(_cacheFilePath, _downloadsCache);

                break;
            }
        }

        /// <summary>
        ///     Remove downloaded files, except those in the <paramref name="remotes" />.
        /// </summary>
        /// <param name="remotes">List of the remote resources that should not be removed.</param>
        public void RemoveDownloadsExcept(IEnumerable<RemoteFile> remotes)
        {
            _downloadsCache.content.RemoveAll(cacheEntry =>
            {
                if (remotes != null && remotes.Contains(cacheEntry.source))
                {
                    return false;
                }

                if (File.Exists(cacheEntry.localPath))
                {
                    Debug.LogWarning($"Delete file {cacheEntry.localPath}");
                    File.Delete(cacheEntry.localPath);
                }
                else if (Directory.Exists(cacheEntry.localPath))
                {
                    Debug.LogWarning($"Delete directory {cacheEntry.localPath}");
                    Directory.Delete(cacheEntry.localPath, true);
                }

                return true;
            });

            SaveCacheToFile(_cacheFilePath, _downloadsCache);
        }

        private IEnumerator Download(RemoteFile remote, DownloadHandler downloadHandler,
            DownloadFailedCallback downloadFailedCallback)
        {
            if (string.IsNullOrEmpty(remote.url))
            {
                throw new ArgumentException("URL of the remote resource is null or empty.");
            }

            if (_downloadsCache.Contains(remote))
            {
                yield break;
            }

            using var request = new UnityWebRequest(remote.url, UnityWebRequest.kHttpVerbGET, downloadHandler, null);

            request.disposeDownloadHandlerOnDispose = false;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                yield break;
            }

            var error = !string.IsNullOrEmpty(request.error) ? request.error : "Download error";

            downloadFailedCallback.Invoke($"Failed to download {remote.url}. Reason: {error}");
        }

        private void AddToCache(RemoteFile remoteFile, string localPath)
        {
            _downloadsCache.content.Add(new DownloadedFileDescription
                {source = remoteFile, localPath = localPath});

            SaveCacheToFile(_cacheFilePath, _downloadsCache);
        }

        private static DownloadsCache LoadCacheFromFile(string cacheFilePath)
        {
            if (!File.Exists(cacheFilePath))
            {
                return new DownloadsCache
                {
                    content = new List<DownloadedFileDescription>()
                };
            }

            var text = File.ReadAllText(cacheFilePath);

            return Json.Deserialize<DownloadsCache>(text);
        }

        private static void SaveCacheToFile(string cacheFilePath, DownloadsCache cache)
        {
            var json = Json.Serialize(cache);

            File.WriteAllText(cacheFilePath, json);
        }

        [Serializable]
        private struct DownloadedFileDescription
        {
            public RemoteFile source;

            public string localPath;

            public bool Equals(DownloadedFileDescription other)
            {
                return source.Equals(other.source) && localPath == other.localPath;
            }
        }

        [Serializable]
        private struct DownloadsCache
        {
            public List<DownloadedFileDescription> content;

            public bool Contains(RemoteFile remote)
            {
                return content.Any(downloaded => downloaded.source.Equals(remote));
            }

            public bool TryGetByRemote(RemoteFile remote, out DownloadedFileDescription downloaded)
            {
                downloaded = content.SingleOrDefault(downloaded => downloaded.source.Equals(remote));

                return !downloaded.Equals(default);
            }
        }
    }
}