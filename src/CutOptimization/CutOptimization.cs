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
        * @return List of pre-format cutting pattern with number: "{stocklen} {numRequiredStock} {patterns...}"
        */
        public static List<string> calRequiredBarWithFormat(
                double stockLengthInput,
                double sawWidthInput,
                List<BarSet> orderSetInputs)
        {
            var rstMap = calRequiredBarCore(stockLengthInput, sawWidthInput, orderSetInputs);

            return toString(rstMap, stockLengthInput);
        }

        /**
        * Main calculation
        *
        * @param stockLengthInput raw bar length in stock
        * @param sawWidthInput    width of saw
        * @param orderSetInputs   required bar set
        * @param minLeftover      the lower bound of unacceptable remaining stock length after cutting
        * @param maxLeftover      the upper bound of unacceptable remaining stock length after cutting
        * @return List of pre-format cutting pattern with number: "{stocklen} {numRequiredStock} {patterns...}"
        */
        public static List<string> calRequiredBarMinMaxWithFormat(
                double stockLengthInput,
                double sawWidthInput,
                List<BarSet> orderSetInputs,
                double minLeftover,
                double maxLeftover)
        {
            var rstMap = calRequiredBarCoreMinMax(stockLengthInput, sawWidthInput, orderSetInputs, minLeftover, maxLeftover);

            return toString(rstMap, stockLengthInput);
        }

        private static List<string> toString(Dictionary<List<BarSet>, int> pttrnMap, double stockLen)
        {
            var patternStrs = new List<string>();
            foreach (var pattern in pttrnMap.Keys)
            {
                var pStrArr = new List<string>();
                // pattern.ForEach(p => pStrArr.Add(p.ToString()));
                foreach (BarSet bs in pattern)
                {
                    pStrArr.Add(bs.ToString());
                }
                string pStr = string.Join(" + ", pStrArr);
                patternStrs.Add(string.Format("{2} {0} {1}\n", pttrnMap[pattern], pStr, stockLen));
            };

            return patternStrs;
        }

        /**
         * Main calculation
         *
         * @param stockLengthInput raw bar length in stock
         * @param sawWidthInput    width of saw
         * @param orderSetInputs   required bar set
         * @return number of total required stock
         */
        public static int calRequiredBar(
                        double stockLengthInput,
                        double sawWidthInput,
                        List<BarSet> orderSetInputs)
        {
            var rstMap = calRequiredBarCore(stockLengthInput, sawWidthInput, orderSetInputs);

            return UtilStatistics.sumInt(new List<int>(rstMap.Values));
        }

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
            var pair = normalizeInput(stockLengthInput, sawWidthInput, orderSetInputs);
            double stockLength = pair.fst;
            var orderSets = pair.snd;

            // Solve CSP
            var rstMap = ColumnGenerationSolver.solve(orderSets, stockLength);

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
         * @return Dictionary of data with map between pattern and number of required stock for this pattern
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
            var pair = normalizeInput(stockLengthInput, sawWidthInput, orderSetInputs);
            double stockLength = pair.fst;
            var orderSets = pair.snd;

            // Solve CSP
            var rstMap = MinMaxSolver.solve(orderSets, stockLength, minLeftover, maxLeftover);

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
            var orderSets = new List<BarSet>();
            foreach (BarSet barSetIn in orderSetInputs)
            {
                orderSets.Add(new BarSet(barSetIn.len + sawWidthInput, barSetIn.num));
            }
            return new Pair<double, List<BarSet>>(stockLength, orderSets);
        }

        private static Dictionary<List<BarSet>, int> denomalizeResult(Dictionary<List<BarSet>, int> rstMap, double sawWidthInput)
        {
            // XXX: this function modify param
            foreach (var ptrn in rstMap.Keys)
            {
                foreach (var b in ptrn)
                {
                    b.len -= sawWidthInput;
                }
            }

            return rstMap;
        }

        // Print result to console
        private static void printToConsole(Dictionary<List<BarSet>, int> rstMap, double stockLen)
        {
            foreach (var pattern in rstMap.Keys)
            {
                var pStrArr = new List<string>();
                // pattern.ForEach(p => pStrArr.Add(p.ToString()));
                foreach (var p in pattern)
                {
                    pStrArr.Add(p.ToString());
                }
                string pStr = string.Join(" + ", pStrArr);
                Console.WriteLine("{2} {0} {1}\n", rstMap[pattern], pStr, stockLen);
            };
        }
    }
}