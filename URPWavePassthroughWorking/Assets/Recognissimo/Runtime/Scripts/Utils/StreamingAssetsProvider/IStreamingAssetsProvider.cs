using System;
using System.Collections;

namespace Recognissimo.Utils.StreamingAssetsProvider
{
    public delegate void StreamingAssetsProvisionFailedCallback(Exception exception);

    /// <summary>
    ///     Interface for providing StreamingAssets files.
    /// </summary>
    public interface IStreamingAssetsProvider
    {
        /// <summary>
        ///     Initialize instance. Must be called before other methods.
        /// </summary>
        /// <returns>Enumerator to run coroutine on.</returns>
        public IEnumerator Initialize();

        /// <summary>
        ///     Load StreamingAssets files at specified path. Must be called before <see cref="Provide" />.
        /// </summary>
        /// <param name="streamingAssetsRelativePath">
        ///     StreamingAssets directory, all files and subdirectories of which should be
        ///     loaded.
        /// </param>
        /// <param name="failedCallback">Callback raised when operation failed.</param>
        /// <returns>Enumerator to run coroutine on.</returns>
        public IEnumerator Populate(string streamingAssetsRelativePath,
            StreamingAssetsProvisionFailedCallback failedCallback);

        /// <summary>
        ///     Get path to populated StreamingAssets directory. Must be called after <see cref="Populate" />.
        /// </summary>
        /// <param name="streamingAssetsRelativePath">StreamingAssets directory </param>
        /// <param name="failedCallback">Callback raised when operation failed.</param>
        /// <returns>Path to replicated StreamingAssets directory.</returns>
        public string Provide(string streamingAssetsRelativePath,
            StreamingAssetsProvisionFailedCallback failedCallback);
    }
}