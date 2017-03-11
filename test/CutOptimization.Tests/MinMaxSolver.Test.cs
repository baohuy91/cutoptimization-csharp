using System.Collections.Generic;
using Xunit;

namespace CutOptimization
{
    public class MinMaxSolverTests
    {
        [TheoryAttribute]
        [InlineDataAttribute(1000d, 30d, 200d, new double[] { 150d }, new int[] { 10 }, 3, 900d, 0.30)] // 0.15
        [InlineDataAttribute(1000d, 30d, 200d, new double[] { 330d }, new int[] { 3 }, 1, 10d, 0.01)]
        [InlineDataAttribute(1000d, 30d, 200d, new double[] { 150d }, new int[] { 10 }, 2, 300d, 0.15)]
        public void TestSolve(double stock, double minLeftover, double maxLeftover, double[] orderLens, int[] orderNums, int nRequiredBar, double expectedWastedLen, double lossRatio)
        {
            var orderSets = new List<BarSet>();
            for (var i = 0; i < orderLens.Length; i++)
            {
                orderSets.Add(new BarSet(orderLens[i], orderNums[i]));
            }

            // Exercise
            var ret = MinMaxSolver.solve(orderSets, stock, minLeftover, maxLeftover);

            // Assert
            int nTotalBar = UtilStatistics.sumInt(new List<int>(ret.Values));
            var wastedLen = UtilStatistics.calWastedLen(ret, stock, maxLeftover);
            Assert.Equal(nRequiredBar, nTotalBar);
            Assert.Equal(expectedWastedLen, wastedLen);
            Assert.InRange(wastedLen / (nTotalBar * stock), lossRatio - 0.01, lossRatio);
        }

        [TheoryAttribute]
        [InlineDataAttribute(1000d, 30d, 200d, new double[] { 300d, 200d }, new int[] { 1, 3 }, new double[] { 200d, 200d, 300d }, new double[] { 200d })]
        [InlineDataAttribute(1000d, 30d, 200d, new double[] { 300d }, new int[] { 3 }, new double[] { 300d, 300d }, new double[] { 300d })]
        [InlineDataAttribute(1000d, 150d, 200d, new double[] { 300d }, new int[] { 3 }, new double[] { 300d, 300d, 300d }, new double[] { })]
        [InlineDataAttribute(1000d, 30d, 100d, new double[] { 300d }, new int[] { 3 }, new double[] { 300d, 300d, 300d }, new double[] { })]
        public void TestRemoveInvalidBarFromPattern(double stock, double minLeftover, double maxLeftover, double[] orderLens, int[] orderNums, double[] expectedNewPttrnPopLens, double[] expectedLeftPopLens)
        {
            var pttrn = initPattern(orderLens, orderNums);

            // Exercise
            var ret = MinMaxSolver.removeInvalidBarFromPattern(pttrn, stock, minLeftover, maxLeftover);

            // Assert
            var newPttrnRet = ret.fst;
            Assert.Equal(expectedNewPttrnPopLens.Length, newPttrnRet.count());
            foreach (var expectedLen in expectedNewPttrnPopLens)
            {
                Assert.Equal(expectedLen, newPttrnRet.popLen());
            }

            var leftBarSetRet = ret.snd;
            for (int i = 0; i < expectedLeftPopLens.Length; i++)
            {
                Assert.Equal(expectedLeftPopLens[i], leftBarSetRet.popLen());
            }
        }

        [Fact]
        public void TestRemoveInvalidBarFromAllPatterns()
        {
            // Prepare data
            var pttrn1 = initPattern(new double[] { 300d }, new int[] { 3 });
            var pttrn2 = initPattern(new double[] { 300d, 200d }, new int[] { 1, 3 });
            var pttrnMap = new Dictionary<BarSets, int>();
            pttrnMap.Add(pttrn1, 1);
            pttrnMap.Add(pttrn2, 2);

            // Exercise
            var ret = MinMaxSolver.removeInvalidBarFromAllPatterns(pttrnMap, 1000d, 30d, 200d);

            // Assert
            var newPttrnMapRet = ret.fst;
            Assert.Equal(2, newPttrnMapRet.Count);
            // check total len of filtered patterns
            var totalLen = 0d;
            foreach (var item in newPttrnMapRet)
            {
                totalLen += item.Key.countTotalLen() * item.Value;
            }
            Assert.Equal(
                300d * 2 * 1
                + (300d * 1 + 200d * 2) * 2, totalLen);

            var leftBarSets = ret.snd;
            leftBarSets.sortAsc();
            var expectedPopLens = new double[] { 200d, 200d, 300d };
            foreach (double expectedPopLen in expectedPopLens)
            {
                Assert.Equal(expectedPopLen, leftBarSets.popLen());
            }
        }

        [Fact]
        public void TestRemoveInvalidBarFromAllPatterns_WithOptimalInput_ExpectSameValue()
        {
            // Prepare data
            var pttrn1 = initPattern(new double[] { 150d }, new int[] { 5 });
            var pttrnMap = new Dictionary<BarSets, int>();
            pttrnMap.Add(pttrn1, 2);

            // Exercise
            var ret = MinMaxSolver.removeInvalidBarFromAllPatterns(pttrnMap, 1000d, 30d, 200d);

            // Assert
            var newPttrnMapRet = ret.fst;
            // check total len of filtered patterns
            Assert.Equal(1, newPttrnMapRet.Count);
            Assert.True(newPttrnMapRet.ContainsValue(2));
            var totalLen = 0d;
            foreach (var item in newPttrnMapRet)
            {
                totalLen += item.Key.countTotalLen() * item.Value;
            }
            Assert.Equal(150d * 10, totalLen);

            var leftBarSets = ret.snd;
            Assert.Equal(0, leftBarSets.count());
        }

        private static BarSets initPattern(double[] lens, int[] nums)
        {
            var bs = new List<BarSet>();
            for (var i = 0; i < lens.Length; i++)
            {
                bs.Add(new BarSet(lens[i], nums[i]));
            }

            return new BarSets(bs);
        }
    }
}