using System;
using System.IO;
using RiffExtractors;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("请输入要处理的文件夹路径: ");
        string? inputPath = Console.ReadLine();

        if (string.IsNullOrEmpty(inputPath) || !Directory.Exists(inputPath))
        {
            Console.WriteLine($"错误: {inputPath} 不是一个有效的目录。");
            return;
        }

        Console.WriteLine("请选择提取方式:");
        Console.WriteLine("1. RIFF - wem");
        Console.WriteLine("2. RIFF - wav");
        Console.WriteLine("3. RIFF - at3");
        Console.WriteLine("4. RIFF - at9");
        Console.WriteLine("5. RIFF - xma");
        Console.WriteLine("6. riff - fev - bank");
        Console.WriteLine("7. riff - webp - webp");
        Console.WriteLine("8. riff - xwma - xwma");
        Console.WriteLine("9. RIFX - wem");

        if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > 9)
        {
            Console.WriteLine("无效的选择。");
            return;
        }

        string dirPath = inputPath;
        switch (choice)
        {
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
                WaveExtractor.ExtractFiles(dirPath, choice);
                break;
            case 6:
                BankExtractor.Extractbanks(dirPath);
                break;
            case 7:
                WebpExtractor.ExtractWebps(dirPath);
                break;
            case 8:
                XwmaExtractor.Extractxwmas(dirPath);
                break;
            case 9:
                RifxExtractor.Extractwems(dirPath);
                break;
        }
    }
}