using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Recognissimo.Editor.BuildUtils
{
    public class WebGL : IPostprocessBuildWithReport
    {
        public int callbackOrder { get; }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.WebGL)
            {
                return;
            }

            var outputDir = report.summary.outputPath;

            if (!Directory.Exists(outputDir))
            {
                ShowErrorHandlingInstruction("Build output not found.");
            }

            var indexHtmlFiles = Directory.GetFiles(outputDir, "index.html", SearchOption.AllDirectories);

            if (indexHtmlFiles.Length is 0 or > 1)
            {
                ShowErrorHandlingInstruction("Recognissimo cannot find index.html in build directory.");
            }

            var indexHtmlFilePath = indexHtmlFiles[0];

            var indexHtmlDir = Path.GetDirectoryName(indexHtmlFilePath);

            if (indexHtmlDir == null)
            {
                throw new InvalidOperationException("index.html directory is null.");
            }

            const string wasmWorkerFileName = "recognissimo-worker.js";

            var wasmWorkerFilePaths =
                Directory.GetFiles(Application.dataPath, wasmWorkerFileName, SearchOption.AllDirectories);

            if (wasmWorkerFilePaths.Length == 0)
            {
                ShowErrorHandlingInstruction($"Recognissimo cannot find {wasmWorkerFileName} in the assets directory.");
                return;
            }

            var wasmWorkerFilePath = wasmWorkerFilePaths[0];

            File.Copy(wasmWorkerFilePath, Path.Combine(indexHtmlDir, wasmWorkerFileName), true);
        }

        private static void ShowErrorHandlingInstruction(string errorReason)
        {
            const string instruction =
                "Copy file 'recognissimo-worker.js' to your build directory near index.html";

            EditorUtility.DisplayDialog("Post-build action required", $"{errorReason}\n{instruction}", "Ok");
        }
    }
}