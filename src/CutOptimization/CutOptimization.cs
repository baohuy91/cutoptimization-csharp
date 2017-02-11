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
        public static List<string> calRequiredBarWithOutPut(
                double stockLengthInput,
                double sawWidthInput,
                List<BarSet> orderSetInputs)
        {
            var rstMap = calRequiredBarCore(stockLengthInput, sawWidthInput, orderSetInputs);

            // result
            var patternStrs = new List<string>();
            foreach (var pattern in rstMap.Keys)
            {
                var pStrArr = new List<string>();
                pattern.ForEach(p => pStrArr.Add(p.ToString()));
                string pStr = string.Join(" + ", pStrArr);
                patternStrs.Add(string.Format("{2} {0} {1}\n", rstMap[pattern], pStr, stockLengthInput));
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

            int totalStock = 0;
            foreach (var n in rstMap.Values)
            {
                totalStock += n;
            }
            return totalStock;
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
            Console.WriteLine("-----Bar len: %s-----\n", stockLengthInput);

            // Normalize problem by remove saw width to Cutting Stock Problem (CSP)
            double stockLength = stockLengthInput + sawWidthInput;
            List<BarSet> orderSets = new List<BarSet>();
            orderSetInputs.ForEach(barSetIn => orderSets.Add(new BarSet(barSetIn.len + sawWidthInput, barSetIn.num)));

            // Solve CSP
            Dictionary<List<BarSet>, int> rstMap = ColumnGenerationSolver.solve(orderSets, stockLength);

            // Convert problem back to before normalized
            foreach (var ptrn in rstMap.Keys)
            {
                ptrn.ForEach(b => b.len -= sawWidthInput);
            };

            // Print result to console
            foreach (var pattern in rstMap.Keys)
            {
                var pStrArr = new List<string>();
                pattern.ForEach(p => pStrArr.Add(p.ToString()));
                string pStr = string.Join(" + ", pStrArr);
                Console.WriteLine("{2} {0} {1}\n", rstMap[pattern], pStr, stockLengthInput);
            };

            return rstMap;
        }
    }
}