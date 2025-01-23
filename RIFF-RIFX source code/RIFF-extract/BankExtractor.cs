using System;
using System.IO;
using System.Collections.Generic;

namespace RiffExtractors
{
    class BankExtractor
    {
        // 定义 RIFF 文件头
        private static readonly byte[] riffHeader = { 0x52, 0x49, 0x46, 0x46 };
        // 定义 bank 数据块
        private static readonly byte[] bankBlock = { 0x46, 0x45, 0x56, 0x20, 0x46, 0x4D, 0x54 };

        // 从文件内容中提取 bank 数据
        private static List<byte[]> ExtractbankData(byte[] fileContent)
        {
            List<byte[]> bankDataList = new List<byte[]>();
            int bankDataStart = 0;
            while ((bankDataStart = IndexOf(fileContent, riffHeader, bankDataStart)) != -1)
            {
                try
                {
                    // 从文件内容中读取文件大小
                    int fileSize = BitConverter.ToInt32(fileContent, bankDataStart + 4);
                    // 确保文件大小是 4 字节对齐
                    fileSize = (fileSize + 1) & ~1;

                    int blockStart = bankDataStart + 8;
                    bool hasbankBlock = IndexOf(fileContent, bankBlock, blockStart) != -1;

                    if (hasbankBlock)
                    {
                        byte[] bankData = new byte[fileSize + 8];
                        Array.Copy(fileContent, bankDataStart, bankData, 0, fileSize + 8);
                        bankDataList.Add(bankData);
                    }

                    bankDataStart += fileSize + 8;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error extracting bank data: {ex.Message}");
                }
            }
            return bankDataList;
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

        // 从单个文件中提取 bank 文件
        private static void ExtractbanksFromFile(string filePath)
        {
            try
            {
                byte[] fileContent = File.ReadAllBytes(filePath);
                List<byte[]> bankDataList = ExtractbankData(fileContent);
                int count = 0;
                foreach (byte[] bankData in bankDataList)
                {
                    string baseFilename = Path.GetFileNameWithoutExtension(filePath);
                    string extractedFilename = $"{baseFilename}_{count}.bank";
                    string? dirName = Path.GetDirectoryName(filePath);
                    string extractedPath;
                    if (dirName != null)
                    {
                        extractedPath = Path.Combine(dirName, extractedFilename);
                    }
                    else
                    {
                        // 如果目录名为空，使用当前目录
                        extractedPath = Path.Combine(Directory.GetCurrentDirectory(), extractedFilename);
                    }

                    string? dirToCreate = Path.GetDirectoryName(extractedPath);
                    if (dirToCreate != null)
                    {
                        Directory.CreateDirectory(dirToCreate);
                    }
                    File.WriteAllBytes(extractedPath, bankData);
                    Console.WriteLine($"Extracted content saved as: {extractedPath}");
                    count++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
            }
        }

        // 从指定目录中提取 bank 文件
        public static void Extractbanks(string directoryPath)
        {
            try
            {
                foreach (string filePath in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
                {
                    if (Path.GetExtension(filePath).Equals(".bank", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    ExtractbanksFromFile(filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing directory {directoryPath}: {ex.Message}");
            }
        }
    }
}