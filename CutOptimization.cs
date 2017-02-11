using System;
using System.Collections.Generic;

namespace CutOptimzation
{

    public class CutOptimization
    {
        /**
         * Main calculation
         *
         * @param stockLengthInput raw bar length in stock
         * @param sawWidthInput    width of saw
         * @param orderSetInputs   required bar set
         * @return num of require bar
         */
        public static int calRequiredBar(
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

            // Print result
            foreach (var pattern in rstMap.Keys)
            {
                string pStr = "";
                pattern.ForEach(p => pStr += p.toString() + ", ");
                Console.WriteLine("(x%d): %s\n", rstMap[pattern], pStr);
            };

            int totalStock = 0;
            foreach (var n in rstMap.Values)
            {
                totalStock += n;
            }
            return totalStock;
        }
    }
}