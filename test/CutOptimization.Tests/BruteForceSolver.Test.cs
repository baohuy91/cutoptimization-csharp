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
            int rawBarHeightInput = 1000;
            List<BarSet> requiredBarSetInputs = new List<BarSet>(){
                new BarSet(600d, 4),
                new BarSet(200d, 5)};

            var pttnRst = BruteForceSolver.solve(requiredBarSetInputs, rawBarHeightInput);
            var nBar = pttnRst.fst;
            
            Assert.True(nBar == 4);
        }
    }
}
