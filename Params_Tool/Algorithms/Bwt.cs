using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Params_Tool.Algorithms
{
    static class Bwt
    {
        public static byte[] Transform(byte[] input, out int endIndex)
        {
            var output = new byte[input.Length];
            var newInput = new short[input.Length + 1];

            for (int i = 0; i < input.Length; i++)
                newInput[i] = (short)(input[i] + 1);

            newInput[input.Length] = 0;
            var suffixArray = SuffixArray.Construct(newInput);
            var end = 0;
            var outputInd = 0;
            for (var i = 0; i < suffixArray.Length; i++)
            {
                if (suffixArray[i] == 0)
                {
                    end = i;
                    continue;
                }
                output[outputInd] = (byte)(newInput[suffixArray[i] - 1] - 1);
                outputInd++;
            }
            endIndex = end - 1;

            return output;
        }

        public static byte[] InverseTransform(byte[] input, int startIndex)
        {
            var T1 = new int[input.Length];
            var T2 = new int[256];

            for (var i = 0; i < input.Length; i++)
            {
                T2[input[i]]++;
            }

            for (var i = 1; i < 256; i++)
            {
                T2[i] += T2[i - 1];
            }

            for (var i = input.Length - 1; i >= 0; i--)
            {
                T1[--T2[input[i]]] = i;
            }

            var output = new byte[input.Length];

            var index = T1[startIndex]; // <- 2F

            for (var i = 0; i < input.Length; i++)
            {
                output[i] = input[index];
                index = T1[index];
            }

            return output;
        }
    }
}
