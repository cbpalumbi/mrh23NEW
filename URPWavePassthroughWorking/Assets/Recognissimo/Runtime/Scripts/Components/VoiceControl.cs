using System;
using System.Collections.Generic;
using System.Linq;
using Recognissimo.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Recognissimo.Components
{
    /// <summary>
    ///     Phrase/callback pair.
    /// </summary>
    [Serializable]
    public struct VoiceControlCommand
    {
        /// <summary>
        ///     Phrase to recognize.
        ///     You can use groups "()" and alternations "|" to create options:
        ///     <code>
        ///         "red|green"; // triggered when "red" or "green" is spoken
        ///         "turn (on|off) the light"; // triggered when "turn on the light" or "turn off the light" is spoken
        ///         "turn (on|off) (the )?light"; // optional "the"
        ///     </code>
        /// </summary>
        [Tooltip("Phrase to recognize")]
        public string phrase;

        /// <summary>
        ///     UnityEvent that will be triggered when the <see cref="phrase" /> is spoken.
        /// </summary>
        public UnityEvent onSpoken;

        /// <summary>
        ///     Create instance.
        /// </summary>
        /// <param name="phrase">Phrase to recognize. </param>
        /// <param name="onSpoken">Unity event that will be triggered when the <paramref name="phrase" /> is spoken.</param>
        public VoiceControlCommand(string phrase, UnityEvent onSpoken)
        {
            this.phrase = phrase;
            this.onSpoken = onSpoken;
        }

        /// <summary>
        ///     Create instance and bind <paramref name="action" /> to <see cref="onSpoken" />.
        /// </summary>
        /// <param name="phrase">Phrase to recognize.</param>
        /// <param name="action">Action that will be triggered when the <paramref name="phrase" /> is spoken.</param>
        public VoiceControlCommand(string phrase, UnityAction action)
        {
            this.phrase = phrase;
            onSpoken = new UnityEvent();
            onSpoken.AddListener(action);
        }
    }

    /// <summary>
    ///     <see cref="SpeechProcessor" /> for voice control.
    /// </summary>
    [AddComponentMenu("Recognissimo/Speech Processors/Voice Control")]
    public sealed class VoiceControl : SpeechProcessor
    {
        [SerializeField]
        private VoiceControlSettings settings;

        /// <summary>
        ///     List of voice commands.
        /// </summary>
        public List<VoiceControlCommand> Commands
        {
            get => settings.commands;
            set => settings.commands = value;
        }

        /// <summary>
        ///     Whether to try to recognize voice commands using the preliminary recognition results.
        /// </summary>
        public bool AsapMode
        {
            get => settings.asapMode;
            set => settings.asapMode = value;
        }

        private void OnResult(VoiceControlAlgorithm.Result result)
        {
            settings.commands?[result.CommandIndex].onSpoken?.Invoke();
        }

        private VoiceControlAlgorithm.Settings OnSetup()
        {
            var commands = settings.commands ?? new List<VoiceControlCommand>();

            var regex = string
                .Join("|", commands.Select(cmd => $"({cmd.phrase})"))
                .ToLower();

            return new VoiceControlAlgorithm.Settings
            {
                AsapMode = settings.asapMode,
                Commands = regex
            };
        }

        internal override Algorithm CreateAlgorithm()
        {
            return new VoiceControlAlgorithm(OnSetup, OnResult);
        }

        [Serializable]
        private struct VoiceControlSettings
        {
            [Tooltip("List of voice commands and its handlers")]
            public List<VoiceControlCommand> commands;

            [Tooltip("Whether to try to recognize voice commands using preliminary recognition results")]
            public bool asapMode;
        }
    }
}