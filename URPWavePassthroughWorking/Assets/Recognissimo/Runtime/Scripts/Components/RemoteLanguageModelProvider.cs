using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Recognissimo.Utils;
using Recognissimo.Utils.Network;
using UnityEngine;
using UnityEngine.Android;

namespace Recognissimo.Components
{
    /// <summary>
    ///     Remote language model description.
    /// </summary>
    [Serializable]
    public struct RemoteLanguageModelArchive
    {
        /// <summary>
        ///     Language of the model.
        /// </summary>
        public SystemLanguage language;

        /// <summary>
        ///     URL to the zipped language model.
        /// </summary>
        public string url;

        /// <summary>
        ///     In-archive path to language model content
        /// </summary>
        public string entry;
    }

    /// <summary>
    ///     <see cref="LanguageModelProvider" /> that provides language models located on a remote resource.
    /// </summary>
    [AddComponentMenu("Recognissimo/Language Model Providers/Remote Language Model Provider")]
    public sealed class RemoteLanguageModelProvider : LanguageModelProvider
    {
        private const string DownloadsDirectoryName = "DownloadedLanguageModels";

        /// <summary>
        ///     Language for which the language model will be loaded.
        /// </summary>
        public SystemLanguage language = SystemLanguage.English;

        /// <summary>
        ///     List of the available language models.
        /// </summary>
        [Tooltip("List of available language models")]
        public List<RemoteLanguageModelArchive> languageModels = new();

        private DownloadManager _downloadManager;

        private RemoteFilesManifest _manifest;

        private void OnEnable()
        {
            var downloadsDirectoryPath = Path.Combine(Application.persistentDataPath, DownloadsDirectoryName);

            _downloadManager = new DownloadManager(downloadsDirectoryPath);

            if (Application.platform == RuntimePlatform.Android)
            {
                RegisterInitializationTask("Request Android permissions", RequestAndroidPermissions,
                    CallCondition.Once);
            }

            RegisterInitializationTask("Update list of remote files", UpdateManifest,
                CallCondition.ValueChanged(() => languageModels, Enumerable.SequenceEqual));

            RegisterInitializationTask("Remove unused language models", RemoveUnusedLanguageModels,
                CallCondition.ValueChanged(() => languageModels, Enumerable.SequenceEqual));

            RegisterInitializationTask("Load language model", LoadModel, CallCondition.ValueChanged(() => language));
        }

        /// <summary>
        ///     Checks if the language model files are downloaded.
        /// </summary>
        /// <param name="downloadedLanguage">Language to check</param>
        /// <returns>True if language is downloaded, false otherwise. </returns>
        public bool IsDownloaded(SystemLanguage downloadedLanguage)
        {
            if (languageModels == null)
            {
                return false;
            }

            var model = languageModels.SingleOrDefault(model => model.language == downloadedLanguage);

            return !model.Equals(default) &&
                   _downloadManager.IsDownloaded(RemoteLanguageModelArchiveToRemoteFile(model), true);
        }

        /// <summary>
        ///     Remove language model files.
        /// </summary>
        /// <param name="removeLanguage">The language whose files are to be removed.</param>
        public void RemoveLanguageModelFiles(SystemLanguage removeLanguage)
        {
            var model = languageModels.SingleOrDefault(model => model.language == removeLanguage);

            if (model.Equals(default))
            {
                return;
            }

            _downloadManager.RemoveDownload(RemoteLanguageModelArchiveToRemoteFile(model));
        }

        private void UpdateManifest()
        {
            _manifest = AutoGenerateManifest();
        }

        private void RemoveUnusedLanguageModels()
        {
            if (_manifest.content == null)
            {
                return;
            }

            _downloadManager.RemoveDownloadsExcept(_manifest.content);
        }

        private IEnumerator LoadModel()
        {
            if (languageModels == null)
            {
                throw new InvalidOperationException("List of language models is missing.");
            }

            var model = languageModels.SingleOrDefault(model => model.language == language);

            if (model.Equals(default))
            {
                throw new InvalidOperationException($"Language model for {language.ToString()} language is missing.");
            }

            if (!_manifest.TryFindByUrl(model.url, out var remote))
            {
                throw new InvalidOperationException($"Model with URL {model.url} not found in manifest.");
            }

            if (!_downloadManager.IsDownloaded(remote, true))
            {
                var didDownloadFail = false;

                yield return _downloadManager.DownloadAndExtractZip(remote, failReason =>
                {
                    didDownloadFail = true;

                    FailInitialization(new IOException(failReason));
                });

                if (didDownloadFail)
                {
                    yield break;
                }

                if (Filesystem.RequiresSyncing())
                {
                    yield return Filesystem.Commit();
                }
            }

            var archiveContentPath = _downloadManager.GetDownloadedItemPath(remote);

            var modelPath = Path.Combine(archiveContentPath, model.entry ?? string.Empty);

            Model = new LanguageModel(modelPath);
        }

        private RemoteFilesManifest AutoGenerateManifest()
        {
            return new RemoteFilesManifest
            {
                content = languageModels
                    .Select(RemoteLanguageModelArchiveToRemoteFile)
                    .ToList()
            };
        }

        private static RemoteFile RemoteLanguageModelArchiveToRemoteFile(RemoteLanguageModelArchive description)
        {
            return new RemoteFile {url = description.url, version = description.url};
        }

        private void RequestAndroidPermissions()
        {
            string[] permissions = {Permission.ExternalStorageRead, Permission.ExternalStorageWrite};

            if (permissions.All(Permission.HasUserAuthorizedPermission))
            {
                return;
            }

            Permission.RequestUserPermissions(permissions);

            foreach (var permission in permissions)
            {
                if (Permission.HasUserAuthorizedPermission(permission))
                {
                    continue;
                }

                FailInitialization(new UnauthorizedAccessException(
                    $"Requested permissions {permission} denied, language model cannot be loaded."));

                return;
            }
        }
    }
}