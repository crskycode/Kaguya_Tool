using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALP_Tool
{
    static class Utility
    {
        public static bool PathIsFolder(string path)
        {
            return new FileInfo(path).Attributes.HasFlag(FileAttributes.Directory);
        }
    }
}
