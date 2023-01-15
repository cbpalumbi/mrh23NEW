using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Recognissimo.Utils;
using Recognissimo.Utils.StreamingAssetsProvider;
using UnityEngine;

namespace Recognissimo.Components
{
    /// <summary>
    ///     StreamingAssets language model description.
    /// </summary>
    [Serializable]
    public struct StreamingAssetsLanguageModel
    {
        /// <summary>
        ///     Language of the model.
        /// </summary>
        [Tooltip("Language of the model")]
        public SystemLanguage language;

        /// <summary>
        ///     Path relative to StreamingAssets folder.
        /// </summary>
        [Tooltip("Path relative to StreamingAssets folder")]
        public string path;
    }

    /// <summary>
    ///     <see cref="LanguageModelProvider" /> that provides language models located in StreamingAssets folder.
    /// </summary>
    [AddComponentMenu("Recognissimo/Language Model Providers/Streaming Assets Language Model Provider")]
    public sealed class StreamingAssetsLanguageModelProvider : LanguageModelProvider
    {
        /// <summary>
        ///     Language for which the language model will be loaded.
        /// </summary>
        [Tooltip("Language for which the language model will be loaded")]
        public SystemLanguage language = SystemLanguage.English;

        /// <summary>
        ///     List of the available language models.
        /// </summary>
        [Tooltip("List of available language models")]
        public List<StreamingAssetsLanguageModel> languageModels;

        private IStreamingAssetsProvider _streamingAssetsProvider;

        private void OnEnable()
        {
            _streamingAssetsProvider = StreamingAssetsProviderFactory.MaybeCreate();

            if (_streamingAssetsProvider != null)
            {
                RegisterInitializationTask("Discover streaming assets", _streamingAssetsProvider.Initialize,
                    CallCondition.Once);
            }

            RegisterInitializationTask("Load language model", LoadModel, CallCondition.ValueChanged(() => language));
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

            string absoluteModelPath;

            if (_streamingAssetsProvider == null)
            {
                absoluteModelPath = Path.Combine(Application.streamingAssetsPath, model.path);
            }
            else
            {
                yield return _streamingAssetsProvider.Populate(model.path, FailInitialization);

                if (Filesystem.RequiresSyncing())
                {
                    yield return Filesystem.Commit();
                }

                absoluteModelPath = _streamingAssetsProvider.Provide(model.path, FailInitialization);
            }

            Model = new LanguageModel(absoluteModelPath);
        }
    }
}