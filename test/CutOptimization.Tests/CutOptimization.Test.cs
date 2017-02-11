using System.Collections.Generic;
using Xunit;

namespace CutOptimization.Tests
{
    public class CutOptimizationTests
    {
        [Fact]
        public void calRequiredBar_WithTensOrder_ExpectOptimal()
        {
            // Input
            double rawBarHeightInput = 1000d;
            double sawWidthInput = 1.5d;
            List<BarSet> requiredBarSetInputs = new List<BarSet>(){
                    new BarSet(600d, 50),
                    new BarSet(200d, 50),
                    new BarSet(100d, 10)};

            int nBar = CutOptimization.calRequiredBar(rawBarHeightInput, sawWidthInput, requiredBarSetInputs);

            Assert.Equal(50, nBar);
        }

        [Fact]
        public void TestcalRequiredBar_WithSmallOrder2_ExpectOptimal()
        {
            // Input
            double rawBarHeightInput = 1000d;
            double sawWidthInput = 0d;
            List<BarSet> requiredBarSetInputs = new List<BarSet>(){
                new BarSet(600d, 4),
                new BarSet(200d, 5)};

            var nBar = CutOptimization.calRequiredBar(rawBarHeightInput, sawWidthInput, requiredBarSetInputs);

            Assert.Equal(4, nBar);
        }
    }
}
