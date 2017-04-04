using System;
using System.Collections.Generic;
using lpsolve_win;
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
            List<BarSet> newOrders = removeOrderLongerThanStock(orderSets, stockLen);
            BarSets orders = new BarSets(newOrders);
            orders.sortDesc();

            Dictionary<BarSets, int> pttrnMap = new Dictionary<BarSets, int>();
            while (orders.count() > 0)
            {
                BarSets pttrn = optimizeToOneStock(stockLen, orders);
                if (pttrn == null)
                {
                    // Can't solve with condition, return empty result
                    break;
                }
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

        private static List<BarSet> removeOrderLongerThanStock(List<BarSet> orginalOrders, double stock)
        {
            List<BarSet> newOrders = new List<BarSet>();
            foreach (BarSet bs in orginalOrders)
            {
                if (bs.len <= stock)
                {
                    newOrders.Add(bs);
                }
            }

            return newOrders;
        }

        public BarSets optimizeToOneStock(double stockLen, BarSets orders)
        {
            BarSets optimizedBarSets = calKnapsack(stockLen, orders);
            if (isValidOptimizedPattern(stockLen, optimizedBarSets, minLeftOver, maxLeftOver))
            {
                return optimizedBarSets;
            }

            // If we failed, try to get pattern with usable leftover
            optimizedBarSets = calKnapsack(stockLen - maxLeftOver, orders);
            if (isValidOptimizedPattern(stockLen, optimizedBarSets, minLeftOver, maxLeftOver))
            {
                return optimizedBarSets;
            }

            return null;
        }

        /**
         * Select the most optimized cut but doesn't violate the min max condition
         * It won't return empty bar sets
         */
        private static bool isValidOptimizedPattern(double stockLen, BarSets pttn, double minLeftOver, double maxLeftOver)
        {
            if (pttn == null || pttn.isEmpty())
            {
                return false;
            }

            double leftOver = stockLen - pttn.countTotalLen();
            if (leftOver < maxLeftOver && leftOver > minLeftOver)
            {
                return false;
            }

            return true;
        }

        /**
         * If bar sets with same pattern is exist, add up the count only
         * if not exist, add new pattern with count = 1 to dictionary
         */
        public static void addPatternToDictionary(Dictionary<BarSets, int> dic, BarSets barSets)
        {
            List<BarSets> bsKeys = new List<BarSets>(dic.Keys);
            foreach (BarSets bsKey in bsKeys)
            {
                if (bsKey.compareEqualWith(barSets))
                {
                    dic[bsKey] += 1;
                    return;
                }
            }

            // If not exist add
            dic.Add(barSets, 1);
        }

        public static BarSets calKnapsack(double stockLen, BarSets orders)
        {
            List<BarSet> bsOrders = orders.getBarSets();
            double[] orderNumVec = new double[bsOrders.Count + 1];
            double[] orderLenVec = new double[bsOrders.Count + 1];
            orderNumVec[0] = bsOrders.Count;
            orderLenVec[0] = bsOrders.Count;
            for (int i = 0; i < bsOrders.Count; i++)
            {
                orderNumVec[i + 1] = bsOrders[i].num;
                orderLenVec[i + 1] = bsOrders[i].len;
            }

            double[][] rstMap = calKnapsackWithLpSolve(stockLen, orderNumVec, orderLenVec);
            double[] patternVec = rstMap[1];

            return filterEmptyBarInPattern(bsOrders, patternVec);
        }

        private static BarSets filterEmptyBarInPattern(List<BarSet> bsOrders, double[] patternVec)
        {
            List<BarSet> pattern = new List<BarSet>();
            for (int i = 0; i < patternVec.Length; i++)
            {
                int nBar = (int)patternVec[i];
                if (nBar > 0)
                {
                    pattern.Add(new BarSet(bsOrders[i].len, (int)patternVec[i]));
                }
            }

            return new BarSets(pattern);
        }

        private static double[][] calKnapsackWithLpSolve(double stockLen, double[] orderNumVec, double[] orderLenVec)
        {
            int nVar = orderNumVec.Length - 1;
            IntPtr solver = Lpsolve.make_lp(0, nVar);
            Lpsolve.set_verbose(solver, 1);

            // add constraints
            Lpsolve.add_constraint(solver, orderLenVec, Lpsolve.lpsolve_constr_types.LE, stockLen);
            // objArr.length is equal to nCol + 1
            for (int c = 1; c <= nVar; c++)
            {
                Lpsolve.set_int(solver, c, 1);

                double[] constraintVec = new double[nVar + 1];
                constraintVec[0] = nVar;
                for (int cIndex = 1; cIndex <= nVar; cIndex++)
                {
                    constraintVec[cIndex] = cIndex == c ? 1 : 0;
                }
                Lpsolve.add_constraint(solver, constraintVec, Lpsolve.lpsolve_constr_types.LE, orderNumVec[c]);
            }

            // set objective function
            Lpsolve.set_obj_fn(solver, orderLenVec);
            Lpsolve.set_maxim(solver);

            // solve the problem
            int solFlag = (int)Lpsolve.solve(solver);

            // solution
            double reducedCost = Lpsolve.get_objective(solver);
            double[] var = new double[Lpsolve.get_Ncolumns(solver)];
            Lpsolve.get_variables(solver, var);

            // delete the problem and free memory
            Lpsolve.delete_lp(solver);

            // Prepare result
            double[][] rst = new double[2][];
            rst[0] = new double[] { reducedCost };
            rst[1] = var;
            return rst;
        }
    }
}