using System;
using System.IO;
using System.Collections.Generic;

class XwmaExtractor
{
    // 定义 RIFF 文件头
    private static readonly byte[] riffHeader = { 0x52, 0x49, 0x46, 0x46 };
    // 定义 xwma 数据块
    private static readonly byte[] xwmaBlock = { 0x58, 0x57, 0x4D, 0x41, 0x66, 0x6D, 0x74 };

    // 从文件内容中提取 xwma 数据
    private static IEnumerable<byte[]> ExtractxwmaData(byte[] fileContent)
    {
        int xwmaDataStart = 0;
        while ((xwmaDataStart = IndexOf(fileContent, riffHeader, xwmaDataStart)) != -1)
        {
            // 从文件内容中读取文件大小
            int fileSize = BitConverter.ToInt32(fileContent, xwmaDataStart + 4);
            // 确保文件大小是 4 字节对齐
            fileSize = (fileSize + 1) & ~1;

            int blockStart = xwmaDataStart + 8;
            bool hasxwmaBlock = IndexOf(fileContent, xwmaBlock, blockStart) != -1;

            if (hasxwmaBlock)
            {
                byte[] xwmaData = new byte[fileSize + 8];
                Array.Copy(fileContent, xwmaDataStart, xwmaData, 0, fileSize + 8);
                yield return xwmaData;
            }

            xwmaDataStart += fileSize + 8;
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

    // 从单个文件中提取 xwma 文件
    private static void ExtractxwmasFromFile(string filePath)
    {
        byte[] fileContent = File.ReadAllBytes(filePath);
        int count = 0;
        foreach (byte[] xwmaData in ExtractxwmaData(fileContent))
        {
            string baseFilename = Path.GetFileNameWithoutExtension(filePath);
            string extractedFilename = $"{baseFilename}_{count}.xwma";
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
            File.WriteAllBytes(extractedPath, xwmaData);
            Console.WriteLine($"Extracted content saved as: {extractedPath}");
            count++;
        }
    }

    // 从指定目录中提取 xwma 文件
    public static void Extractxwmas(string directoryPath)
    {
        foreach (string filePath in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
        {
            if (Path.GetExtension(filePath).Equals(".xwma", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            ExtractxwmasFromFile(filePath);
        }
    }
}