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