using System;
using System.IO;

namespace ALP_Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Kaguya ALP Tool");
                Console.WriteLine("  -- Created by Crsky");
                Console.WriteLine("Usage:");
                Console.WriteLine("  Extract   : ALP_Tool -e [ap2] [image.alp|folder]");
                Console.WriteLine("  Create    : ALP_Tool -c [ap2] [image.png|folder]");
                Console.WriteLine();
                Console.WriteLine("Help:");
                Console.WriteLine("  This tool is only works with 'AP-2' files,");
                Console.WriteLine("    please check the file header first.");
                Console.WriteLine("  Metadata (.json) and image (.png) are required to build ALP.");
                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            var mode = args[0];
            var vers = args[1];
            var path = Path.GetFullPath(args[2]);

            switch (mode)
            {
                case "-e":
                {
                    void Extract(string filePath)
                    {
                        Console.WriteLine($"Extracting image from {Path.GetFileName(filePath)}");

                        try
                        {
                            if (vers == "ap2")
                            {
                                var pngPath = Path.ChangeExtension(filePath, ".png");
                                AP2.Extract(filePath, pngPath);
                            }
                            else
                            {
                                Console.WriteLine("ERROR: Specified file version is not supported.");
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }

                    if (Utility.PathIsFolder(path))
                    {
                        foreach (var item in Directory.EnumerateFiles(path, "*.alp"))
                        {
                            Extract(item);
                        }
                    }
                    else
                    {
                        Extract(path);
                    }

                    break;
                }
                case "-c":
                {
                    void Create(string filePath)
                    {
                        Console.WriteLine($"Creating image from {Path.GetFileName(filePath)}");

                        try
                        {
                            if (vers == "ap2")
                            {
                                var alpPath = Path.ChangeExtension(filePath, ".alp");
                                AP2.Create(alpPath, filePath);
                            }
                            else
                            {
                                Console.WriteLine("ERROR: Specified file version is not supported.");
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }

                    if (Utility.PathIsFolder(path))
                    {
                        foreach (var item in Directory.EnumerateFiles(path, "*.png"))
                        {
                            Create(item);
                        }
                    }
                    else
                    {
                        Create(path);
                    }

                    break;
                }
            }
        }
    }
}
