using System;
using System.IO;
using System.Text;

namespace ScriptTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (args.Length < 3)
            {
                Console.WriteLine("Kaguya MSG Tool");
                Console.WriteLine("  -- Created by Crsky");
                Console.WriteLine("Usage:");
                Console.WriteLine("  Export text     : ScriptTool -e [ReadEncoding] [message.dat]");
                Console.WriteLine("  Rebuild script  : ScriptTool -b [ReadEncoding] [WriteEncoding] [message.dat]");
                Console.WriteLine();
                Console.WriteLine("Examples:");
                Console.WriteLine("  ScriptTool -e shift_jis message.dat");
                Console.WriteLine("  ScriptTool -b shift_jis gbk message.dat");
                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            string mode = args[0];

            switch (mode)
            {
                case "-e":
                {
                    if (args.Length != 3)
                    {
                        Console.WriteLine("ERROR： Missing parameters.");
                        return;
                    }

                    try
                    {
                        var encoding = args[1];
                        var filePath = Path.GetFullPath(args[2]);

                        var file = new MsgFile();
                        file.Load(filePath, encoding);
                        file.ExportText(Path.ChangeExtension(filePath, "txt"));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                    break;
                }
                case "-b":
                {
                    if (args.Length != 4)
                    {
                        Console.WriteLine("ERROR： Missing parameters.");
                        return;
                    }

                    try
                    {
                        var readEncoding = args[1];
                        var writeEncoding = args[2];
                        var filePath = Path.GetFullPath(args[3]);

                        string txtFilePath = Path.ChangeExtension(filePath, "txt");
                        string newFilePath = Path.ChangeExtension(filePath, "new.dat");
                        var file = new MsgFile();
                        file.Load(filePath, readEncoding);
                        file.ImportText(txtFilePath);
                        file.Save(newFilePath, writeEncoding);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                    break;
                }
            }
        }
    }
}
