using System;
using System.Collections.Generic;
using Recognissimo.Core;
using Recognissimo.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Recognissimo.Components
{
    /// <summary>
    ///     <see cref="SpeechProcessor" /> for speech recognition.
    /// </summary>
    [AddComponentMenu("Recognissimo/Speech Processors/Speech Recognizer")]
    public sealed class SpeechRecognizer : SpeechProcessor
    {
        [SerializeField]
        private SpeechRecognizerSettings settings = new()
        {
            partialResultReady = new PartialResultEvent(),
            resultReady = new ResultEvent()
        };

        /// <summary>
        ///     List of the words to recognize. Speech recognizer will select the result only from the presented words.
        ///     Use special word "[unk]" (without quotes) to allow unknown words in the output.
        /// </summary>
        /// <example>
        ///     <code>
        ///     var vocabulary = new List&lt;string&gt; {"light", "on", "off", "[unk]"};
        ///     </code>
        /// </example>
        /// <remarks>
        ///     This feature may not work with some language models.
        /// </remarks>
        public List<string> Vocabulary
        {
            get => settings.vocabulary;
            set => settings.vocabulary = value;
        }

        /// <summary>
        ///     Whether the recognition result should include details.
        /// </summary>
        public bool EnableDetails
        {
            get => settings.enableDetails;
            set => settings.enableDetails = value;
        }

        /// <summary>
        ///     Whether the recognition result should contain a list of alternative results.
        /// </summary>
        public int Alternatives
        {
            get => settings.alternatives;
            set => settings.alternatives = value;
        }

        /// <summary>
        ///     New partial result ready.
        /// </summary>
        public PartialResultEvent PartialResultReady => settings.partialResultReady;

        /// <summary>
        ///     New result ready.
        /// </summary>
        public ResultEvent ResultReady => settings.resultReady;

        private string CreateVocabularyString()
        {
            const string separator = " ";
            return $"[\"{string.Join(separator, settings.vocabulary).ToLower()}\"]";
        }

        private void OnResult(SpeechRecognizerAlgorithm.Result result)
        {
            switch (result.ResultType)
            {
                case SpeechRecognizerAlgorithm.ResultType.Partial:
                {
                    PartialResultReady.Invoke(Json.Deserialize<PartialResult>(result.Data));
                    break;
                }
                case SpeechRecognizerAlgorithm.ResultType.Complete:
                {
                    ResultReady.Invoke(Json.Deserialize<Result>(result.Data));
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(result.ResultType));
            }
        }

        private SpeechRecognizerAlgorithm.Settings OnSetup()
        {
            return new SpeechRecognizerAlgorithm.Settings
            {
                Vocabulary = settings.vocabulary?.Count > 0 ? CreateVocabularyString() : null,
                EnableDetails = EnableDetails,
                MaxAlternatives = Alternatives
            };
        }

        internal override Algorithm CreateAlgorithm()
        {
            return new SpeechRecognizerAlgorithm(OnSetup, OnResult);
        }

        [Serializable]
        private struct SpeechRecognizerSettings
        {
            public List<string> vocabulary;

            public bool enableDetails;

            public int alternatives;

            public PartialResultEvent partialResultReady;

            public ResultEvent resultReady;
        }
    }

    [Serializable]
    public class PartialResultEvent : UnityEvent<PartialResult>
    {
    }

    [Serializable]
    public class ResultEvent : UnityEvent<Result>
    {
    }

    /// <summary>
    ///     Detailed description of a decoded word.
    /// </summary>
    [Serializable]
    public struct Word
    {
        /// <summary>
        ///     Confidence.
        /// </summary>
        public float conf;

        /// <summary>
        ///     Start time of the word in seconds.
        /// </summary>
        public float start;

        /// <summary>
        ///     End time of the word in seconds.
        /// </summary>
        public float end;

        /// <summary>
        ///     Decoded word.
        /// </summary>
        public string word;
    }

    /// <summary>
    ///     Partial speech recognition result which may change as recognizer process more data.
    /// </summary>
    [Serializable]
    public struct PartialResult
    {
        /// <summary>
        ///     Detailed description of decoded text.
        /// </summary>
        public List<Word> result;

        /// <summary>
        ///     Decoded text.
        /// </summary>
        public string partial;
    }

    /// <summary>
    ///     Speech recognition result.
    /// </summary>
    [Serializable]
    public struct Result
    {
        /// <summary>
        ///     Detailed description of decoded text.
        /// </summary>
        public List<Word> result;

        /// <summary>
        ///     Decoded text.
        /// </summary>
        public string text;

        /// <summary>
        ///     List of alternative results.
        /// </summary>
        public List<string> alternatives;
    }
}