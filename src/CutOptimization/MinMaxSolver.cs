using System.Collections.Generic;
namespace CutOptimization
{
    public class MinMaxSolver
    {
        private double minLeftOver;
        private double maxLeftOver;
        public MinMaxSolver(double minLeftOver, double maxLeftOver)
        {
            this.minLeftOver = minLeftOver;
            this.maxLeftOver = maxLeftOver;
        }

        /**
         * Main solver with threshold
         *
         * @param orderSets list of object of order length & order's length
         * @param stockLen  raw bar length to cut
         * @return Map of cutting pattern and num of them
         */
        public Dictionary<List<BarSet>, int> solve(List<BarSet> orderSets, double stockLen)
        {
            BarSets orders = new BarSets(orderSets);

            Dictionary<BarSets, int> pttrnMap = new Dictionary<BarSets, int>();
            while (orders.count() > 0)
            {
                BarSets pttrn = optimizeToOneStock(stockLen, orders);
                addPatternToDictionary(pttrnMap, pttrn);
                orders.substractAll(pttrn);
            }

            Dictionary<List<BarSet>, int> rstMap = new Dictionary<List<BarSet>, int>();
            foreach (KeyValuePair<BarSets, int> entry in pttrnMap)
            {
                rstMap.Add(entry.Key.getBarSets(), entry.Value);
            }
            return rstMap;
        }

        private static void addPatternToDictionary(Dictionary<BarSets, int> dic, BarSets barSets)
        {
            foreach (KeyValuePair<BarSets, int> entry in dic)
            {
                if (entry.Key.compareEqualWith(barSets))
                {
                    dic[entry.Key] += 1;
                    return;
                }
            }

            // If not exist add
            dic.Add(barSets, 1);
        }

        public Dictionary<List<BarSet>, int> solveWithColGen(List<BarSet> orderSets, double stockLen)
        {
            Pair<Dictionary<List<BarSet>, int>, List<BarSet>> rstPair = ColumnGenerationSolver.solveByLinearProgramming(orderSets, stockLen);
            Dictionary<List<BarSet>, int> originalPttrnMap = rstPair.fst;
            BarSets remainOrderSet = new BarSets(rstPair.snd);

            // Convert to convenient input
            Dictionary<BarSets, int> convertOriginalPttrnMap = new Dictionary<BarSets, int>();
            foreach (KeyValuePair<List<BarSet>, int> item in originalPttrnMap)
            {
                convertOriginalPttrnMap.Add(new BarSets(item.Key), item.Value);
            }

            Pair<Dictionary<BarSets, int>, BarSets> pair = removeInvalidBarFromAllPatterns(convertOriginalPttrnMap, stockLen, minLeftOver, maxLeftOver);
            Dictionary<BarSets, int> newPttrnMap = pair.fst;
            BarSets leftBarSets = pair.snd;

            if (leftBarSets.count() > 0)
            {
                // Solve 2nd time
                Pair<Dictionary<List<BarSet>, int>, List<BarSet>> rstPair2 = ColumnGenerationSolver.solveByLinearProgramming(leftBarSets.getBarSets(), stockLen);
                Dictionary<List<BarSet>, int> pttrnMap2 = rstPair2.fst;
                remainOrderSet.addAll(new BarSets(rstPair2.snd));

                // Add 2nd solution pattern to 1st pattern
                foreach (KeyValuePair<List<BarSet>, int> item in pttrnMap2)
                {
                    newPttrnMap.Add(new BarSets(item.Key), item.Value);
                }
            }

            // Solve remaining order
            if (remainOrderSet.count() > 0)
            {
                List<List<BarSet>> remainingPttrns = BruteForceSolver.solveMinMax(remainOrderSet.getBarSets(), stockLen, minLeftOver, maxLeftOver);
                // add to new pttrn map
                foreach (List<BarSet> pttrn in remainingPttrns)
                {
                    if (pttrn.Count > 0)
                    {
                        newPttrnMap.Add(new BarSets(pttrn), 1);
                    }
                }
            }

            // Revert back to return output
            Dictionary<List<BarSet>, int> revertNewPttrnMap = new Dictionary<List<BarSet>, int>();
            foreach (KeyValuePair<BarSets, int> item in newPttrnMap)
            {
                revertNewPttrnMap.Add(item.Key.getBarSets(), item.Value);
            }

            return revertNewPttrnMap;
        }

        public static Pair<Dictionary<BarSets, int>, BarSets> removeInvalidBarFromAllPatterns(Dictionary<BarSets, int> originalPttrn, double stockLen, double minLeftOver, double maxLeftOver)
        {
            Dictionary<BarSets, int> newPttnMap = new Dictionary<BarSets, int>();
            BarSets leftBarSets = new BarSets();
            foreach (KeyValuePair<BarSets, int> item in originalPttrn)
            {
                BarSets pttrn = item.Key;
                int nPttrn = item.Value;

                Pair<BarSets, BarSets> ret = removeInvalidBarFromPattern(pttrn, stockLen, minLeftOver, maxLeftOver);

                // Add new pattern to map
                BarSets newPttrn = ret.fst;
                newPttnMap.Add(newPttrn, nPttrn);

                // mutiply leaving bar set number and add to leaving bar sets
                foreach (BarSet bs in ret.snd.getBarSets())
                {
                    bs.num *= nPttrn;
                }
                leftBarSets.addAll(ret.snd);
            }

            return new Pair<Dictionary<BarSets, int>, BarSets>(newPttnMap, leftBarSets);
        }

        public static Pair<BarSets, BarSets> removeInvalidBarFromPattern(BarSets originalPattern, double stockLen, double minLeftOver, double maxLeftOver)
        {
            BarSets newPttrn = originalPattern.clone();
            newPttrn.sortAsc();
            BarSets leftBarSets = new BarSets();

            double optimalTotalLen = stockLen - minLeftOver;
            double reusuableTotalLen = stockLen - maxLeftOver;
            while (newPttrn.countTotalLen() < optimalTotalLen && newPttrn.countTotalLen() > reusuableTotalLen)
            {
                leftBarSets.addLen(newPttrn.popLen());
            }

            return new Pair<BarSets, BarSets>(newPttrn, leftBarSets);
        }

        public BarSets optimizeToOneStock(double stockLen, BarSets orders)
        {
            List<List<BarSet>> notFilteredMinMaxPttns = BruteForceSolver.calPossibleCutsFor1Stock(0, orders.getBarSets(), stockLen);

            BarSets optimizedBarSets = selectValidOptimizedBar(stockLen, notFilteredMinMaxPttns, minLeftOver, maxLeftOver);

            if (optimizedBarSets != null)
            {
                return optimizedBarSets;
            }

            // If we failed, try to get pattern with usable leftover
            List<List<BarSet>> PttnWithUsableLeftOver = BruteForceSolver.calPossibleCutsFor1Stock(0, orders.getBarSets(), stockLen - maxLeftOver);
            foreach (List<BarSet> pttn in PttnWithUsableLeftOver)
            {
                BarSets bs = new BarSets(pttn);
                if (optimizedBarSets == null)
                {
                    optimizedBarSets = bs;
                    continue;
                }

                double optimizedLeftOver = stockLen - optimizedBarSets.countTotalLen();
                double leftOver = stockLen - bs.countTotalLen();
                if (leftOver <= optimizedLeftOver)
                {
                    optimizedBarSets = bs;
                }
            }

            return optimizedBarSets;
        }

        private static BarSets selectValidOptimizedBar(double stockLen, List<List<BarSet>> notFilteredMinMaxPttns, double minLeftOver, double maxLeftOver)
        {
            BarSets optimizedBarSets = null;
            foreach (List<BarSet> pttn in notFilteredMinMaxPttns)
            {
                BarSets bs = new BarSets(pttn);
                double leftOver = stockLen - bs.countTotalLen();
                if (leftOver < maxLeftOver && leftOver > minLeftOver)
                {
                    continue;
                }

                if (optimizedBarSets == null)
                {
                    optimizedBarSets = bs;
                    continue;
                }

                double optimizedLeftOver = stockLen - optimizedBarSets.countTotalLen();
                if (leftOver <= optimizedLeftOver)
                {
                    optimizedBarSets = bs;
                }
            }

            return optimizedBarSets;
        }
    }
}