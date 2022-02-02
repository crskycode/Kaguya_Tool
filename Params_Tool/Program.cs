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
            obj.Load(@"D:\Game\MamaHolic\params.dat");
            obj.DumpCgSet(@"D:\Game\MamaHolic\cgSet.json");
        }
    }
}
