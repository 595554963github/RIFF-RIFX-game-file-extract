using System;
using System.IO;
using System.Collections.Generic;

namespace RiffExtractors
{
    class WaveExtractor
    {
        // 定义 RIFF 文件头
        private static readonly byte[] riffHeader = { 0x52, 0x49, 0x46, 0x46 };
        // 定义 音频 数据块
        private static readonly byte[] audioBlock = { 0x57, 0x41, 0x56, 0x45, 0x66, 0x6D, 0x74 };

        // 从指定目录中提取指定扩展名的音频文件
        public static void ExtractFiles(string directoryPath, int choice)
        {
            string targetExtension = GetTargetExtension(choice);

            foreach (string filePath in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
                if (Path.GetExtension(filePath).Equals($".{targetExtension}", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                ExtractFromFile(filePath, targetExtension);
            }
        }

        // 根据用户选择获取目标文件扩展名
        private static string GetTargetExtension(int choice)
        {
            switch (choice)
            {
                case 1:
                    return "wem";
                case 2:
                    return "wav";
                case 3:
                    return "at3";
                case 4:
                    return "at9";
                case 5:
                    return "xma";
                default:
                    throw new ArgumentException("无效的选择，请输入 1 - 5 之间的数字。");
            }
        }

        // 从单个文件中提取目标音频文件
        private static void ExtractFromFile(string filePath, string targetExtension)
        {
            byte[] fileContent = File.ReadAllBytes(filePath);
            int count = 0;
            foreach (byte[] waveData in ExtractWaveData(fileContent))
            {
                string baseFilename = Path.GetFileNameWithoutExtension(filePath);
                string extractedFilename = $"{baseFilename}_{count}.{targetExtension}";
                string? dirName = Path.GetDirectoryName(filePath);
                string extractedPath;
                if (dirName != null)
                {
                    extractedPath = Path.Combine(dirName, extractedFilename);
                }
                else
                {
                    // 如果目录名为空，这里可以根据需求进行处理，比如使用当前目录
                    extractedPath = extractedFilename;
                }

                string? dirToCreate = Path.GetDirectoryName(extractedPath);
                if (dirToCreate != null)
                {
                    Directory.CreateDirectory(dirToCreate);
                }
                File.WriteAllBytes(extractedPath, waveData);
                Console.WriteLine($"Extracted content saved as: {extractedPath}");
                count++;
            }
        }

        // 从文件内容中提取音频数据
        private static IEnumerable<byte[]> ExtractWaveData(byte[] fileContent)
        {
            int waveDataStart = 0;
            while ((waveDataStart = IndexOf(fileContent, riffHeader, waveDataStart)) != -1)
            {
                // 从文件内容中读取文件大小
                int fileSize = BitConverter.ToInt32(fileContent, waveDataStart + 4);
                // 确保文件大小是 4 字节对齐
                fileSize = (fileSize + 1) & ~1;

                int blockStart = waveDataStart + 8;
                bool hasAudioBlock = IndexOf(fileContent, audioBlock, blockStart) != -1;

                if (hasAudioBlock)
                {
                    byte[] waveData = new byte[fileSize + 8];
                    Array.Copy(fileContent, waveDataStart, waveData, 0, fileSize + 8);
                    yield return waveData;
                }

                waveDataStart += fileSize + 8;
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
    }
}