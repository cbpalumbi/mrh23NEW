using System;
using System.Collections;
using Recognissimo.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Recognissimo
{
    /// <summary>
    ///     <see cref="SpeechProcessor" /> state.
    /// </summary>
    public enum SpeechProcessorState
    {
        Inactive,
        Initializing,
        Processing,
        Finalizing
    }

    /// <summary>
    ///     Base class for all speech processors.
    /// </summary>
    public abstract class SpeechProcessor : MonoBehaviour
    {
        [SerializeField]
        private Settings generalSettings;

        [SerializeField]
        private LifecycleEvents lifecycleEvents = new()
        {
            started = new UnityEvent(),
            finished = new UnityEvent(),
            initializationFailed = new UnityEvent<InitializationException>(),
            runtimeFailed = new UnityEvent<RuntimeException>()
        };

        private Algorithm _algorithm;

        private Context _context;

        private Coroutine _initializationCoroutine;

        /// <summary>
        ///     Current state of <see cref="SpeechProcessor" />
        /// </summary>
        public SpeechProcessorState State { get; private set; } = SpeechProcessorState.Inactive;

        /// <summary>
        ///     Language model provider. This value is read when <see cref="StartProcessing" /> called.
        /// </summary>
        public LanguageModelProvider LanguageModelProvider
        {
            get => generalSettings.modelProvider;
            set => generalSettings.modelProvider = value;
        }

        /// <summary>
        ///     Speech source. This value is read when <see cref="StartProcessing" /> called.
        /// </summary>
        public SpeechSource SpeechSource
        {
            get => generalSettings.speechSource;
            set
            {
                if (SpeechSource)
                {
                    UnbindSpeechSource();
                }

                generalSettings.speechSource = value;
            }
        }

        /// <summary>
        ///     Whether to execute <see cref="StartProcessing" /> at start.
        /// </summary>
        public bool AutoStart
        {
            get => generalSettings.autoStart;
            set => generalSettings.autoStart = value;
        }

        /// <summary>
        ///     <see cref="SpeechProcessor" /> successfully started.
        /// </summary>
        public UnityEvent Started => lifecycleEvents.started;

        /// <summary>
        ///     <see cref="SpeechProcessor" /> successfully finished.
        /// </summary>
        public UnityEvent Finished => lifecycleEvents.finished;

        /// <summary>
        ///     <see cref="SpeechProcessor" /> or one of its dependencies failed during initialization.
        /// </summary>
        public UnityEvent<InitializationException> InitializationFailed => lifecycleEvents.initializationFailed;

        /// <summary>
        ///     <see cref="SpeechProcessor" /> or <see cref="SpeechSource" /> dependency failed at runtime.
        /// </summary>
        public UnityEvent<RuntimeException> RuntimeFailed => lifecycleEvents.runtimeFailed;

        private void Start()
        {
            if (AutoStart)
            {
                StartProcessing();
            }
        }

        private void Update()
        {
            if (State == SpeechProcessorState.Inactive)
            {
                return;
            }

            switch (_context.NextEvent())
            {
                case ContextEvent.None:
                    break;
                case ContextEvent.Started:
                    State = SpeechProcessorState.Processing;
                    Started?.Invoke();
                    break;
                case ContextEvent.Finished:
                    State = SpeechProcessorState.Inactive;
                    Finished?.Invoke();
                    break;
                case ContextEvent.Crashed:
                    var errorReport = _context.LastError();

                    var errorAssociatedState = State;

                    Stop(StopMode.Hard);

                    switch (errorAssociatedState)
                    {
                        case SpeechProcessorState.Initializing:
                            InitializationFailed?.Invoke(errorReport.Error switch
                            {
                                Error.InvalidLanguageModel =>
                                    new InvalidLanguageModelException(errorReport.Description),
                                Error.InvalidSampleRate => new InvalidSampleRateException(errorReport.Description),
                                Error.Algorithm => new InvalidAlgorithmInputException(errorReport.Description),
                                Error.Internal => new InternalInitializationException(errorReport.Description),
                                _ => throw new ArgumentOutOfRangeException(nameof(errorReport))
                            });
                            break;
                        case SpeechProcessorState.Processing when errorReport.Error == Error.Internal:
                            RuntimeFailed?.Invoke(new InternalRuntimeException(errorReport.Description));
                            break;
                        case SpeechProcessorState.Processing:
                        case SpeechProcessorState.Finalizing:
                            RuntimeFailed?.Invoke(new InternalRuntimeException(
                                $"Unexpected state: error of type {errorReport.Error} ({errorReport.Description}) occured at runtime"));
                            break;
                        case SpeechProcessorState.Inactive:
                        default:
                            break;
                    }

                    break;
                case ContextEvent.ResultReady:
                    if (State is SpeechProcessorState.Processing or SpeechProcessorState.Finalizing)
                    {
                        _algorithm.LoadResult();
                    }
                    else
                    {
                        Debug.LogWarning("SpeechProcessor disposed result");
                        _algorithm.DisposeResult();
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnEnable()
        {
            _algorithm = CreateAlgorithm();
            _context = new Context(_algorithm);

            if (Application.isEditor && State != SpeechProcessorState.Inactive)
            {
                StartProcessing();
            }
        }

        private void OnDestroy()
        {
            Stop(StopMode.Hard);

            _context.Dispose();
            _algorithm.Dispose();
        }

        internal abstract Algorithm CreateAlgorithm();

        private void SpeechSourceSamplesReadyEventHandler(object sender, SamplesReadyEventArgs eventArgs)
        {
            _context.EnqueueFloat32(eventArgs.Samples, eventArgs.Length);
        }

        private void SpeechSourceDriedEventHandler(object sender, EventArgs eventArgs)
        {
            Stop();
        }

        private void SpeechSourceRuntimeFailureEventHandler(object sender, RuntimeFailureEventArgs eventArgs)
        {
            Stop(StopMode.Hard);

            RuntimeFailed?.Invoke(eventArgs.Exception);
        }

        /// <summary>
        ///     Start speech processing. <see cref="SpeechProcessor" /> will setup itself asynchronously, then emit
        ///     <see cref="Started" />.
        ///     <see cref="SpeechSource" /> and <see cref="LanguageModelProvider" /> must be set by the time the method is called.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     If <see cref="SpeechSource" /> or <see cref="LanguageModelProvider" /> is null.
        /// </exception>
        public void StartProcessing()
        {
            if (!SpeechSource)
            {
                throw new InvalidOperationException("SpeechSource is not set.");
            }

            if (!LanguageModelProvider)
            {
                throw new InvalidOperationException("LanguageModelProvider is not set.");
            }

            if (State != SpeechProcessorState.Inactive)
            {
                Stop(StopMode.Hard);
            }

            _initializationCoroutine = StartCoroutine(Initialize());
        }

        /// <summary>
        ///     Stop speech processing. <see cref="SpeechProcessor" /> will:
        ///     <list type="number">
        ///         <item>
        ///             stop accepting new samples;
        ///         </item>
        ///         <item>
        ///             process the remaining samples;
        ///         </item>
        ///         <item>
        ///             emit <see cref="Finished" />.
        ///         </item>
        ///     </list>
        /// </summary>
        public void StopProcessing()
        {
            Stop();
        }

        private void Stop(StopMode mode = StopMode.Soft)
        {
            try {
                switch (mode)
                {
                    case StopMode.Soft:
                        State = SpeechProcessorState.Finalizing;
                        _context.Stop();
                        break;
                    case StopMode.Hard:
                        State = SpeechProcessorState.Inactive;
                        _context.Abort();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
                }
            } catch {
                Debug.LogWarning("caught error in speech processor");
            }


            if (SpeechSource)
            {
                UnbindSpeechSource();
                SpeechSource.StopProducing();
            }

            if (_initializationCoroutine != null)
            {
                StopCoroutine(_initializationCoroutine);
            }
        }

        private void BindSpeechSource()
        {
            SpeechSource.SamplesReady += SpeechSourceSamplesReadyEventHandler;
            SpeechSource.Dried += SpeechSourceDriedEventHandler;
            SpeechSource.RuntimeFailure += SpeechSourceRuntimeFailureEventHandler;
        }

        private void UnbindSpeechSource()
        {
            SpeechSource.SamplesReady -= SpeechSourceSamplesReadyEventHandler;
            SpeechSource.Dried -= SpeechSourceDriedEventHandler;
            SpeechSource.RuntimeFailure -= SpeechSourceRuntimeFailureEventHandler;
        }

        private IEnumerator Initialize()
        {
            State = SpeechProcessorState.Initializing;

            InitializationFailedCallback CreateCallback(SpeechProcessorDependency dependency)
            {
                return (initializationTaskName, exception) =>
                {
                    State = SpeechProcessorState.Inactive;

                    StopCoroutine(_initializationCoroutine);

                    InitializationFailed?.Invoke(
                        new DependencyInitializationException(dependency, initializationTaskName, exception));
                };
            }

            // Skip a frame to ensure that the dependency has registered initialization tasks. 
            yield return null;

            yield return LanguageModelProvider.Initialize(null, CreateCallback(LanguageModelProvider));

            if (LanguageModelProvider.Model == null)
            {
                throw new InvalidOperationException(
                    $"{LanguageModelProvider.GetType().Name} provided null language model.");
            }

            yield return SpeechSource.Initialize(null, CreateCallback(SpeechSource));

            _algorithm.Setup();

            _context.Start(LanguageModelProvider.Model.Resource, SpeechSource.SampleRate);

            UnbindSpeechSource();

            BindSpeechSource();

            SpeechSource.StartProducing();
        }

        private enum StopMode
        {
            Soft,
            Hard
        }

        [Serializable]
        private struct Settings
        {
            public LanguageModelProvider modelProvider;
            public SpeechSource speechSource;
            public bool autoStart;
        }

        [Serializable]
        private struct LifecycleEvents
        {
            public UnityEvent started;
            public UnityEvent finished;
            public UnityEvent<InitializationException> initializationFailed;
            public UnityEvent<RuntimeException> runtimeFailed;
        }
    }
}