using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Recognissimo.Utils.Network;
using UnityEngine;

namespace Recognissimo.Editor.BuildUtils
{
    public static class StreamingAssetsManifestGenerator
    {
        private static bool IsFileValuable(string filePath)
        {
            var fileName = Path.GetFileName(filePath);

            return Path.GetExtension(fileName) != ".meta" && !fileName.StartsWith('.');
        }

        private static string GenerateFileVersion(string path)
        {
            var lastWriteTime = File.GetLastWriteTime(path);

            return lastWriteTime.ToFileTimeUtc() != 0
                ? lastWriteTime.ToString("O")
                : new Guid().ToString();
        }

        private static IEnumerable<RemoteFile> Enumerate()
        {
            var root = $"{Application.streamingAssetsPath}/";

            if (!Directory.Exists(root))
            {
                return new List<RemoteFile>();
            }
            
            return Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                .Where(IsFileValuable)
                .Select(path => new RemoteFile
                {
                    url = path.Replace("\\", "/").Replace(root, ""),
                    version = GenerateFileVersion(path)
                });
        }

        public static RemoteFilesManifest Generate()
        {
            return new RemoteFilesManifest
            {
                content = Enumerate().ToList()
            };
        }
    }
}