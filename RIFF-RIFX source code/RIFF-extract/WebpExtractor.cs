using System;
using System.IO;
using System.Collections.Generic;

class WebpExtractor
{
    // 定义 RIFF 文件头
    private static readonly byte[] riffHeader = { 0x52, 0x49, 0x46, 0x46 };
    // 定义 WebP 数据块
    private static readonly byte[] webpBlock = { 0x57, 0x45, 0x42, 0x50, 0x56, 0x50, 0x38 };

    // 从文件内容中提取 WebP 数据
    private static IEnumerable<byte[]> ExtractWebpData(byte[] fileContent)
    {
        int webpDataStart = 0;
        while ((webpDataStart = IndexOf(fileContent, riffHeader, webpDataStart)) != -1)
        {
            // 从文件内容中读取文件大小
            int fileSize = BitConverter.ToInt32(fileContent, webpDataStart + 4);
            // 确保文件大小是 4 字节对齐
            fileSize = (fileSize + 1) & ~1;

            int blockStart = webpDataStart + 8;
            bool hasWebpBlock = IndexOf(fileContent, webpBlock, blockStart) != -1;

            if (hasWebpBlock)
            {
                byte[] webpData = new byte[fileSize + 8];
                Array.Copy(fileContent, webpDataStart, webpData, 0, fileSize + 8);
                yield return webpData;
            }

            webpDataStart += fileSize + 8;
        }
    }

    // 辅助方法，用于查找字节数组中某个子数组的起始位置
    private static int IndexOf(byte[] source, byte[] pattern, int startIndex)
    {
        for (int i = startIndex; i <= source.Length - pattern.Length; i++)
        {
            bool found = true;
            for (int j = 0; j < pattern.Length; j++)
            {
                if (source[i + j] != pattern[j])
                {
                    found = false;
                    break;
                }
            }
            if (found)
            {
                return i;
            }
        }
        return -1;
    }

    // 从单个文件中提取 WebP 文件
    private static void ExtractWebpsFromFile(string filePath)
    {
        byte[] fileContent = File.ReadAllBytes(filePath);
        int count = 0;
        foreach (byte[] webpData in ExtractWebpData(fileContent))
        {
            string baseFilename = Path.GetFileNameWithoutExtension(filePath);
            string extractedFilename = $"{baseFilename}_{count}.webp";
            string? dirName = Path.GetDirectoryName(filePath);
            string extractedPath;
            if (dirName != null)
            {
                extractedPath = Path.Combine(dirName, extractedFilename);
            }
            else
            {
                extractedPath = extractedFilename;
            }

            string? dirToCreate = Path.GetDirectoryName(extractedPath);
            if (dirToCreate != null)
            {
                Directory.CreateDirectory(dirToCreate);
            }
            File.WriteAllBytes(extractedPath, webpData);
            Console.WriteLine($"Extracted content saved as: {extractedPath}");
            count++;
        }
    }

    // 从指定目录中提取 WebP 文件
    public static void ExtractWebps(string directoryPath)
    {
        foreach (string filePath in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
        {
            if (Path.GetExtension(filePath).Equals(".webp", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            ExtractWebpsFromFile(filePath);
        }
    }
}