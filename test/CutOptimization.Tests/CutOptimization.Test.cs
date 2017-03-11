using System.Collections.Generic;
using Xunit;

namespace CutOptimization.Tests
{
    public class CutOptimizationTests
    {
        [TheoryAttribute]
        [InlineDataAttribute(1000d, 1.5d, new double[] { 600d, 200d, 100d }, new int[] { 50, 50, 10 }, 50, 9000d, 0.18)]
        [InlineDataAttribute(1000d, 0d, new double[] { 600d, 200d }, new int[] { 4, 5 }, 4, 400d, 0.1)]
        [InlineDataAttribute(1000d, 0d, new double[] { 600d, 200d }, new int[] { 2, 4 }, 2, 0d, 0.00)]
        [InlineDataAttribute(1100d, 0d, new double[] { 500d, 200d }, new int[] { 4, 12 }, 4, 0d, 0.00)]
        [InlineDataAttribute(6000d, 0d, new double[] { 2158d, 1656d, 1458d, 734d, 646d, 546d }, new int[] { 1065, 83, 565, 565, 556, 556 }, 738, 38050d, 0.01)]
        [InlineDataAttribute(1000d, 0d, new double[] { 150d }, new int[] { 10 }, 2, 300d, 0.15)]
        public void TestcalRequiredBarCore(double stock, double saw, double[] orderLens, int[] orderNums, int nRequiredBar, double expectedWastedLen, double lossRatio)
        {
            // Input
            double rawBarHeightInput = stock;
            double sawWidthInput = saw;
            var requiredBarSetInputs = new List<BarSet>();
            for (int i = 0; i < orderLens.Length; i++)
            {
                requiredBarSetInputs.Add(new BarSet(orderLens[i], orderNums[i]));
            }

            // Exercise
            var ret = CutOptimization.calRequiredBarCore(rawBarHeightInput, sawWidthInput, requiredBarSetInputs);

            // Assert
            int nTotalBar = UtilStatistics.sumInt(new List<int>(ret.Values));
            var wastedLen = UtilStatistics.calWastedLen(ret, stock, 200d);

            Assert.Equal(nRequiredBar, nTotalBar);
            Assert.Equal(expectedWastedLen, wastedLen);
            Assert.InRange(wastedLen / (nTotalBar * stock), lossRatio - 0.01, lossRatio);
        }

        [TheoryAttribute]
        [InlineDataAttribute(1000d, 1.5d, 30d, 200d, new double[] { 600d, 200d, 100d }, new int[] { 50, 50, 10 }, 63 /*50*/, 11800 /*9000d*/, 0.19 /*0.18*/)]
        [InlineDataAttribute(1000d, 0d, 30d, 200d, new double[] { 600d, 200d }, new int[] { 4, 5 }, 4, 400d, 0.1)]
        [InlineDataAttribute(1000d, 0d, 30d, 200d, new double[] { 600d, 200d }, new int[] { 2, 4 }, 2, 0d, 0.00)]
        [InlineDataAttribute(1100d, 0d, 30d, 200d, new double[] { 500d, 200d }, new int[] { 4, 12 }, 4, 0d, 0.00)]
        [InlineDataAttribute(6000d, 0d, 30d, 200d, new double[] { 2158d, 1656d, 1458d, 734d, 646d, 546d }, new int[] { 1065, 83, 565, 565, 556, 556 }, 778 /*738*/, 215850d /*38050d */, 0.05 /*0.01*/)]
        [InlineDataAttribute(1000d, 0d, 30d, 200d, new double[] { 150d }, new int[] { 10 }, 2, 100d /*300d*/, 0.05 /*0.15*/)]
        public void TestcalRequiredBarCoreMinMax(double stock, double saw, double minLeftover, double maxLeftover, double[] orderLens, int[] orderNums, int nRequiredBar, double expectedWastedLen, double lossRatio)
        {
            // Input
            double rawBarHeightInput = stock;
            double sawWidthInput = saw;
            var requiredBarSetInputs = new List<BarSet>();
            for (int i = 0; i < orderLens.Length; i++)
            {
                requiredBarSetInputs.Add(new BarSet(orderLens[i], orderNums[i]));
            }

            // Exercise
            var ret = CutOptimization.calRequiredBarCoreMinMax(rawBarHeightInput, sawWidthInput, requiredBarSetInputs, minLeftover, maxLeftover);

            // Assert
            int nTotalBar = UtilStatistics.sumInt(new List<int>(ret.Values));
            var wastedLen = UtilStatistics.calWastedLen(ret, stock, maxLeftover);

            Assert.Equal(nRequiredBar, nTotalBar);
            Assert.Equal(expectedWastedLen, wastedLen);
            Assert.InRange(wastedLen / (nTotalBar * stock), lossRatio - 0.01, lossRatio);
        }
    }
}
