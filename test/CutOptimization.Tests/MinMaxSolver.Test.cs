using System;
using System.Collections.Generic;
using Xunit;

namespace CutOptimization
{
    public class MinMaxSolverTests
    {
        [TheoryAttribute]
        [InlineDataAttribute(1000d, 30d, 200d, new double[] { 150d }, new int[] { 10 }, 2, 100d, 0.05)] // 0.15
        [InlineDataAttribute(1000d, 30d, 200d, new double[] { 330d }, new int[] { 3 }, 1, 10d, 0.01)]
        [InlineDataAttribute(1000d, 30d, 200d, new double[] { 150d }, new int[] { 10 }, 2, 100d, 0.05)]
        [InlineDataAttribute(300d, 0d, 0d, new double[] { 250d, 350d }, new int[] { 2, 1 }, 2, 100d, 0.17)]
        [InlineDataAttribute(300d, 0d, 0d, new double[] { 250d, 350d, 200d }, new int[] { 2, 1, 1 }, 3, 200d, 0.23)]
        public void TestSolve(double stock, double minLeftover, double maxLeftover, double[] orderLens, int[] orderNums, int nRequiredBar, double expectedWastedLen, double lossRatio)
        {
            var orderSets = new List<BarSet>();
            for (var i = 0; i < orderLens.Length; i++)
            {
                orderSets.Add(new BarSet(orderLens[i], orderNums[i]));
            }

            // Exercise
            var sut = new MinMaxSolver(minLeftover, maxLeftover);
            var ret = sut.solve(orderSets, stock);

            // Assert
            int nTotalBar = UtilStatistics.sumInt(new List<int>(ret.Values));
            var wastedLen = UtilStatistics.calWastedLen(ret, stock, maxLeftover);
            Assert.Equal(nRequiredBar, nTotalBar);
            Assert.Equal(expectedWastedLen, wastedLen);
            Assert.InRange(wastedLen / (nTotalBar * stock), lossRatio - 0.01, lossRatio);
        }

        [TheoryAttribute]
        [InlineDataAttribute(1000d, 30d, 200d, new double[] { 900d }, new int[] { 10 })] 
        public void TestSolve_WithCantCutCodition(double stock, double minLeftover, double maxLeftover, double[] orderLens, int[] orderNums)
        {
            var orderSets = new List<BarSet>();
            for (var i = 0; i < orderLens.Length; i++)
            {
                orderSets.Add(new BarSet(orderLens[i], orderNums[i]));
            }

            // Exercise
            var sut = new MinMaxSolver(minLeftover, maxLeftover);
            var ret = sut.solve(orderSets, stock);

            Assert.Equal(0, ret.Count);
        }

        [TheoryAttribute]
        [InlineDataAttribute(1000d, 30d, 200d, new double[] { 300d, 200d }, new int[] { 3, 3 }, new double[] { 200d, 200d, 300d, 300d })]
        [InlineDataAttribute(1000d, 30d, 200d, new double[] { 300d, 200d }, new int[] { 1, 3 }, new double[] { 200d, 200d, 300d })]
        [InlineDataAttribute(1000d, 200d, 300d, new double[] { 300d, 200d }, new int[] { 3, 1 }, new double[] { 300d, 300d, 300d })]
        [InlineDataAttribute(500d, 30d, 180d, new double[] { 300d }, new int[] { 2 }, new double[] { 300d })]
        public void TestOptimizeToOneStock(double stock, double minLeftover, double maxLeftover, double[] orderLens, int[] orderNums, double[] expectedNewPttrnPopLens)
        {
            BarSets orders = initPattern(orderLens, orderNums);
            MinMaxSolver sut = new MinMaxSolver(minLeftover, maxLeftover);
            BarSets pattern = sut.optimizeToOneStock(stock, orders);
            pattern.sortAsc();

            Assert.Equal(expectedNewPttrnPopLens.Length, pattern.count());
            foreach (double expectedPopLen in expectedNewPttrnPopLens)
            {
                Assert.Equal(expectedPopLen, pattern.popLen());
            }
        }

        [TheoryAttribute]
        [InlineDataAttribute(300d, 30d, 200d, new double[] { 200d }, new int[] { 3 })]
        [InlineDataAttribute(500d, 30d, 400d, new double[] { 300d }, new int[] { 2 })]
        public void TestOptimizeToOneStock_WithCantCutCondition_ExpectNull(double stock, double minLeftover, double maxLeftover, double[] orderLens, int[] orderNums)
        {
            BarSets orders = initPattern(orderLens, orderNums);
            MinMaxSolver sut = new MinMaxSolver(minLeftover, maxLeftover);
            BarSets ret = sut.optimizeToOneStock(stock, orders);

            Assert.Null(ret);
        }

        [Fact]
        public void TestAddPatternToDictionary(){
            var dic = new Dictionary<BarSets, int>();
            BarSets bs1 = initPattern(new double[]{35d}, new int[]{1});
            BarSets addedBs = initPattern(new double[]{30d}, new int[]{1});
            dic.Add(bs1, 2);

            MinMaxSolver.addPatternToDictionary(dic, addedBs);
            
            Assert.False(bs1.compareEqualWith(addedBs));
            Assert.Equal(2, dic.Count);
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