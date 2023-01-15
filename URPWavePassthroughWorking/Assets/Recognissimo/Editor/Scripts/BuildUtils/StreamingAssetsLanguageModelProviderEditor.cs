using System.IO;
using Recognissimo.Utils;
using Recognissimo.Utils.StreamingAssetsProvider.Common;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Recognissimo.Editor.BuildUtils
{
    public class StreamingAssetsLanguageModelProvider : IPreprocessBuildWithReport,
        IPostprocessBuildWithReport
    {
        public void OnPostprocessBuild(BuildReport report)
        {
            switch (report.summary.platform)
            {
                case BuildTarget.Android:
                case BuildTarget.WebGL:
                    RemoveStreamingAssetsManifest();
                    break;
            }
        }

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.Android)
            {
                Utils.CheckAndroidForceSDCardPermission();
            }

            if (report.summary.platform is BuildTarget.Android or BuildTarget.WebGL)
            {
                GenerateStreamingAssetsManifest();
            }
        }

        private static void GenerateStreamingAssetsManifest()
        {
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                return;
            }
            
            var manifest = StreamingAssetsManifestGenerator.Generate();
            var manifestSavePath = Path.Combine(Application.streamingAssetsPath,
                RemoteStreamingAssetsProvider.StreamingAssetsManifestName);
            File.WriteAllText(manifestSavePath, Json.Serialize(manifest));
        }

        private static void RemoveStreamingAssetsManifest()
        {
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                return;
            }
            
            var manifestSavePath = Path.Combine(Application.streamingAssetsPath,
                RemoteStreamingAssetsProvider.StreamingAssetsManifestName);
            File.Delete(manifestSavePath);
            var manifestMetaPath = manifestSavePath + ".meta";
            File.Delete(manifestMetaPath);
        }
    }
}