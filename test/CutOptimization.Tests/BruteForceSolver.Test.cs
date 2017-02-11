using System.Collections.Generic;
using Xunit;

namespace CutOptimization.Tests
{
    public class BruteForceSolverTests
    {
        [TheoryAttribute]
        [InlineDataAttribute(1000d, new double[] { 600d, 200d }, new int[] { 4, 5 }, 4)]
        public void Test1(double stock, double[] orderLens, int[] orderNums, int nRequiredBar)
        {
            // Input
            double rawBarHeightInput = stock;
            var requiredBarSetInputs = new List<BarSet>();
            for (int i = 0; i < orderLens.Length; i++)
            {
                requiredBarSetInputs.Add(new BarSet(orderLens[i], orderNums[i]));
            }

            var pttnRst = BruteForceSolver.solve(requiredBarSetInputs, rawBarHeightInput);
            var nBar = pttnRst.fst;

            Assert.Equal(4, nBar);
        }
    }
}
