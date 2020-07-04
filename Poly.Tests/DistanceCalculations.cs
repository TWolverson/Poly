using NuGet.Frameworks;
using System;
using Xunit;

namespace Poly.Tests
{
    public class DistanceCalculations
    {
        private readonly Simulation simulation = new Simulation(100, 8, 0.5M);

        [Fact]
        public void Test1()
        {
            var distance = simulation.wrapDistance(99, 1);
            Assert.Equal(2, distance);
        }

        [Fact]
        public void Test2()
        {
            var distance = simulation.wrapDistance(99, 2);
            Assert.Equal(3, distance);
        }

        [Fact]
        public void Test3()
        {
            var distance = simulation.wrapDistance(98, 99);
            Assert.Equal(1, distance);
        }

        [Fact]
        public void Test4()
        {
            var distance = simulation.wrapDistance(1, 99);
            Assert.Equal(-2, distance);
        }

        [Fact]
        public void Test5()
        {
            var distance = simulation.wrapDistance(49, 51);
            Assert.Equal(2, distance);
        }


        [Fact]
        public void Test6()
        {
            var distance = simulation.wrapDistance(51, 49);
            Assert.Equal(-2, distance);
        }
    }
}
