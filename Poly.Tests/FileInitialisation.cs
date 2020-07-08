using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Poly.Tests
{
    public class FileInitialisation
    {
        private readonly Simulation simulation = new Simulation(100, 8, 0.5M);

        [Fact]
        public void Loads_from_binary_file()
        {
            var occupancies = simulation.PopulateOccupanciesFromBinaryFile(@"Data\occupanciesbinary_637297463816335541_100_48_0.8");
            Assert.Equal(399936, occupancies.Length);
        }

        [Fact]
        public void Read_file_properties()
        {
            var properties = SimulationProperties.FromFileName("occupanciesbinary_637297463816335541_100_48_0.8");
            Assert.Equal(100, properties.CellSize);
            Assert.Equal(48, properties.ChainSize);
            Assert.Equal(0.8M, properties.FillFactor);
        }

        [Fact]
        public void Create_filename_from_file_properties() {
            var properties = new SimulationProperties
            {
                CellSize = 100,
                ChainSize = 48,
                FillFactor = 0.8M
            };
            var fileName = properties.FormatAsFileName();
            Assert.Equal("100_48_0.8", fileName);        
        }
    }
}
