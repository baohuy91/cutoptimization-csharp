using System.Collections.Generic;

namespace CutOptimization
{
    public class UtilStatistics
    {
        public static double calWastedLen(Dictionary<List<BarSet>, int> patterns, double stockLen, double maxLeftover)
        {
            var totalWastedLen = 0d;
            foreach (var pattern in patterns)
            {
                var nPatternCut = pattern.Value;
                var remainLen = stockLen;
                foreach (var barSet in pattern.Key)
                {
                    remainLen -= barSet.len * barSet.num;
                }
                
                totalWastedLen += remainLen > 200 ? (remainLen - 200) * nPatternCut : remainLen * nPatternCut;
            }

            return totalWastedLen;
        }

        public static int sumInt(List<int> values){
            int sum = 0;
            foreach (var n in values)
            {
                sum += n;
            }
            return sum;
        }
    }
}