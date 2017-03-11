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

            var pttnRst = BruteForceSolver.solveRecursive(requiredBarSetInputs, rawBarHeightInput, false, 0d, 0d);
            var nBar = pttnRst.fst;

            Assert.Equal(4, nBar);
        }

        [FactAttribute]
        public void TestApplyMinMaxFilter_WithOneValidPattern_ExpectReturnOneOnly()
        {
            // Input
            var possiblePttrns = new List<List<BarSet>>();
            // invalid
            var pttrn1 = new List<BarSet>();
            pttrn1.Add(new BarSet(300d, 3));
            // valid
            var pttrn2 = new List<BarSet>();
            pttrn2.Add(new BarSet(300d, 2));
            pttrn2.Add(new BarSet(200d, 1));
            possiblePttrns.Add(pttrn1);
            possiblePttrns.Add(pttrn2);

            // Exercise
            var ret = BruteForceSolver.applyMinMaxFilter(possiblePttrns, 1000d, 30d, 200d);

            // Assert
            Assert.Equal(1, ret.Count);
            Assert.Equal(2, ret[0].Count);
            Assert.Equal(300d, ret[0][0].len);
            Assert.Equal(2, ret[0][0].num);
            Assert.Equal(200d, ret[0][1].len);
            Assert.Equal(1, ret[0][1].num);
        }
    }
}
