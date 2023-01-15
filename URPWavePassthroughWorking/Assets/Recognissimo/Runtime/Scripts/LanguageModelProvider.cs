namespace Recognissimo
{
    /// <summary>
    ///     Base class for all model providers.
    /// </summary>
    public abstract class LanguageModelProvider : SpeechProcessorDependency
    {
        private LanguageModel _model;

        /// <summary>
        ///     Language model instance. Must be set during initialization.
        /// </summary>
        public LanguageModel Model
        {
            get => _model;

            protected set
            {
                _model?.Dispose();
                _model = value;
            }
        }
    }
}