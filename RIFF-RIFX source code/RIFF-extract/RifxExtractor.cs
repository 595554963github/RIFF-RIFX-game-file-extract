using System;
using System.IO;
using System.Collections.Generic;

class RifxExtractor
{
    // 定义 RIFX 文件头
    private static readonly byte[] RIFXHeader = { 0x52, 0x49, 0x46, 0x58 };
    // 定义 wem 数据块
    private static readonly byte[] wemBlock = { 0x57, 0x41, 0x56, 0x45, 0x66, 0x6D, 0x74 };

    // 从文件内容中提取 wem 数据
    private static IEnumerable<byte[]> ExtractwemData(byte[] fileContent)
    {
        int wemDataStart = 0;
        while ((wemDataStart = IndexOf(fileContent, RIFXHeader, wemDataStart)) != -1)
        {
            // 从文件内容中读取文件大小
            int fileSize = BitConverter.ToInt32(fileContent, wemDataStart + 4);
            // 确保文件大小是 4 字节对齐
            fileSize = (fileSize + 1) & ~1;

            int blockStart = wemDataStart + 8;
            bool haswemBlock = IndexOf(fileContent, wemBlock, blockStart) != -1;

            if (haswemBlock)
            {
                byte[] wemData = new byte[fileSize + 8];
                Array.Copy(fileContent, wemDataStart, wemData, 0, fileSize + 8);
                yield return wemData;
            }

            wemDataStart += fileSize + 8;
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

    // 从单个文件中提取 Wem 文件
    private static void ExtractwemsFromFile(string filePath)
    {
        byte[] fileContent = File.ReadAllBytes(filePath);
        int count = 0;
        foreach (byte[] wemData in ExtractwemData(fileContent))
        {
            string baseFilename = Path.GetFileNameWithoutExtension(filePath);
            string extractedFilename = $"{baseFilename}_{count}.Wem";
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
            File.WriteAllBytes(extractedPath, wemData);
            Console.WriteLine($"Extracted content saved as: {extractedPath}");
            count++;
        }
    }

    // 从指定目录中提取 wem 文件
    public static void Extractwems(string directoryPath)
    {
        foreach (string filePath in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
        {
            if (Path.GetExtension(filePath).Equals(".wem", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            ExtractwemsFromFile(filePath);
        }
    }
}