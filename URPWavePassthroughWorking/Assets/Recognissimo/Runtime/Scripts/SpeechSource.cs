using System;

namespace Recognissimo
{
    /// <summary>
    ///     <see cref="SpeechSource.SamplesReady" /> event data.
    /// </summary>
    public class SamplesReadyEventArgs : EventArgs
    {
        public SamplesReadyEventArgs(float[] samples, int length)
        {
            Samples = samples;
            Length = length;
        }

        /// <summary>
        ///     Audio samples in Float32 format.
        /// </summary>
        public float[] Samples { get; }

        /// <summary>
        ///     Audio samples length.
        /// </summary>
        public int Length { get; }
    }

    /// <summary>
    ///     <see cref="SpeechSource.RuntimeFailure" /> event data.
    /// </summary>
    public class RuntimeFailureEventArgs : EventArgs
    {
        public RuntimeFailureEventArgs(SpeechSourceRuntimeException exception)
        {
            Exception = exception;
        }

        public SpeechSourceRuntimeException Exception { get; }
    }

    /// <summary>
    ///     Base class for all speech sources.
    /// </summary>
    public abstract class SpeechSource : SpeechProcessorDependency
    {
        /// <summary>
        ///     Speech sampling rate. Must be set during initialization.
        /// </summary>
        public virtual int SampleRate { get; protected set; }

        /// <summary>
        ///     Raised when new samples arrive.
        /// </summary>
        public event EventHandler<SamplesReadyEventArgs> SamplesReady;

        /// <summary>
        ///     Raised when <see cref="SpeechSource" /> have run out of samples.
        /// </summary>
        public event EventHandler Dried;

        /// <summary>
        ///     Raised when <see cref="SpeechSource" /> failed during runtime.
        /// </summary>
        public event EventHandler<RuntimeFailureEventArgs> RuntimeFailure;

        /// <summary>
        ///     Called by <see cref="SpeechProcessor" /> at the start of processing.
        /// </summary>
        public abstract void StartProducing();

        /// <summary>
        ///     Called when processing stops (e.g. when <see cref="SpeechProcessor.StopProcessing" /> called or
        ///     when <see cref="RuntimeFailure" /> event emitted).
        /// </summary>
        public abstract void StopProducing();

        /// <summary>
        ///     Helper method for triggering the event.
        /// </summary>
        /// <param name="eventArgs">Event argument.</param>
        protected void OnSamplesReady(SamplesReadyEventArgs eventArgs)
        {
            SamplesReady?.Invoke(this, eventArgs);
        }

        /// <summary>
        ///     Helper method for triggering the event.
        /// </summary>
        protected void OnDried()
        {
            Dried?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Helper method for triggering the event.
        /// </summary>
        /// <param name="eventArgs">Event argument. </param>
        protected void OnRuntimeFailure(RuntimeFailureEventArgs eventArgs)
        {
            RuntimeFailure?.Invoke(this, eventArgs);
        }
    }
}