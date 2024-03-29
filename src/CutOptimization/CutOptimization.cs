using System;
using System.Collections.Generic;

namespace CutOptimization
{


    public class CutOptimization
    {
        /**
         * Main calculation
         *
         * @param stockLengthInput raw bar length in stock
         * @param sawWidthInput    width of saw
         * @param orderSetInputs   required bar set
         * @return Dictionary of data with map between pattern and number of required stock for this pattern
         */
        public static Dictionary<List<BarSet>, int> calRequiredBarCore(
                double stockLengthInput,
                double sawWidthInput,
                List<BarSet> orderSetInputs)
        {
            Console.WriteLine(string.Format("-----Bar len: {0}-----\n", stockLengthInput));

            // Normalize problem by remove saw width to Cutting Stock Problem (CSP)
            Pair<double, List<BarSet>> pair = normalizeInput(stockLengthInput, sawWidthInput, orderSetInputs);
            double stockLength = pair.fst;
            List<BarSet> orderSets = pair.snd;

            // Solve CSP
            Dictionary<List<BarSet>, int> rstMap = ColumnGenerationSolver.solve(orderSets, stockLength);

            // Convert problem back to before normalized
            rstMap = denomalizeResult(rstMap, sawWidthInput);

            printToConsole(rstMap, stockLengthInput);

            return rstMap;
        }

        /**
         * Main calculation
         *
         * @param stockLengthInput raw bar length in stock
         * @param sawWidthInput    width of saw
         * @param orderSetInputs   required bar set
         * @param minLeftover      the lower bound of unacceptable remaining stock length after cutting
         * @param maxLeftover      the upper bound of unacceptable remaining stock length after cutting
         * @return Dictionary of data with map between pattern and number of required stock for this pattern,
         *         if can't cut, return EMPTY dictionary
         */
        public static Dictionary<List<BarSet>, int> calRequiredBarCoreMinMax(
                double stockLengthInput,
                double sawWidthInput,
                List<BarSet> orderSetInputs,
                double minLeftover,
                double maxLeftover)
        {
            Console.WriteLine(string.Format("-----Bar len: {0} bound:{1}-{2}-----\n", stockLengthInput, minLeftover, maxLeftover));

            // Normalize problem by remove saw width to Cutting Stock Problem (CSP)
            Pair<double, List<BarSet>> pair = normalizeInput(stockLengthInput, sawWidthInput, orderSetInputs);
            double stockLength = pair.fst;
            List<BarSet> orderSets = pair.snd;

            // Solve CSP
            MinMaxSolver solver = new MinMaxSolver(minLeftover, maxLeftover);
            Dictionary<List<BarSet>, int> rstMap = solver.solve(orderSets, stockLength);

            // Convert problem back to before normalized
            rstMap = denomalizeResult(rstMap, sawWidthInput);

            printToConsole(rstMap, stockLengthInput);

            return rstMap;
        }

        private static Pair<double, List<BarSet>> normalizeInput(double stockLengthInput,
                double sawWidthInput,
                List<BarSet> orderSetInputs)
        {
            double stockLength = stockLengthInput + sawWidthInput;
            List<BarSet> orderSets = new List<BarSet>();
            foreach (BarSet barSetIn in orderSetInputs)
            {
                orderSets.Add(new BarSet(barSetIn.len + sawWidthInput, barSetIn.num));
            }
            return new Pair<double, List<BarSet>>(stockLength, orderSets);
        }

        private static Dictionary<List<BarSet>, int> denomalizeResult(Dictionary<List<BarSet>, int> rstMap, double sawWidthInput)
        {
            // XXX: this function modify param
            foreach (List<BarSet> ptrn in rstMap.Keys)
            {
                foreach (BarSet b in ptrn)
                {
                    b.len -= sawWidthInput;
                }
            }

            return rstMap;
        }

        // Print result to console
        private static void printToConsole(Dictionary<List<BarSet>, int> rstMap, double stockLen)
        {
            foreach (List<BarSet> pattern in rstMap.Keys)
            {
                List<string> pStrArr = new List<string>();
                // pattern.ForEach(p => pStrArr.Add(p.ToString()));
                foreach (BarSet p in pattern)
                {
                    pStrArr.Add(p.ToString());
                }
                string pStr = string.Join(" + ", pStrArr.ToArray());
                Console.WriteLine("{2} {0} {1}\n", rstMap[pattern], pStr, stockLen);
            };
        }
    }
}