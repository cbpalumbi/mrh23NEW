using System.Collections;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace Recognissimo.Utils
{
    public static class ZipExtractor
    {
        public static IEnumerator Extract(ZipArchive archive, string extractPath)
        {
            yield return ExtractEntries(archive, extractPath, "");
        }

        private static IEnumerator ExtractEntries(ZipArchive archive, string extractPath, string entryPath)
        {
            entryPath ??= "";

            foreach (var entry in archive.Entries)
            {
                if (entryPath.StartsWith("/"))
                {
                    Debug.LogWarning("Entry path must not start with backslash");
                    entryPath = entryPath[1..];
                }

                if (!entry.FullName.StartsWith(entryPath) || entry.FullName.EndsWith('/'))
                {
                    continue;
                }

                var entryName = string.IsNullOrEmpty(entryPath)
                    ? entry.FullName
                    : entry.FullName.Replace(entryPath, "");

                var entryExtractFilePath = Path.Combine(extractPath, entryName);

                var entryExtractDirectoryPath = Path.GetDirectoryName(entryExtractFilePath);

                if (!string.IsNullOrEmpty(entryExtractDirectoryPath) && !Directory.Exists(entryExtractDirectoryPath))
                {
                    Directory.CreateDirectory(entryExtractDirectoryPath);
                }

                if (File.Exists(entryExtractFilePath))
                {
                    File.Delete(entryExtractFilePath);
                }

                try
                {
                    entry.ExtractToFile(entryExtractFilePath);
                }
                catch
                {
                    const int fileStreamBufferSize = 4096;

                    using var fileStream = new FileStream(entryExtractFilePath, FileMode.CreateNew, FileAccess.Write,
                        FileShare.None, fileStreamBufferSize, false);
                    using var zipEntryStream = entry.Open();
                    zipEntryStream.CopyTo(fileStream);
                }

                yield return null;
            }
        }
    }
}