using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Poly
{
    public class SimulationProperties
    {
        public int CellSize { get; set; }

        public int ChainSize { get; set; }

        public decimal FillFactor { get; set; }

        public virtual string FormatAsFileName()
        {
            return $"{this.CellSize}_{this.ChainSize}_{this.FillFactor}";
        }

        public static SimulationProperties FromFileName(string fileName)
        {
            var properties = new SimulationProperties();
            var parameters = fileName.Split('_');
            properties.CellSize = int.Parse(parameters[2]);
            properties.ChainSize = int.Parse(parameters[3]);
            properties.FillFactor = decimal.Parse(parameters[4]);
            return properties;
        }
    }

    public partial class Simulation
    {
        public Vector[] PopulateOccupanciesFromFile(string fileName)
        {
            var parameters = fileName.Split('_');
            cellSize = int.Parse(parameters[2]);
            chainSize = int.Parse(parameters[3]);
            fillFactor = decimal.Parse(parameters[4]);

            var totalVacancies = cellSize * cellSize * cellSize / 2; // only half the number of cubic cells are occupiable
            numChains = (int)(totalVacancies * fillFactor / chainSize);
            numChains = numChains - numChains % 2;

            var content = File.ReadAllText(fileName);
            content = content.Substring(1, content.Length - 1);
            var vectors = content.Split(new string[] { ")", "(" }, StringSplitOptions.RemoveEmptyEntries);

            occupancies = new Vector[numChains * chainSize];
            lattice = new OccupationType[cellSize, cellSize, cellSize];

            int vectorIndex = 0;
            foreach (var vectorString in vectors)
            {
                var split = vectorString.Split(',');
                var vector = new Vector(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]));
                occupancies[vectorIndex] = vector;
                lattice[vector.x, vector.y, vector.z] = OccupancyTypeOfChain(vectorIndex / numChains);
                vectorIndex++;
            }

            return occupancies;

        }

        public Vector[] PopulateOccupanciesFromBinaryFile(string fileName)
        {
            var parameters = fileName.Split('_');
            cellSize = int.Parse(parameters[2]);
            chainSize = int.Parse(parameters[3]);
            fillFactor = decimal.Parse(parameters[4]);

            var totalVacancies = cellSize * cellSize * cellSize / 2; // only half the number of cubic cells are occupiable
            numChains = (int)(totalVacancies * fillFactor / chainSize);
            numChains = numChains - numChains % 2;

            occupancies = new Vector[numChains * chainSize];
            lattice = new OccupationType[cellSize, cellSize, cellSize];

            var bytebuffer = new byte[12];
            int vectorIndex = 0;
            using (var stream = File.OpenRead(fileName))
            {
                while (vectorIndex < numChains * chainSize)
                {
                    stream.Read(bytebuffer, 0, 12);
                    var vector = new Vector(BitConverter.ToInt32(bytebuffer, 0), BitConverter.ToInt32(bytebuffer, 4), BitConverter.ToInt32(bytebuffer, 8));
                    occupancies[vectorIndex] = vector;
                    lattice[vector.x, vector.y, vector.z] = OccupancyTypeOfChain(vectorIndex / numChains);
                    vectorIndex++;
                }
            }

            return occupancies;

        }

        public void WriteOccupanciesToFile()
        {
            var ticks = DateTime.Now.Ticks.ToString();
            using (var file = File.OpenWrite($"occupancies_{ticks}_{cellSize}_{chainSize}_{fillFactor}"))
            {
                using (var writer = new StreamWriter(file))
                {
                    foreach (var vector in occupancies)
                    {
                        writer.Write($"({vector.x},{vector.y},{vector.z})");
                    }
                }
            }

            using (var file = File.OpenWrite($"lattice_{ticks}_{cellSize}_{chainSize}_{fillFactor}"))
            {
                using (var writer = new StreamWriter(file))
                {
                    foreach (var occupancy in lattice)
                    {
                        writer.Write((int)occupancy);
                        writer.Write(",");
                    }
                }
            }
        }


        public void WriteOccupanciesToBinaryFile()
        {
            var ticks = DateTime.Now.Ticks.ToString();
            using (var file = File.OpenWrite($"occupanciesbinary_{ticks}_{cellSize}_{chainSize}_{fillFactor}"))
            {
                foreach (var occ in occupancies)
                {
                    file.Write(BitConverter.GetBytes(occ.x), 0, 4);
                    file.Write(BitConverter.GetBytes(occ.y), 0, 4);
                    file.Write(BitConverter.GetBytes(occ.z), 0, 4);
                }
            }
        }
    }
}

