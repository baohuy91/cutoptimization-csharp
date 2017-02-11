using System.Collections.Generic;
namespace CutOptimzation
{
    public class BruteForceSolver
    {
        /**
         * This recursive generate all possible
         * This is O(2^n) function so it's only feasible for small amount of order
         * OrderSets must be DESC sorted
         *
         * @param orderSets   all orders
         * @param stockLength length of the bar in stock
         * @return a pair of minimum number of  required bar & patterns for each
         */
        public static Pair<int, List<List<BarSet>>> solve(List<BarSet> orderSets, double stockLength)
        {
            // Stop if there is no more bar
            if (isEmpty(orderSets))
            {
                List<List<BarSet>> bs = new List<List<BarSet>>();
                bs.Add(new List<BarSet>()); // Create array to let father func put order to
                return new Pair<int, List<List<BarSet>>>(0, bs);
            }

            // Generate all possible patterns
            List<List<BarSet>> possiblePatterns = calPossibleCutsFor1Stock(0, orderSets, stockLength);

            // With each pattern, recursive solve the problem with remain orders
            Pair<int, List<List<BarSet>>> minPattern = null;
            foreach (List<BarSet> pattern in possiblePatterns)
            {
                List<BarSet> remainOrderSets = new List<BarSet>();
                for (int iBar = 0; iBar < orderSets.Count; iBar++)
                {
                    int nUsedBar = 0;
                    if (pattern.Count > iBar)
                    {
                        nUsedBar = pattern[iBar].num;
                    }
                    remainOrderSets.Add(new BarSet(orderSets[iBar].len, orderSets[iBar].num - nUsedBar));
                }

                // Recursive solve
                Pair<int, List<List<BarSet>>> optimizedPattern = solve(remainOrderSets, stockLength);

                // Check if it new one better than current minimum
                if (minPattern == null || optimizedPattern.fst + 1 < minPattern.fst)
                {
                    optimizedPattern.snd.Add(pattern);
                    minPattern = new Pair<int, List<List<BarSet>>>(optimizedPattern.fst + 1, optimizedPattern.snd);
                }
            }

            return minPattern;
        }

        /**
         * This func generate all possible pattern to be fit in one stock len (or remain stock len)
         * from the @param{curOrderIndex} to end of orderSets
         */
        private static List<List<BarSet>> calPossibleCutsFor1Stock(int curOrderIndex, List<BarSet> orderSets, double stockLen)
        {
            var possiblePatterns = new List<List<BarSet>>();

            bool canCut = orderSets.Find(barSet => barSet.num > 0 && barSet.len <= stockLen) != null;
            if (!canCut)
            {
                possiblePatterns.Add(new List<BarSet>()); // Create array to let father func put bar in
                return possiblePatterns;
            }

            if (curOrderIndex == orderSets.Count)
            {
                return possiblePatterns;
            }

            BarSet curOrderSet = orderSets[curOrderIndex];
            int maxBarNum = minInt((int)(stockLen / curOrderSet.len), curOrderSet.num);
            for (int nBar = 0; nBar <= maxBarNum; nBar++)
            {
                // Clone current order except the one at current index
                List<BarSet> remainOrderSets = new List<BarSet>();
                remainOrderSets.AddRange(orderSets);
                remainOrderSets[curOrderIndex] = new BarSet(curOrderSet.len, curOrderSet.num - nBar);

                List<List<BarSet>> subPatterns = calPossibleCutsFor1Stock(
                        curOrderIndex + 1, remainOrderSets, stockLen - curOrderSet.len * nBar);

                int barNum = nBar;
                subPatterns.ForEach(c => c[0] = new BarSet(curOrderSet.len, barNum));
                possiblePatterns.AddRange(subPatterns);
            }

            return possiblePatterns;
        }

        private static int minInt(int a, int b)
        {
            return a > b ? b : a;
        }

        private static bool isEmpty(List<BarSet> barSets)
        {
            return barSets.Find(s => s.num > 0) == null;
        }
    }
}