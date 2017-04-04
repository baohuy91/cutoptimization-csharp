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
            var expectedTotalBar = 0;
            for (int i = 0; i < orderLens.Length; i++)
            {
                requiredBarSetInputs.Add(new BarSet(orderLens[i], orderNums[i]));
                expectedTotalBar += orderNums[i];
            }

            // Exercise
            var ret = CutOptimization.calRequiredBarCore(rawBarHeightInput, sawWidthInput, requiredBarSetInputs);

            // Assert
            int nTotalBar = UtilStatistics.sumInt(new List<int>(ret.Values));
            var wastedLen = UtilStatistics.calWastedLen(ret, stock, 200d);
            var totalBar = UtilStatistics.calTotalBar(ret);
            Assert.Equal(expectedTotalBar, totalBar);
            Assert.Equal(nRequiredBar, nTotalBar);
            Assert.Equal(expectedWastedLen, wastedLen);
            Assert.InRange(wastedLen / (nTotalBar * stock), lossRatio - 0.01, lossRatio);
        }

        [TheoryAttribute]
        [InlineDataAttribute(1000d, 1.5d, 30d, 200d, new double[] { 600d, 200d, 100d }, new int[] { 50, 50, 10 }, 68 /*50*/, 13400 /*9000d*/, 0.20 /*0.18*/)]
        [InlineDataAttribute(1000d, 0d, 30d, 200d, new double[] { 600d, 200d }, new int[] { 4, 5 }, 4, 400d, 0.1)]
        [InlineDataAttribute(1000d, 0d, 30d, 200d, new double[] { 600d, 200d }, new int[] { 2, 4 }, 2, 0d, 0.00)]
        [InlineDataAttribute(1100d, 0d, 30d, 200d, new double[] { 500d, 200d }, new int[] { 4, 12 }, 4, 0d, 0.00)]
        [InlineDataAttribute(6000d, 0d, 30d, 200d, new double[] { 2158d, 1656d, 1458d, 734d, 646d, 546d }, new int[] { 1065, 83, 565, 565, 556, 556 }, 758 /*738*/, 131650d /*38050d */, 0.03 /*0.01*/)]
        [InlineDataAttribute(1000d, 0d, 30d, 200d, new double[] { 150d }, new int[] { 10 }, 2, 100d /*300d*/, 0.05 /*0.15*/)]
        [InlineDataAttribute(5800d, 1.5d, 270d, 280d, new double[] { 300.00, 150.00, 245.00, 246.00, 256.00, 250.00, 255.00, 400.00, 390.00, 265.00, 373.00, 370.00, 380.00, 352.00}, new int[] { 198, 12, 1, 7, 6, 3, 13, 16, 53, 44, 11, 53, 94, 43 }, 32, 3253d, 0.02)]
        public void TestcalRequiredBarCoreMinMax(double stock, double saw, double minLeftover, double maxLeftover, double[] orderLens, int[] orderNums, int nRequiredBar, double expectedWastedLen, double lossRatio)
        {
            // Input
            double rawBarHeightInput = stock;
            double sawWidthInput = saw;
            var requiredBarSetInputs = new List<BarSet>();
            var expectedTotalBar = 0;
            for (int i = 0; i < orderLens.Length; i++)
            {
                requiredBarSetInputs.Add(new BarSet(orderLens[i], orderNums[i]));
                expectedTotalBar += orderNums[i];
            }

            // Exercise
            var ret = CutOptimization.calRequiredBarCoreMinMax(rawBarHeightInput, sawWidthInput, requiredBarSetInputs, minLeftover, maxLeftover);

            // Assert
            int nTotalStock = UtilStatistics.sumInt(new List<int>(ret.Values));
            var wastedLen = UtilStatistics.calWastedLen(ret, stock, maxLeftover);
            var totalBar = UtilStatistics.calTotalBar(ret);
            Assert.Equal(expectedTotalBar, totalBar);
            Assert.Equal(nRequiredBar, nTotalStock);
            Assert.Equal(expectedWastedLen, wastedLen);
            Assert.InRange(wastedLen / (nTotalStock * stock), lossRatio - 0.01, lossRatio);
            // assert within range
            foreach (KeyValuePair<List<BarSet>, int> entry in ret)
            {
                List<BarSet> barSets = entry.Key;
                double leftover = stock - new BarSets(barSets).countTotalLen();
                Assert.NotInRange(leftover, minLeftover + 0.001, maxLeftover - 0.001);
            }
        }

        [TheoryAttribute]
        [InlineDataAttribute(1000d, 0d, 30d, 200d, new double[] { 900d }, new int[] { 10 })]
        public void TestcalRequiredBarCoreMinMax_WithCantCutCondition_ExpectEmptyResult(double stock, double saw, double minLeftover, double maxLeftover, double[] orderLens, int[] orderNums)
        {
            // Input
            double rawBarHeightInput = stock;
            double sawWidthInput = saw;
            var requiredBarSetInputs = new List<BarSet>();
            var expectedTotalBar = 0;
            for (int i = 0; i < orderLens.Length; i++)
            {
                requiredBarSetInputs.Add(new BarSet(orderLens[i], orderNums[i]));
                expectedTotalBar += orderNums[i];
            }

            // Exercise
            var ret = CutOptimization.calRequiredBarCoreMinMax(rawBarHeightInput, sawWidthInput, requiredBarSetInputs, minLeftover, maxLeftover);

            Assert.Equal(0, ret.Count);
        }

        [TheoryAttribute]
        [InlineDataAttribute(300d, 0d, 0d, 0d, new double[] { 250d, 350d }, new int[] { 2, 2 }, 2)]
        [InlineDataAttribute(300d, 0d, 0d, 0d, new double[] { 250d, 350d, 200d }, new int[] { 2, 2, 1 }, 3)]
        public void TestcalRequiredBarCoreMinMax_WithCantCutAll(double stock, double saw, double minLeftover, double maxLeftover, double[] orderLens, int[] orderNums, int nRequiredBar)
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
            int nTotalStock = UtilStatistics.sumInt(new List<int>(ret.Values));
            Assert.Equal(nRequiredBar, nTotalStock);
            // assert within range
            foreach (KeyValuePair<List<BarSet>, int> entry in ret)
            {
                List<BarSet> barSets = entry.Key;
                double cutLen = new BarSets(barSets).countTotalLen();
                Assert.InRange(cutLen, 0, stock);
            }
        }
    }
}
