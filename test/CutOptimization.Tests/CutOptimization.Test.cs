using System.Collections.Generic;
using Xunit;

namespace CutOptimization.Tests
{
    public class CutOptimizationTests
    {
        [TheoryAttribute]
        [InlineDataAttribute(1000d, 1.5d, new double[] { 600d, 200d, 100d }, new int[] { 50, 50, 10 }, 50)]
        [InlineDataAttribute(1000d, 0d, new double[] { 600d, 200d }, new int[] { 4, 5 }, 4)]
        [InlineDataAttribute(1000d, 0d, new double[] { 600d, 200d }, new int[] { 2, 4 }, 2)]
        [InlineDataAttribute(1100d, 0d, new double[] { 500d, 200d }, new int[] { 4, 12 }, 4)]
        [InlineDataAttribute(6000d, 0d, new double[] { 2158d, 1656d, 1458d, 734d, 646d, 546d }, new int[] { 1065, 83, 565, 565, 556, 556 }, 738)]
        public void TestcalRequiredBar(double stock, double saw, double[] orderLens, int[] orderNums, int nRequiredBar)
        {
            // Input
            double rawBarHeightInput = stock;
            double sawWidthInput = saw;
            var requiredBarSetInputs = new List<BarSet>();
            for (int i = 0; i < orderLens.Length; i++)
            {
                requiredBarSetInputs.Add(new BarSet(orderLens[i], orderNums[i]));
            }

            var nBar = CutOptimization.calRequiredBar(rawBarHeightInput, sawWidthInput, requiredBarSetInputs);

            Assert.Equal(nRequiredBar, nBar);
        }
    }
}
