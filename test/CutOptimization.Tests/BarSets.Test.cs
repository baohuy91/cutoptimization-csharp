using System.Collections.Generic;
using Xunit;

namespace CutOptimization.Tests
{
    public class BarSetsTests
    {

        [TheoryAttribute]
        [InlineDataAttribute(new double[] { 5d }, new int[] { 2 }, new double[] { 5d, 5d })]
        [InlineDataAttribute(new double[] { 5d, 6d }, new int[] { 2, 1 }, new double[] { 5d, 5d, 6d })]
        [InlineDataAttribute(new double[] { }, new int[] { }, new double[] { })]
        public void TestClone(double[] lens, int[] nums, double[] expectedPopLen)
        {
            // Prepare
            var sut = initBarSets(lens, nums);
            var originalCount = sut.count();

            // Exercise
            var ret = sut.clone();

            // Assert
            foreach (var expectedLen in expectedPopLen)
            {
                Assert.Equal(expectedLen, ret.popLen());
            }
            // after pop, original BarSets is not affected
            Assert.Equal(originalCount, sut.count());
        }

        [TheoryAttribute]
        [InlineDataAttribute(new double[] { 5d }, new int[] { 2 }, new double[] { 5d }, new int[] { 1 },
        new double[] { 5d, 5d, 5d })]
        [InlineDataAttribute(new double[] { 5d, 6d }, new int[] { 2, 1 }, new double[] { 6d, 7d }, new int[] { 1, 2 },
        new double[] { 5d, 5d, 6d, 6d, 7d, 7d })]
        [InlineDataAttribute(new double[] { }, new int[] { }, new double[] { 5d }, new int[] { 2 },
        new double[] { 5d, 5d })]
        public void TestAddAll(double[] len1s, int[] num1s, double[] len2s, int[] num2s, double[] expectedPopLens)
        {
            // Prepare
            var sut = initBarSets(len1s, num1s);
            var barSets2 = initBarSets(len2s, num2s);

            // Exercise
            sut.addAll(barSets2);

            // Assert
            sut.sortAsc();
            foreach (var expectedLen in expectedPopLens)
            {
                Assert.Equal(expectedLen, sut.popLen());
            }
        }

        [TheoryAttribute]
        [InlineDataAttribute(new double[] { 5d }, new int[] { 2 }, 6d, new double[] { 5d, 5d, 6d })]
        [InlineDataAttribute(new double[] { 5d, 6d }, new int[] { 2, 1 }, 7d, new double[] { 5d, 5d, 6d, 7d })]
        [InlineDataAttribute(new double[] { }, new int[] { }, 1d, new double[] { 1d })]
        public void TestAddLen(double[] lens, int[] nums, double newLen, double[] expectedPopLen)
        {
            // Prepare
            var sut = initBarSets(lens, nums);
            var originalCount = sut.count();

            // Exercise
            sut.addLen(newLen);

            // Assert
            Assert.Equal(originalCount + 1, sut.count());
            foreach (var expectedLen in expectedPopLen)
            {
                Assert.Equal(expectedLen, sut.popLen());
            }
        }

        [TheoryAttribute]
        [InlineDataAttribute(new double[] { 5d }, new int[] { 2 }, new double[] { 5d })]
        [InlineDataAttribute(new double[] { 5d, 6d }, new int[] { 2, 1 }, new double[] { 5d, 5d, 6d })]
        [InlineDataAttribute(new double[] { }, new int[] { }, new double[] { })]
        public void TestSortAsc(double[] lens, int[] nums, double[] expectedPopLen)
        {
            // Prepare
            var sut = initBarSets(lens, nums);

            // Exercise
            sut.sortAsc();

            // Assert
            foreach (var expectedLen in expectedPopLen)
            {
                Assert.Equal(expectedLen, sut.popLen());
            }
        }

        [TheoryAttribute]
        [InlineDataAttribute(new double[] { 5d }, new int[] { 2 }, 5d, 1)]
        [InlineDataAttribute(new double[] { 5d, 6d }, new int[] { 2, 1 }, 5d, 2)]
        [InlineDataAttribute(new double[] { }, new int[] { }, 0d, 0)]
        public void TestPopLen(double[] lens, int[] nums, double expectedLen, int expectedCount)
        {
            var sut = initBarSets(lens, nums);

            var ret = sut.popLen();

            Assert.Equal(expectedLen, ret);
            Assert.Equal(expectedCount, sut.count());
        }

        [TheoryAttribute]
        [InlineDataAttribute(new double[] { 5d }, new int[] { 2 }, 10d)]
        [InlineDataAttribute(new double[] { 5d, 6d }, new int[] { 2, 1 }, 16d)]
        [InlineDataAttribute(new double[] { }, new int[] { }, 0d)]
        public void TestCountTotalLen(double[] lens, int[] nums, double expectedLen)
        {
            var sut = initBarSets(lens, nums);

            var ret = sut.countTotalLen();

            Assert.Equal(expectedLen, ret);
        }

        [TheoryAttribute]
        [InlineDataAttribute(new double[] { 5d }, new int[] { 2 }, 2)]
        [InlineDataAttribute(new double[] { 5d, 6d }, new int[] { 2, 1 }, 3)]
        [InlineDataAttribute(new double[] { }, new int[] { }, 0)]
        public void TestCount(double[] lens, int[] nums, int expectedCount)
        {
            var sut = initBarSets(lens, nums);

            var ret = sut.count();

            Assert.Equal(expectedCount, ret);
        }

        [TheoryAttribute]
        [InlineDataAttribute(new double[] { 5d }, new int[] { 1 }, false)]
        [InlineDataAttribute(new double[] { }, new int[] { }, true)]
        public void TestIsEmpty(double[] lens, int[] nums, bool expectedIsEmpty)
        {
            var sut = initBarSets(lens, nums);

            var ret = sut.isEmpty();

            Assert.Equal(expectedIsEmpty, ret);
        }

        private static BarSets initBarSets(double[] lens, int[] nums)
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