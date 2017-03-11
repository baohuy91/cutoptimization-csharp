using System.Collections.Generic;

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
            var rstPair = ColumnGenerationSolver.solveByLinearProgramming(orderSets, stockLen);
            var originalPttrnMap = rstPair.fst;
            var remainOrderSet = new BarSets(rstPair.snd);

            // Convert to convenient input
            var convertOriginalPttrnMap = new Dictionary<BarSets, int>();
            foreach (var item in originalPttrnMap)
            {
                convertOriginalPttrnMap.Add(new BarSets(item.Key), item.Value);
            }

            var pair = removeInvalidBarFromAllPatterns(convertOriginalPttrnMap, stockLen, minLeftOver, maxLeftOver);
            var newPttrnMap = pair.fst;
            var leftBarSets = pair.snd;

            if (leftBarSets.count() > 0)
            {
                // Solve 2nd time
                var rstPair2 = ColumnGenerationSolver.solveByLinearProgramming(leftBarSets.getBarSets(), stockLen);
                var pttrnMap2 = rstPair2.fst;
                remainOrderSet.addAll(new BarSets(rstPair2.snd));

                // Add 2nd solution pattern to 1st pattern
                foreach (var item in pttrnMap2)
                {
                    newPttrnMap.Add(new BarSets(item.Key), item.Value);
                }
            }

            // Solve remaining order
            if (remainOrderSet.count() > 0)
            {
                var remainingPttrns = BruteForceSolver.solveMinMax(remainOrderSet.getBarSets(), stockLen, minLeftOver, maxLeftOver);
                // add to new pttrn map
                foreach (var pttrn in remainingPttrns)
                {
                    if (pttrn.Count > 0){
                        newPttrnMap.Add(new BarSets(pttrn), 1);
                    }
                }
            }

            // Revert back to return output
            var revertNewPttrnMap = new Dictionary<List<BarSet>, int>();
            foreach (var item in newPttrnMap)
            {
                revertNewPttrnMap.Add(item.Key.getBarSets(), item.Value);
            }

            return revertNewPttrnMap;
        }

        public static Pair<Dictionary<BarSets, int>, BarSets> removeInvalidBarFromAllPatterns(Dictionary<BarSets, int> originalPttrn, double stockLen, double minLeftOver, double maxLeftOver)
        {
            var newPttnMap = new Dictionary<BarSets, int>();
            var leftBarSets = new BarSets();
            foreach (var item in originalPttrn)
            {
                var pttrn = item.Key;
                var nPttrn = item.Value;

                var ret = removeInvalidBarFromPattern(pttrn, stockLen, minLeftOver, maxLeftOver);

                // Add new pattern to map
                var newPttrn = ret.fst;
                newPttnMap.Add(newPttrn, nPttrn);

                // mutiply leaving bar set number and add to leaving bar sets
                foreach (var bs in ret.snd.getBarSets())
                {
                    bs.num *= nPttrn;
                }
                leftBarSets.addAll(ret.snd);
            }

            return new Pair<Dictionary<BarSets, int>, BarSets>(newPttnMap, leftBarSets);
        }

        public static Pair<BarSets, BarSets> removeInvalidBarFromPattern(BarSets originalPattern, double stockLen, double minLeftOver, double maxLeftOver)
        {
            var newPttrn = originalPattern.clone();
            newPttrn.sortAsc();
            var leftBarSets = new BarSets();

            var optimalTotalLen = stockLen - minLeftOver;
            var reusuableTotalLen = stockLen - maxLeftOver;
            while (newPttrn.countTotalLen() < optimalTotalLen && newPttrn.countTotalLen() > reusuableTotalLen)
            {
                leftBarSets.addLen(newPttrn.popLen());
            }

            return new Pair<BarSets, BarSets>(newPttrn, leftBarSets);
        }
    }
}