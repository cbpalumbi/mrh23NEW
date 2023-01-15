using System;
using System.Collections;
using Recognissimo.Utils.Network;
using UnityEngine.Networking;

namespace Recognissimo.Utils.StreamingAssetsProvider.Common
{
    internal static class RemoteFilesManifestDownloader
    {
        public static IEnumerator Download(string url, Action<RemoteFilesManifest> downloaded)
        {
            using var downloadHandler = new DownloadHandlerBuffer();

            using var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET,
                downloadHandler, null);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new InvalidOperationException(request.error);
            }

            var manifest = Json.Deserialize<RemoteFilesManifest>(downloadHandler.text);

            downloaded?.Invoke(manifest);
        }
    }
}