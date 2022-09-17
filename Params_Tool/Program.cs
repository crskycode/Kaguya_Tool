using System;
using System.IO;
using System.Text;

namespace Params_Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            var obj = new GameScriptParams();
            obj.Load(@"params.original.dat");
            obj.SaveToJson(@"params.original.json");
        }
    }
}
