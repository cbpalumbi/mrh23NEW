using System;
using Recognissimo.Utils.StreamingAssetsProvider.Common;

namespace Recognissimo.Utils.StreamingAssetsProvider.WebGL
{
    /// <summary>
    ///     Helper class for providing StreamingAssets files for WebGL.
    /// </summary>
    public class WebGLStreamingAssetsProvider : RemoteStreamingAssetsProvider
    {
        /// <summary>
        ///     Create a new instance of <see cref="WebGLStreamingAssetsProvider" />.
        /// </summary>
        /// <param name="remoteStreamingAssetsPath">Path to the remote StreamingAssets.</param>
        /// <param name="indexedDataBasePath">Path to the indexed database.</param>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="remoteStreamingAssetsPath" /> or
        ///     <paramref name="indexedDataBasePath" /> is null or empty.
        /// </exception>
        public WebGLStreamingAssetsProvider(string remoteStreamingAssetsPath, string indexedDataBasePath) : base(
            remoteStreamingAssetsPath, indexedDataBasePath)
        {
        }
    }
}