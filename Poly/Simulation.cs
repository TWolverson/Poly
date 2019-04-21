using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poly
{
    public class Simulation
    {
        public Simulation(int cellSize, int chainSize, decimal fillFactor)
        {
            this.cellSize = cellSize;
            this.chainSize = chainSize;
            this.fillFactor = fillFactor;
        }

        decimal fillFactor = 0.5M;
        int chainSize = 12;
        int cellSize = 100;
        int numChains;
        Vector[] occupancies;

        VacancyType[,,] lattice;

        LatticeVector[] latticeVectors = (LatticeVector[])Enum.GetValues(typeof(LatticeVector));

        decimal[] acceptanceFactors = new decimal[12];

        public void Run()
        {
            var randomVacancySelector = new Random();

            var totalVacancies = cellSize * cellSize * cellSize / 2; // only half the number of cubic cells are occupiable
            numChains = (int)(totalVacancies * fillFactor / chainSize);
            numChains = numChains - numChains % 2; // must be even as we want equal distribution of As and Bs
            Console.WriteLine($"Fill factor: {fillFactor} Computed fill factor: {(double)numChains * chainSize / totalVacancies} Chain size: {chainSize} Computed number of chains: {numChains}");

            lattice = new VacancyType[cellSize, cellSize, cellSize];
            short xIndex = 0;
            short yIndex = 0;
            short zIndex = 0;

            var chainsPerRun = cellSize / chainSize;
            var chainsPerPlane = chainsPerRun * cellSize / 2;

            Console.WriteLine($"Chains per run: {chainsPerRun}");
            Console.WriteLine($"Chains per plane: {chainsPerPlane}");

            occupancies = PopulateOccupancies(randomVacancySelector, chainSize, numChains);

            var currentChain = 0;
            var currentLink = 0;

            while (zIndex < cellSize)
            {
                while (yIndex < cellSize)
                {
                    while (xIndex < cellSize)
                    {
                        if ((xIndex + yIndex + zIndex) % 2 == 0)
                        {
                            currentChain =
                               xIndex / chainSize  // x
                               + chainsPerRun * (yIndex / 2) // y
                               + zIndex * chainsPerPlane; // z 

                            currentLink = xIndex % chainSize;

                            if (currentChain < numChains && (xIndex - (chainsPerRun * chainSize)) < 0)
                            {
                                occupancies[currentChain * chainSize + currentLink] = new Vector(xIndex, yIndex, zIndex);
                                var fillVacancy = (VacancyType)(1 - ((currentChain % 2) == 0 ? 2 : 0));
                                lattice[xIndex, yIndex, zIndex] = fillVacancy;
                            }

                        }
                        else
                        {
                        }
                        xIndex++;

                    }
                    xIndex = 0;

                    yIndex++;

                }
                yIndex = 0;
                zIndex++;
            }
            zIndex = 0;

            var lookup = new HashSet<Vector>(occupancies);

            var totalEntropy = CountEntropy();

            bool stop = false;
            // var kT = 5000;

            var numberOfAdjustments = 0;
            Vector[] updatedPositions = new Vector[chainSize];
            while (!stop)
            {
                //UpdateAcceptanceFactors(5000);

                currentChain = randomVacancySelector.Next(0, numChains);

                // fudge this so that we always choose the head, for now
                // speeds up the algorithm as we are not generating 11/12 randomised points that don't have a movement algorithm
                var chosenVacancy = currentChain * chainSize;
                var chosenVacancyLocation = occupancies[chosenVacancy];

                var randomDirection = latticeVectors[randomVacancySelector.Next(12)];
                var randomlySelectedNeighbour = chosenVacancyLocation + randomDirection;
                randomlySelectedNeighbour.x = (randomlySelectedNeighbour.x + cellSize) % cellSize;
                randomlySelectedNeighbour.y = (randomlySelectedNeighbour.y + cellSize) % cellSize;
                randomlySelectedNeighbour.z = (randomlySelectedNeighbour.z + cellSize) % cellSize;

                if (lookup.Contains(randomlySelectedNeighbour))
                {
                    // the neighbour is occupied; compute a swap
                }
                else
                {
                    // the neighbour is unoccupied; determine whether we can move into this vacancy

                    // 1) if we are the head or the tail we can always move into this vacancy
                    if (chosenVacancy % chainSize == 0)
                    {
                        Array.Copy(occupancies, chosenVacancy, updatedPositions, 1, chainSize - 1);
                        updatedPositions[0] = randomlySelectedNeighbour;
                        lookup.Add(randomlySelectedNeighbour);

                        Vector vacatedPosition = occupancies[chosenVacancy + chainSize - 1]; // the position of the last element is vacated

                        // compare the neighbour interactions between the two configurations
                        int currentBindings = 0;

                        for (int linkIndex = 0; linkIndex < chainSize; linkIndex++)
                        {
                            Vector linkCoords = occupancies[chosenVacancy + linkIndex];
                            VacancyType linkType = lattice[linkCoords.x, linkCoords.y, linkCoords.z];
                            foreach (var neighbourOrientation in latticeVectors)
                            {
                                //LatticeVector neighbourOrientation = (LatticeVector)neighbourIndex;
                                VacancyType neighbourType = lattice[
                                    (linkCoords.x + Vector.XUnit(neighbourOrientation) + cellSize) % cellSize,
                                    (linkCoords.y + Vector.YUnit(neighbourOrientation) + cellSize) % cellSize,
                                    (linkCoords.z + Vector.ZUnit(neighbourOrientation) + cellSize) % cellSize];

                                currentBindings += (int)neighbourType * (int)linkType;
                            }
                        }

                        int newBindings = 0;

                        for (int linkIndex = 0; linkIndex < chainSize; linkIndex++)
                        {
                            Vector linkCoords = updatedPositions[linkIndex];
                            VacancyType linkType = lattice[linkCoords.x, linkCoords.y, linkCoords.z];
                            foreach (var neighbourOrientation in latticeVectors)
                            {
                                // LatticeVector neighbourOrientation = (LatticeVector)neighbourIndex;
                                VacancyType neighbourType = lattice[
                                    (linkCoords.x + Vector.XUnit(neighbourOrientation) + cellSize) % cellSize,
                                    (linkCoords.y + Vector.YUnit(neighbourOrientation) + cellSize) % cellSize,
                                    (linkCoords.z + Vector.ZUnit(neighbourOrientation) + cellSize) % cellSize];

                                newBindings += (int)neighbourType * (int)linkType;
                            }
                        }

                        Array.Copy(updatedPositions, 0, occupancies, chosenVacancy, chainSize);


                        var thisChainVacancyType = (VacancyType)(1 - ((currentChain % 2) == 0 ? 2 : 0));

                        numberOfAdjustments++;
                        foreach (var vector in updatedPositions)
                        {
                            var chainIndex = chosenVacancy / numChains;
                            lattice[vector.x, vector.y, vector.z] = thisChainVacancyType;
                        }

                        lattice[vacatedPosition.x, vacatedPosition.y, vacatedPosition.z] = VacancyType.Empty;
                        lookup.Remove(vacatedPosition);

                        if (numberOfAdjustments % 1000 == 0)
                        {
                            RenderLayerToConsole();
                            ValidateChainIntegrity();
                        }

                    }
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
                            if (thisVacancy == (VacancyType)0) { xIndex++; continue; }

                            // count the number of interaction contributions with this site's neighbours
                            // double-counting the same pair of sites is intentional
                            foreach (var vector in latticeVectors)
                            {
                                // A-A makes a positive contribution
                                // B-B makes a positive contribution
                                // B-A and A-B make a negative contribution
                                // A-0 and B-0 and 0-A and 0-B make zero contribution
                                totalEntropy += (int)lattice[
                                    (zIndex + Vector.ZUnit(vector) + cellSize) % cellSize,
                                    (yIndex + Vector.YUnit(vector) + cellSize) % cellSize,
                                    (xIndex + Vector.XUnit(vector) + cellSize) % cellSize
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

        private Vector[] PopulateOccupancies(Random randomVacancySelector, int chainSize, int numChains)
        {
            return new Vector[numChains * chainSize];
        }

        /// <summary>
        /// Print a slice of the cell to the console indicating the occupancies and the current entropy
        /// </summary>
        private void RenderLayerToConsole()
        {
            Console.Clear();

            var builder = new StringBuilder();

            short xIndex = 0;
            short yIndex = 0;
            short zIndex = 99;

            while (yIndex < cellSize)
            {
                while (xIndex < cellSize)
                {
                    if ((xIndex + yIndex + zIndex) % 2 == 0)
                    {
                        char type = '#';
                        switch (lattice[xIndex, yIndex, zIndex])
                        {
                            case VacancyType.A:
                                type = 'A'; break;
                            case VacancyType.B:
                                type = 'B'; break;
                            case VacancyType.Empty:
                                type = '#'; break;
                        }
                        builder.Append(type);
                    }
                    else
                    {
                        builder.Append(' ');
                    }
                    xIndex++;

                }
                xIndex = 0;

                yIndex++;
                builder.AppendLine();

            }

            Console.Write(builder.ToString());

            Console.WriteLine("Total Entropy: " + CountEntropy());

        }

        private void ValidateChainIntegrity()
        {
            for (int chainIndex = 0; chainIndex < numChains; chainIndex++)
            {
                for (int unitIndex = 0; unitIndex < chainSize - 1; unitIndex++) // we don't need to look up the last unit because its position relative to the previous unit has been validated
                {
                    Vector unitPosition = occupancies[chainIndex * chainSize + unitIndex];
                    Vector nextUnitPosition = occupancies[chainIndex * chainSize + unitIndex + 1];
                    //if ((unitPosition.x - ((nextUnitPosition.x - cellSize) % cellSize)) * (unitPosition.x - ((nextUnitPosition.x - cellSize) % cellSize))
                    //    + (unitPosition.y - ((nextUnitPosition.y - cellSize) % cellSize)) * (unitPosition.y - ((nextUnitPosition.y - cellSize) % cellSize))
                    //    + (unitPosition.z - ((nextUnitPosition.z - cellSize) % cellSize)) * (unitPosition.z - ((nextUnitPosition.z - cellSize) % cellSize)) != 2)

                    if (squaredDistance(unitPosition.x, nextUnitPosition.x) + squaredDistance(unitPosition.y, nextUnitPosition.y) + squaredDistance(unitPosition.z, nextUnitPosition.z) != 2)

                    { Debugger.Break(); }
                }
            }
        }

        private int squaredDistance(int x1, int x2)
        {
            var sum = Math.Abs(x1 - x2);
            if (sum > cellSize / 2) sum = cellSize - sum;
            return sum * sum;
        }
    }
}