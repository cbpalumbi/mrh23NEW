using Recognissimo.Utils.StreamingAssetsProvider.Android;
using Recognissimo.Utils.StreamingAssetsProvider.WebGL;
using UnityEngine;

namespace Recognissimo.Utils.StreamingAssetsProvider
{
    public static class StreamingAssetsProviderFactory
    {
        /// <summary>
        ///     Create instance if streaming assets files are not available through the standard methods
        ///     for the current platform.
        ///     Factory is using <see cref="UnityEngine.Application" /> to detect platform and paths.
        /// </summary>
        /// <returns></returns>
        public static IStreamingAssetsProvider MaybeCreate()
        {
            return Application.platform switch
            {
                RuntimePlatform.WebGLPlayer => new WebGLStreamingAssetsProvider(Application.streamingAssetsPath,
                    Application.persistentDataPath),
                RuntimePlatform.Android => new AndroidStreamingAssetsProvider(Application.dataPath,
                    Application.streamingAssetsPath, Application.persistentDataPath),
                _ => null
            };
        }
    }
}