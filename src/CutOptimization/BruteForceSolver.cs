using System.Collections.Generic;
using System;
namespace CutOptimization
{
    public static class BruteForceSolver
    {
        /**
         * This recursive generate all possible
         * This is O(2^n) function so it's only feasible for small amount of order
         *
         * @param orderSets   all orders
         * @param stockLength length of the bar in stock
         * @return patterns of minimum number
         */
        public static List<List<BarSet>> solve(List<BarSet> orderSets, double stockLength)
        {
            return solveWithCleanUp(orderSets, stockLength, false, 0, 0);
        }

        public static List<List<BarSet>> solveMinMax(List<BarSet> orderSets, double stockLength, double minLeftover, double maxLeftover)
        {
            return solveWithCleanUp(orderSets, stockLength, true, minLeftover, maxLeftover);
        }
        public static List<List<BarSet>> solveWithCleanUp(List<BarSet> orderSets, double stockLength, bool useMinMaxFilter, double minLeftover, double maxLeftover)
        {
            // Sort DESC
            orderSets.Sort(delegate (BarSet a, BarSet b)
            {
                return Math.Sign(b.len - a.len);
            });

            Pair<int, List<List<BarSet>>> remainRst = BruteForceSolver.solveRecursive(orderSets, stockLength, useMinMaxFilter, minLeftover, maxLeftover);
            List<List<BarSet>> pttrns = remainRst.snd;

            // Remove BarSets in patterns that have num = 0
            for (int i = 0; i < pttrns.Count; i++)
            {
                List<BarSet> trimPtrn = new List<BarSet>();
                foreach (BarSet bs in pttrns[i])
                {
                    if (bs.num > 0)
                    {
                        trimPtrn.Add(bs);
                    }
                }
                pttrns[i] = trimPtrn;
            }

            return pttrns;
        }

        /**
         * This recursive generate all possible
         * This is O(2^n) function so it's only feasible for small amount of order
         * OrderSets must be DESC sorted
         *
         * @param orderSets   all orders
         * @param stockLength length of the bar in stock
         * @return a pair of minimum number of  required bar & patterns for each
         */
        public static Pair<int, List<List<BarSet>>> solveRecursive(List<BarSet> orderSets, double stockLength, bool useMinMaxFilter, double minLeftover, double maxLeftover)
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
            if (useMinMaxFilter){
                possiblePatterns = applyMinMaxFilter(possiblePatterns, stockLength, minLeftover, maxLeftover);
            }
            

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
                Pair<int, List<List<BarSet>>> optimizedPattern = solveRecursive(remainOrderSets, stockLength, useMinMaxFilter, minLeftover, maxLeftover);

                // XXX: dangerous here
                if (optimizedPattern == null){
                    continue;
                }
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
            List<List<BarSet>> possiblePatterns = new List<List<BarSet>>();

            // bool canCut = orderSets.Find(barSet => barSet.num > 0 && barSet.len <= stockLen) != null;
            bool canCut = checkCanCut(orderSets, stockLen);
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
                // subPatterns.ForEach(c => c.Insert(0, new BarSet(curOrderSet.len, barNum)));
                foreach (List<BarSet> pttrn in subPatterns)
                {
                    pttrn.Insert(0, new BarSet(curOrderSet.len, barNum));
                }
                possiblePatterns.AddRange(subPatterns);
            }

            return possiblePatterns;
        }

        public static List<List<BarSet>> applyMinMaxFilter(List<List<BarSet>> possiblePttrns, double stockLen, double minLeftover, double maxLeftover)
        {
            List<List<BarSet>> filteredPttrns = new List<List<BarSet>>();
            foreach (List<BarSet> pttrn in possiblePttrns)
            {
                double leftover = stockLen - new BarSets(pttrn).countTotalLen();
                if (leftover <= minLeftover || leftover >= maxLeftover)
                {
                    filteredPttrns.Add(pttrn);
                }
            }

            return filteredPttrns;
        }

        private static int minInt(int a, int b)
        {
            return a > b ? b : a;
        }

        private static bool isEmpty(List<BarSet> barSets)
        {
            // return barSets.TrueForAll(s => s.num == 0);
            foreach (BarSet s in barSets)
            {
                if (s.num > 0)
                {
                    return false;
                }
            }

            return true;
        }

        /**
         * Check with remanining stockLen, whether we can still cut
         */
        private static bool checkCanCut(List<BarSet> orderSets, double stockLen)
        {
            foreach (BarSet barSet in orderSets)
            {
                if (barSet.num > 0 && barSet.len <= stockLen)
                {
                    return true;
                }
            }

            return false;
        }
    }
}