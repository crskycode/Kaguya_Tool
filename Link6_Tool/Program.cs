using System;
using System.IO;
using System.Text;

namespace Link6_Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (args.Length != 4)
            {
                Console.WriteLine("Kaguya LINK6 Tool");
                Console.WriteLine("Usage:");
                Console.WriteLine("  Create LINK6 archive : Link6_Tool -c [output.arc] [root directory path] [archive name]");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            var mode = args[0];
            var outputPath = Path.GetFullPath(args[1]);
            var rootPath = Path.GetFullPath(args[2]);
            var archiveName = args[3];

            if (mode == "-c")
            {
                try
                {
                    Link6.Create(outputPath, rootPath, archiveName);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }
        }
    }
}
