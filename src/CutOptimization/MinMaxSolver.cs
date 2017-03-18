using System.Collections.Generic;
using CutOptimization;
namespace CutOptimization
{
    public static class MinMaxSolver
    {
        /**
         * Main solver with threshold
         *
         * @param orderSets list of object of order length & order's length
         * @param stockLen  raw bar length to cut
         * @return Map of cutting pattern and num of them
         */
        public static Dictionary<List<BarSet>, int> solve(List<BarSet> orderSets, double stockLen, double minLeftOver, double maxLeftOver)
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
                    if (pttrn.Count > 0){
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
    }
}