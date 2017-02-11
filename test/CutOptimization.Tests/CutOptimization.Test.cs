using System;
using System.Collections.Generic;
using Xunit;
using CutOptimization;

namespace Tests
{
    public class Tests
    {
        [Fact]
        public void Test1()
        {
            // Input
            double rawBarHeightInput = 1000d;
            double sawWidthInput = 0d;
            List<BarSet> requiredBarSetInputs = new List<BarSet>(){
                new BarSet(600d, 4),
                new BarSet(200d, 5)};

            var nBar = CutOptimization.CutOptimization.calRequiredBar(rawBarHeightInput, sawWidthInput, requiredBarSetInputs);
            
            Assert.True(nBar == 4);
        }
    }
}
