using System;
using System.Text;

namespace Recognissimo
{
    /// <summary>
    ///     Base class for all <see cref="SpeechProcessor" /> exceptions.
    /// </summary>
    public abstract class SpeechProcessorException : Exception
    {
        protected SpeechProcessorException(string message) : base(message)
        {
        }

        protected SpeechProcessorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    ///     Base class for all <see cref="SpeechProcessor" /> exceptions during initialization.
    /// </summary>
    public abstract class InitializationException : SpeechProcessorException
    {
        protected InitializationException(string message) : base(message)
        {
        }

        protected InitializationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    ///     Thrown when invalid language model provided.
    /// </summary>
    public class InvalidLanguageModelException : InitializationException
    {
        public InvalidLanguageModelException(string message) : base(message)
        {
        }
    }

    /// <summary>
    ///     Thrown when invalid sample rate provided.
    /// </summary>
    public class InvalidSampleRateException : InitializationException
    {
        public InvalidSampleRateException(string message) : base(message)
        {
        }
    }

    /// <summary>
    ///     Thrown when invalid input is provided to <see cref="SpeechProcessor" /> implementation.
    ///     It is recommended that such an exception be reported to the developer.
    /// </summary>
    public class InvalidAlgorithmInputException : InitializationException
    {
        public InvalidAlgorithmInputException(string message) : base(message)
        {
        }
    }

    /// <summary>
    ///     Thrown when internal error occurs in <see cref="SpeechProcessor" /> during initialization.
    ///     It is recommended that such an exception be reported to the developer.
    /// </summary>
    public class InternalInitializationException : InitializationException
    {
        public InternalInitializationException(string message) : base(message)
        {
        }
    }

    /// <summary>
    ///     Thrown when <see cref="SpeechProcessorDependency" /> initialization fails.
    /// </summary>
    public class DependencyInitializationException : InitializationException
    {
        public DependencyInitializationException(SpeechProcessorDependency dependency, string initializationTaskName,
            Exception innerException)
            : base(PrettyMessage(dependency, initializationTaskName, innerException), innerException)
        {
            Dependency = dependency;
            InitializationTaskName = initializationTaskName;
        }

        public SpeechProcessorDependency Dependency { get; }

        public string InitializationTaskName { get; }

        private static string PrettyMessage(SpeechProcessorDependency dependency, string initializationTaskName,
            Exception innerException)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append($"{dependency.GetType()} failed to execute task \"{initializationTaskName}\".");

            if (innerException != null)
            {
                stringBuilder.Append($" {innerException.Message}.");
            }

            return stringBuilder.ToString();
        }
    }

    /// <summary>
    ///     Base class for all <see cref="SpeechProcessor" /> exceptions during runtime.
    /// </summary>
    public abstract class RuntimeException : SpeechProcessorException
    {
        protected RuntimeException(string message) : base(message)
        {
        }

        protected RuntimeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    ///     Thrown when <see cref="SpeechSource" /> failed at runtime.
    ///     It is recommended that such an exception be reported to the developer.
    /// </summary>
    public class InternalRuntimeException : RuntimeException
    {
        public InternalRuntimeException(string message) : base(message)
        {
        }
    }

    /// <summary>
    ///     Thrown when <see cref="SpeechSource" /> failed at runtime.
    /// </summary>
    public class SpeechSourceRuntimeException : RuntimeException
    {
        public SpeechSourceRuntimeException(string message) : base(message)
        {
        }

        public SpeechSourceRuntimeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}