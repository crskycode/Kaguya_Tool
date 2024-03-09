using System;
using System.IO;
using System.Text;

namespace StrArcTool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (args.Length != 3)
            {
                Console.WriteLine("StrArcTool v1.0");
                Console.WriteLine("  created by Crsky");
                Console.WriteLine();
                Console.WriteLine("Usage:");
                Console.WriteLine("  Export text   : StrArcTool -e Shift_JIS tblstr.arc");
                Console.WriteLine("  Rebuild       : StrArcTool -b Shift_JIS tblstr.arc");
                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            var mode = args[0];
            var enc = args[1];
            var path = Path.GetFullPath(args[2]);

            switch (mode)
            {
                case "-e":
                {
                    var txtPath = Path.ChangeExtension(path, ".txt");

                    var image = new StrArc();
                    image.Load(path, enc);
                    image.Export(txtPath);

                    break;
                }
                case "-b":
                {
                    var txtPath = Path.ChangeExtension(path, ".txt");
                    var newPath = Path.ChangeExtension(path, ".new.arc");

                    var image = new StrArc();
                    image.Load(path, enc);
                    image.Import(txtPath);
                    image.Save(newPath, enc);

                    break;
                }
            }
        }
    }
}
