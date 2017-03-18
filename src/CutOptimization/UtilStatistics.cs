using System.Collections.Generic;

namespace CutOptimization
{
    public class UtilStatistics
    {
        public static double calWastedLen(Dictionary<List<BarSet>, int> patterns, double stockLen, double maxLeftover)
        {
            double totalWastedLen = 0d;
            foreach (KeyValuePair<List<BarSet>, int> pattern in patterns)
            {
                int nPatternCut = pattern.Value;
                double remainLen = stockLen;
                foreach (BarSet barSet in pattern.Key)
                {
                    remainLen -= barSet.len * barSet.num;
                }
                
                totalWastedLen += remainLen > 200 ? (remainLen - 200) * nPatternCut : remainLen * nPatternCut;
            }

            return totalWastedLen;
        }

        public static int sumInt(List<int> values){
            int sum = 0;
            foreach (int n in values)
            {
                sum += n;
            }
            return sum;
        }

        public static double calTotalBar(Dictionary<List<BarSet>, int> patterns)
        {
            double totalBar = 0;
            foreach (KeyValuePair<List<BarSet>, int> pattern in patterns)
            {
                int nBar = 0;
                foreach (BarSet barSet in pattern.Key)
                {
                    nBar += barSet.num;
                }
                
                int nPatternCut = pattern.Value;
                totalBar += nBar * nPatternCut;
            }

            return totalBar;
        }
    }
}