using System;
using Recognissimo.Core;

namespace Recognissimo
{
    /// <summary>
    ///     Loads files that are used by the speech processor to convert speech data to text.
    /// </summary>
    public sealed class LanguageModel : IDisposable
    {
        internal readonly LanguageModelResource Resource;

        /// <summary>
        ///     Create new instance.
        /// </summary>
        /// <param name="path">Path to the language model files.</param>
        public LanguageModel(string path)
        {
            Resource = new LanguageModelResource(path);
        }

        /// <summary>
        ///     Dispose language model.
        /// </summary>
        public void Dispose()
        {
            Resource?.Dispose();
        }
    }
}