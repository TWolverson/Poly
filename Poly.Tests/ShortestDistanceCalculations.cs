using NuGet.Frameworks;
using System;
using Xunit;

namespace Poly.Tests
{
    public class ShortestDistanceCalculations
    {
        private readonly Simulation simulation = new Simulation(100, 8, 0.5M);

        [Fact]
        public void From_99_To_1()
        {
            var distance = simulation.wrapDistance(99, 1);
            Assert.Equal(2, distance);
        }

        [Fact]
        public void From_99_To_2()
        {
            var distance = simulation.wrapDistance(99, 2);
            Assert.Equal(3, distance);
        }

        [Fact]
        public void From_98_To_99()
        {
            var distance = simulation.wrapDistance(98, 99);
            Assert.Equal(1, distance);
        }

        [Fact]
        public void From_1_To_99()
        {
            var distance = simulation.wrapDistance(1, 99);
            Assert.Equal(-2, distance);
        }

        [Fact]
        public void From_49_To_51()
        {
            var distance = simulation.wrapDistance(49, 51);
            Assert.Equal(2, distance);
        }


        [Fact]
        public void From_51_To_49()
        {
            var distance = simulation.wrapDistance(51, 49);
            Assert.Equal(-2, distance);
        }
    }
}
