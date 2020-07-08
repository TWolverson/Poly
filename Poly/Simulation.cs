using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poly
{
    public partial class Simulation
    {
        public Simulation(int cellSize, int chainSize, decimal fillFactor)
        {
            this.cellSize = cellSize;
            this.chainSize = chainSize;
            this.fillFactor = fillFactor;
        }

        private decimal fillFactor;

        private int chainSize;

        private int cellSize;

        int numChains;

        Vector[] occupancies;

        OccupationType[,,] lattice;

        private readonly LatticeVector[] latticeVectors = (LatticeVector[])Enum.GetValues(typeof(LatticeVector));

        decimal[] acceptanceFactors = new decimal[12];

        private readonly Random randomVacancySelector = new Random();



        private void Crank()
        {

        }

        public void Run()
        {
            //var totalVacancies = cellSize * cellSize * cellSize / 2; // only half the number of cubic cells are occupiable
            //numChains = (int)(totalVacancies * fillFactor / chainSize);
            //numChains = numChains - numChains % 2; // must be even as we want equal distribution of As and Bs
            //Console.WriteLine($"Fill factor: {fillFactor} Computed fill factor: {(double)numChains * chainSize / totalVacancies} Chain size: {chainSize} Computed number of chains: {numChains}");

            //lattice = new OccupationType[cellSize, cellSize, cellSize];

            //occupancies = PopulateOccupanciesPlanar(randomVacancySelector, chainSize, numChains);
            //occupancies = PopulateOccupanciesRandom(randomVacancySelector, chainSize, numChains);
            occupancies = PopulateOccupanciesFromBinaryFile("occupanciesbinary_637297463816335541_100_48_0.8");

            RenderLayerToConsole();

            ValidateChainIntegrity();

            var totalEntropy = CountEntropy();

            bool stop = false;
            // var kT = 5000;

            var numberOfAdjustments = 0;
            Vector[] updatedPositions = new Vector[chainSize];
            while (!stop)
            {
                //UpdateAcceptanceFactors(5000);

                var randomSite = randomVacancySelector.Next(0, numChains * chainSize);

                //switch (randomSite % chainSize)
                //{
                //    case 0:
                //        this is the start of a chain, i.e.the index is a whole number of chain sizes
                //        if (TrySnake(randomSite)) numberOfAdjustments++;
                //        break;
                //    case (chainSize - 1):
                //        this is the end of a chain, i.e.the index is displaced once from a whole number of chain sizes
                //        if (TrySnake(randomSite)) numberOfAdjustments++;
                //        break;
                //    default:
                //        the index is in the middle of a chain
                //        Crank();
                //        break;

                //}

                if (randomSite % chainSize == 0 | randomSite % chainSize == (chainSize - 1))
                {
                    if (TrySnake(randomSite)) numberOfAdjustments++;
                }

                if ((numberOfAdjustments + 1) % 1000000 == 0)
                {
                    numberOfAdjustments = 0;
                    RenderLayerToConsole();
                    ValidateChainIntegrity();

                    //WriteOccupanciesToFile();
                    //WriteOccupanciesToBinaryFile();
                }
                //kT /= 2;
                //stop = kT <= 1;
            }

            Console.WriteLine($"Total entropy {totalEntropy}");

            Console.ReadLine();
        }

        private void UpdateAcceptanceFactors(decimal kT)
        {
            for (int i = 1; i < 12; i++)
            {
                acceptanceFactors[i - 1] = (decimal)Math.Exp(-i / (double)kT);
            }
        }

        private int CountEntropy()
        {
            short xIndex = 0;
            short yIndex = 0;
            short zIndex = 0;

            var totalEntropy = 0;

            while (zIndex < 100)
            {
                while (yIndex < 100)
                {
                    while (xIndex < 100)
                    {
                        if ((xIndex + yIndex + zIndex) % 2 == 0)
                        {
                            var thisVacancy = lattice[xIndex, yIndex, zIndex];
                            if (thisVacancy == (OccupationType)0) { xIndex++; continue; }

                            // count the number of interaction contributions with this site's neighbours
                            // double-counting the same pair of sites is intentional
                            foreach (var vector in latticeVectors)
                            {
                                // A-A makes a positive contribution
                                // B-B makes a positive contribution
                                // B-A and A-B make a negative contribution
                                // A-0 and B-0 and 0-A and 0-B make zero contribution
                                totalEntropy += (int)lattice[
                                    (xIndex + Vector.XUnit(vector) + cellSize) % cellSize,
                                    (yIndex + Vector.YUnit(vector) + cellSize) % cellSize,
                                    (zIndex + Vector.ZUnit(vector) + cellSize) % cellSize
                                    ]
                                * (int)thisVacancy;
                            }
                        }
                        xIndex++;
                    }
                    xIndex = 0;
                    yIndex++;
                }
                yIndex = 0;
                zIndex++;
            }

            return totalEntropy;
        }



        private OccupationType OccupancyTypeOfChain(int currentChain)
        {
            return currentChain % 2 == 0 ? OccupationType.A : OccupationType.B;
        }

        private Vector FindRandomUnoccupiedCell(Random randomVacancySelector, OccupationType[,,] lattice)
        {
            int x, y, z;
            while (true)
            {
                x = randomVacancySelector.Next(0, cellSize);
                y = randomVacancySelector.Next(0, cellSize);
                z = randomVacancySelector.Next(0, cellSize);
                if ((x + y + z) % 2 == 0)
                {
                    // the site is physical, check its occupancy
                    if (lattice[x, y, z] == OccupationType.Empty)
                    {
                        return new Vector(x, y, z);
                    }
                }
            }
        }

        private void ValidateChainIntegrity()
        {
            var directionalBias = new Vector();

            for (int chainIndex = 0; chainIndex < numChains; chainIndex++)
            {
                var startIndex = chainIndex * chainSize;
                var endIndex = startIndex + chainSize - 1; // don't consider the last element in the chain because the last-but-one already looks one ahead

                for (int unitIndex = startIndex; unitIndex < endIndex; unitIndex++) // we don't need to look up the last unit because its position relative to the previous unit has been validated
                {
                    Vector unitPosition = occupancies[unitIndex];
                    Vector nextUnitPosition = occupancies[unitIndex + 1];

                    if (squaredDistance(unitPosition.x, nextUnitPosition.x) + squaredDistance(unitPosition.y, nextUnitPosition.y) + squaredDistance(unitPosition.z, nextUnitPosition.z) != 2)

                    { Debugger.Break(); }
                }

                var first = occupancies[startIndex];
                var last = occupancies[endIndex];
                directionalBias.x += wrapDistance(last.x, first.x);
                directionalBias.y += wrapDistance(last.y, first.y);
                directionalBias.z += wrapDistance(last.z, first.z);
            }

            Console.WriteLine($"Directional bias: ({directionalBias.x / (decimal)numChains},{directionalBias.y / (decimal)numChains},{directionalBias.z / (decimal)numChains})");
        }

        public int squaredDistance(int x1, int x2)
        {
            var sum = Math.Abs(x1 - x2);
            if (sum > cellSize / 2) sum = cellSize - sum;
            return sum * sum;
        }

        public int wrapDistance(int x1, int x2)
        {
            var dx = x2 - x1;
            if (dx > cellSize * 0.5) dx = dx - cellSize;
            if (dx <= -cellSize * 0.5) dx = dx + cellSize;
            return dx;
        }
    }
}