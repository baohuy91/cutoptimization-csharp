using System.Collections.Generic;
using System;
using lpsolve_win;

namespace CutOptimization
{
    public class ColumnGenerationSolver
    {
        private const int MAX_ITER = 100;

        /**
         * Main solver
         *
         * @param orderSets list of object of order length & order's length
         * @param stockLen  raw bar length to cut
         * @return Map of cutting pattern and num of them
         */
        public static Dictionary<List<BarSet>, int> solve(List<BarSet> orderSets, double stockLen)
        {
            var pair = solveByLinearProgramming(orderSets, stockLen);
            var pttrnMap = pair.fst;
            var remainOrderSets = pair.snd;

            var remainPttrns = solveRemainOrders(remainOrderSets, stockLen);

            // Add leftover to major result map
            foreach (List<BarSet> pattern in remainPttrns)
            {
                if (pattern.Count > 0)
                {
                    pttrnMap.Add(pattern, 1);
                }
            }

            return pttrnMap;
        }
        /**
         * Solve most of pattern by linear programming
         * @returns 1st value is optimized pattern, 2nd value is the remain orders that haven't been solved
         */
        public static Pair<Dictionary<List<BarSet>, int>, List<BarSet>> solveByLinearProgramming(List<BarSet> orderSets, double stockLen)
        {
            int nOrder = orderSets.Count;

            // Convert to Lp solve-friendly format: [arrayLen, x1, x2, 3...]
            double[] odLenVec = new double[nOrder + 1]; // order length vector
            double[] odNumVec = new double[nOrder + 1]; // order number vector
            odLenVec[0] = nOrder;
            odNumVec[0] = nOrder;
            for (int i = 0; i < nOrder; i++)
            {
                BarSet barSet = orderSets[i];
                odLenVec[i + 1] = barSet.len;
                odNumVec[i + 1] = barSet.num;
            }

            double[][] patternMat = genPatternMatrix(odLenVec, odNumVec, stockLen);
            double[] minPatternNums = new double[nOrder];

            // Lpsolve.Init(".");
            int iter;
            for (iter = 0; iter < MAX_ITER; iter++)
            {
                ;
                // Solve Linear Programming problem
                double[][] lpRst = calLP(patternMat, odNumVec);
                int solFlag = (int)lpRst[0][0];
                if (solFlag != (int)Lpsolve.lpsolve_return.OPTIMAL)
                {
                    Console.WriteLine("Can't solve anymore");
                    break;
                }
                minPatternNums = lpRst[1];
                double[] dualCostVector = lpRst[2];

                // Solve Knapsack problem
                double[][] ksRst = calKnapsack(dualCostVector, odLenVec, stockLen);
                double reducedCost = ksRst[0][0];
                double[] newPattern = ksRst[1];

                // TODO: use native optimized variable instead
                if (reducedCost <= 1.000000000001)
                { // epsilon threshold due to double value error
                    Console.WriteLine("Optimized");
                    break;
                }

                // Cal leaving column
                int[] lcRst = calLeavingColumn(patternMat, newPattern, minPatternNums);
                int lcSolFlag = lcRst[0];
                int leavingColIndex = lcRst[1];
                // Console.WriteLine("Leaving column index: {0}", leavingColIndex);

                if (lcSolFlag != (int)Lpsolve.lpsolve_return.OPTIMAL)
                {
                    Console.WriteLine("Can't solve anymore");
                    break;
                }

                // Save new pattern
                for (int r = 0; r < patternMat.Length; r++)
                {
                    patternMat[r][leavingColIndex] = newPattern[r];
                }


            }

            if (iter == MAX_ITER)
            {
                Console.WriteLine("Maximum of iter reached! Solution is not quite optimized");
            }

            // Console.WriteLine("Optimized pattern: ");
            // for (int r = 0; r < patternMat.Length; r++)
            // {
            //     Console.WriteLine("{0}: {1}", string.Join(", ", patternMat[r]), minPatternNums[r + 1]);
            // }

            // ------------ Round up to keep integer part of result ----------------
            double[] remainOdNumVec = new double[odNumVec.Length];
            odNumVec.CopyTo(remainOdNumVec, 0);
            for (int r = 0; r < patternMat.Length; r++)
            {
                double[] row = patternMat[r];
                for (int c = 1; c < row.Length; c++)
                {
                    remainOdNumVec[r + 1] -= row[c] * Math.Floor(minPatternNums[c]);
                }
            }
            // Console.WriteLine("Leftovers: {0}", string.Join(", ", remainOdNumVec));

            // Optimized for the rest of pattern by BruteForce
            List<BarSet> remainOrderSets = new List<BarSet>();
            for (int r = 1; r < remainOdNumVec.Length; r++)
            {
                if (remainOdNumVec[r] > 0)
                {
                    remainOrderSets.Add(new BarSet(odLenVec[r], (int)remainOdNumVec[r]));
                }
            }

            // Prepare result
            Dictionary<List<BarSet>, int> rstMap = new Dictionary<List<BarSet>, int>();
            for (int c = 1; c < nOrder + 1; c++)
            { // column
                if ((int)minPatternNums[c] == 0)
                { // if number of this pattern is zero, skip
                    continue;
                }

                List<BarSet> pattern = new List<BarSet>();
                for (int r = 0; r < nOrder; r++)
                { // row
                    if (patternMat[r][c] > 0d)
                    {
                        pattern.Add(new BarSet(odLenVec[r + 1], (int)patternMat[r][c]));
                    }
                }
                rstMap.Add(pattern, (int)minPatternNums[c]);
            }

            return new Pair<Dictionary<List<BarSet>, int>, List<BarSet>>(rstMap, remainOrderSets);
        }

        public static List<List<BarSet>> solveRemainOrders(List<BarSet> remainOrderSets, double stockLen)
        {
            if (remainOrderSets.Count == 0)
            {
                return new List<List<BarSet>>();
            }

            // Sort DESC
            remainOrderSets.Sort(delegate (BarSet a, BarSet b)
            {
                return Math.Sign(b.len - a.len);
            });

            // Solve
            var remainRst = BruteForceSolver.solve(remainOrderSets, stockLen);
            var pttrns = remainRst.snd;

            // Remove BarSets in patterns that have num = 0
            for (int i = 0; i < pttrns.Count; i++)
            {
                var trimPtrn = new List<BarSet>();
                foreach (var bs in pttrns[i])
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
         * Generate initial pattern matrix
         */
        private static double[][] genPatternMatrix(double[] odLenVec, double[] odNumVec, double stockLen)
        {
            int vecLen = odNumVec.Length;
            double[][] basicMatrix = new double[vecLen - 1][];
            for (int r = 1; r < vecLen; r++)
            {
                basicMatrix[r - 1] = new double[vecLen];
                basicMatrix[r - 1][r] = Math.Floor(stockLen / odLenVec[r]);
                basicMatrix[r - 1][0] = vecLen - 1;
            }
            return basicMatrix;
        }

        /**
         * @param basicMatrixIn the coefficient matrix of constraints
         * @return Array[2][m+1]
         * First row is just reduced cost
         * Second row is dual cost vector
         */
        private static double[][] calLP(double[][] basicMatrixIn, double[] orderNumVec)
        {
            double[][] basicMatrix = cloneMatrix(basicMatrixIn); // LP solve modifies our input array

            int nVar = basicMatrix.Length;
            IntPtr solver = Lpsolve.make_lp(0, nVar);
            Lpsolve.set_verbose(solver, 1);

            // add constraints
            for (int r = 0; r < nVar; r++)
            {
                Lpsolve.add_constraint(solver, basicMatrix[r], Lpsolve.lpsolve_constr_types.EQ, orderNumVec[r + 1]);
            }

            // set objective function
            double[] minCoef = new double[nVar + 1]; // coefficients
            for (int i = 0; i < minCoef.Length; i++)
            {
                minCoef[i] = 1;
            }
            minCoef[0] = nVar;
            Lpsolve.set_obj_fn(solver, minCoef);
            Lpsolve.set_minim(solver);

            // solve the problem
            int solFlag = (int)Lpsolve.solve(solver);

            // solution
            double[] rhs = new double[Lpsolve.get_Ncolumns(solver)];
            Lpsolve.get_variables(solver, rhs);
            double[] duals = new double[Lpsolve.get_Ncolumns(solver) + Lpsolve.get_Nrows(solver) + 1];
            Lpsolve.get_dual_solution(solver, duals);

            // delete the problem and free memory
            Lpsolve.delete_lp(solver);

            // Prepare result
            double[][] rst = new double[3][];
            rst[0] = new double[nVar + 1];
            rst[0][0] = solFlag;

            rst[1] = new double[nVar + 1];
            rst[1][0] = nVar;
            rhs.CopyTo(rst[1], 1);

            rst[2] = new double[nVar + 1];
            rst[2][0] = nVar;
            Array.Copy(duals, 1, rst[2], 1, nVar);
            return rst;
        }

        /**
         * Solve Knapsack problem
         *
         * @param objArr      objective array
         * @param orderLenVec order len
         * @param stockLen
         * @return 1st: reduced cost, 2nd new pattern to enter
         */
        private static double[][] calKnapsack(double[] objArr, double[] orderLenVec, double stockLen)
        {
            int nVar = objArr.Length - 1;
            IntPtr solver = Lpsolve.make_lp(0, nVar);
            Lpsolve.set_verbose(solver, 1);

            // add constraints
            Lpsolve.add_constraint(solver, orderLenVec, Lpsolve.lpsolve_constr_types.LE, stockLen);
            // objArr.length is equal to nCol + 1
            for (int c = 1; c <= nVar; c++)
            {
                Lpsolve.set_int(solver, c, 1);
            }

            // set objective function
            Lpsolve.set_obj_fn(solver, objArr);
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

        /**
         * Calculate which column index should be replaced
         *
         * @return index of the column that will leave the pattern matrix
         */
        private static int[] calLeavingColumn(double[][] basicMatrixIn, double[] newPattern, double[] rhs)
        {
            double[][] basicMatrix = cloneMatrix(basicMatrixIn); // LP solve modifies our input array

            int nVar = basicMatrix.Length;
            IntPtr solver = Lpsolve.make_lp(0, nVar);
            Lpsolve.set_verbose(solver, 1);

            // add constraints
            for (int r = 0; r < nVar; r++)
            {
                Lpsolve.add_constraint(solver, basicMatrix[r], Lpsolve.lpsolve_constr_types.EQ, newPattern[r]);
            }

            // set objective function
            double[] minCoef = new double[nVar + 1];
            for (int i = 0; i < minCoef.Length; i++)
            {
                minCoef[i] = 1;
            }
            minCoef[0] = nVar;
            Lpsolve.set_obj_fn(solver, minCoef);
            Lpsolve.set_minim(solver);

            // solve the problem
            int solFlag = (int)Lpsolve.solve(solver);

            // solution
            double[] var = new double[Lpsolve.get_Ncolumns(solver)];
            Lpsolve.get_variables(solver, var);

            // leaving column
            int minIndex = -1;
            double minVal = double.MaxValue;
            for (int i = 0; i < var.Length; i++)
            {
                if (var[i] > 0 && (rhs[i + 1] / var[i]) < minVal)
                {
                    minIndex = i;
                    minVal = rhs[i + 1] / var[i];
                }
            }

            // delete the problem and free memory
            Lpsolve.delete_lp(solver);

            return new int[] { solFlag, minIndex + 1 };
        }

        private static double[][] cloneMatrix(double[][] mat)
        {
            double[][] clone = new double[mat.Length][];
            for (int r = 0; r < mat.Length; r++)
            {
                clone[r] = new double[mat[r].Length];
                mat[r].CopyTo(clone[r], 0);
            }
            return clone;
        }
    }
}