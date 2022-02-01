using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Params_Tool.Algorithms
{
    static class SuffixArray
    {
        public static int[] Construct(short[] str)
        {
            var suffixArray = new int[str.Length];
            var rankArray = new int[str.Length];
            var tempRankArray = new int[str.Length];

            for (var i = 0; i < str.Length; i++)
            {
                suffixArray[i] = i;
                rankArray[i] = str[i];
            }

            for (var k = 1; k < str.Length; k *= 2)
            {
                CountSort(suffixArray, rankArray, k); // sort SA[i] based on RA[SA[i]+k]
                CountSort(suffixArray, rankArray, 0);// sort SA[i] based on RA[SA[i]]

                var newRank = 0;
                tempRankArray[suffixArray[0]] = 0;

                for (var i = 1; i < rankArray.Length; i++)
                {

                    var curRank = rankArray[suffixArray[i]];
                    var prevRank = rankArray[suffixArray[i - 1]];
                    var curRankWithOffset = 0;
                    var prevRankWithOffset = 0;

                    if (suffixArray[i] + k < rankArray.Length)
                        curRankWithOffset = rankArray[suffixArray[i] + k];

                    if (suffixArray[i - 1] + k < rankArray.Length)
                        prevRankWithOffset = rankArray[suffixArray[i - 1] + k];

                    if (curRank != prevRank || curRankWithOffset != prevRankWithOffset)
                        newRank++;

                    tempRankArray[suffixArray[i]] = newRank;
                }

                Array.Copy(tempRankArray, rankArray, rankArray.Length);

            }
            return suffixArray;
        }

        // sort suffixArray based on rankArray
        // suffixArray[i] mapped to rankArray[i+rankArrayOffset]
        // time Complexity  : O(N)
        private static void CountSort(int[] suffixArray, int[] rankArray, int rankArrayOffset)
        {
            var freqLength = Math.Max(rankArray.Max() + 1, rankArray.Length);
            var frequency = new int[freqLength];
            var cumulativeFrequency = new int[freqLength]; // used as a start index
            var tempSuffixArray = new int[suffixArray.Length];

            for (var i = 0; i < rankArray.Length; i++)
            {
                var val = 0;
                if (i + rankArrayOffset < rankArray.Length)
                    val = rankArray[i + rankArrayOffset];
                frequency[val]++;
            }

            for (var i = 1; i < freqLength; i++)
                cumulativeFrequency[i] = cumulativeFrequency[i - 1] + frequency[i - 1];

            for (var i = 0; i < suffixArray.Length; i++)
            {
                int newIndex;
                if (suffixArray[i] + rankArrayOffset < rankArray.Length)
                {
                    newIndex = cumulativeFrequency[rankArray[suffixArray[i] + rankArrayOffset]];
                    cumulativeFrequency[rankArray[suffixArray[i] + rankArrayOffset]]++;
                }
                else
                {
                    newIndex = cumulativeFrequency[0];
                    cumulativeFrequency[0]++;
                }
                tempSuffixArray[newIndex] = suffixArray[i];
            }
            Array.Copy(tempSuffixArray, suffixArray, suffixArray.Length);
        }
    }
}
