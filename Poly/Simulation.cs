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

            //occupancies = PopulateOccupanciesPlanar(randomVacancySelector, chainSize, numChains);
            occupancies = PopulateOccupanciesRandom(randomVacancySelector, chainSize, numChains);

            var currentChain = 0;
            var currentLink = 0;

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
                        //int currentBindings = 0;

                        //for (int linkIndex = 0; linkIndex < chainSize; linkIndex++)
                        //{
                        //    Vector linkCoords = occupancies[chosenVacancy + linkIndex];
                        //    VacancyType linkType = lattice[linkCoords.x, linkCoords.y, linkCoords.z];
                        //    foreach (var neighbourOrientation in latticeVectors)
                        //    {
                        //        //LatticeVector neighbourOrientation = (LatticeVector)neighbourIndex;
                        //        VacancyType neighbourType = lattice[
                        //            (linkCoords.x + Vector.XUnit(neighbourOrientation) + cellSize) % cellSize,
                        //            (linkCoords.y + Vector.YUnit(neighbourOrientation) + cellSize) % cellSize,
                        //            (linkCoords.z + Vector.ZUnit(neighbourOrientation) + cellSize) % cellSize];

                        //        currentBindings += (int)neighbourType * (int)linkType;
                        //    }
                        //}

                        //int newBindings = 0;

                        //for (int linkIndex = 0; linkIndex < chainSize; linkIndex++)
                        //{
                        //    Vector linkCoords = updatedPositions[linkIndex];
                        //    VacancyType linkType = lattice[linkCoords.x, linkCoords.y, linkCoords.z];
                        //    foreach (var neighbourOrientation in latticeVectors)
                        //    {
                        //        // LatticeVector neighbourOrientation = (LatticeVector)neighbourIndex;
                        //        VacancyType neighbourType = lattice[
                        //            (linkCoords.x + Vector.XUnit(neighbourOrientation) + cellSize) % cellSize,
                        //            (linkCoords.y + Vector.YUnit(neighbourOrientation) + cellSize) % cellSize,
                        //            (linkCoords.z + Vector.ZUnit(neighbourOrientation) + cellSize) % cellSize];

                        //        newBindings += (int)neighbourType * (int)linkType;
                        //    }
                        //}

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
                            numberOfAdjustments = 0;
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

        private Vector[] PopulateOccupanciesPlanar(Random randomVacancySelector, int chainSize, int numChains)
        {
            short xIndex = 0;
            short yIndex = 0;
            short zIndex = 0;

            occupancies = new Vector[numChains * chainSize];

            var currentChain = 0;
            var currentLink = 0;

            var chainsAlreadyPlaced = 0;
            var chainsPerRun = cellSize / chainSize;
            var chainsPerPlane = chainsPerRun * cellSize / 2;

            Console.WriteLine($"Chains per run: {chainsPerRun}");
            Console.WriteLine($"Chains per plane: {chainsPerPlane}");

            while (zIndex < cellSize)
            {
                // check whether we should fill this plane or not

                // calculated our cumulative fill factor

                // number of units actually placed
                var unitsPlaced = (chainsAlreadyPlaced * chainSize);

                // number of units expected to be placed so far
                var unitsExpectedPlaced = (fillFactor * (zIndex + 1) * cellSize * cellSize / 2);

                var cumulativeFillFactor = unitsExpectedPlaced > 0 ? unitsPlaced / unitsExpectedPlaced : 0;

                if (unitsPlaced < unitsExpectedPlaced)
                {
                    while (yIndex < cellSize)
                    {
                        while (xIndex < cellSize)
                        {
                            if ((xIndex + yIndex + zIndex) % 2 == 0)
                            {
                                // just keeping a tally won't work here because chains are zig-zag across y and y+1 so we'd have to step it back to wherever it was at x=0 for the previous y
                                // determining where we should be, must be algebraic
                                currentChain =
                                   xIndex / chainSize  // x
                                   + chainsPerRun * (yIndex / 2) // y
                                                                 //+ zIndex * chainsPerPlane; // z 
                                   + chainsAlreadyPlaced;

                                currentLink = xIndex % chainSize;

                                if (currentChain < numChains && (xIndex - (chainsPerRun * chainSize)) < 0)
                                {
                                    occupancies[currentChain * chainSize + currentLink] = new Vector(xIndex, yIndex, zIndex);
                                    var fillVacancy = VacancyTypeOfChain(currentChain);
                                    lattice[xIndex, yIndex, zIndex] = fillVacancy;
                                }

                            }
                            else
                            {
                                // this isn't a physical site in the face-centred cubic lattice
                            }
                            xIndex++;

                        }
                        xIndex = 0;

                        yIndex++;

                    }
                    chainsAlreadyPlaced += chainsPerPlane;
                }
                else
                {
                    // nothing needed for this plane
                }

                yIndex = 0;
                zIndex++;
            }
            zIndex = 0;

            return occupancies;
        }


        private Vector[] PopulateOccupanciesRandom(Random randomVacancySelector, int chainSize, int numChains)
        {
            occupancies = new Vector[numChains * chainSize];

            var tempPositions = new List<Vector>();

            List<LatticeVector> untriedNeighbourCoordinates = new List<LatticeVector>();

            int requiredRestarts = 0;

            for (int currentChain = 0; currentChain < numChains; currentChain++)
            {
                var chainType = VacancyTypeOfChain(currentChain);
                Console.Clear();
                Console.WriteLine($"Placing chain {currentChain} (required restarts: {requiredRestarts})");
                while (true) // might need to do this several times if the lattice is getting full
                {
                    // if we've come round this loop again we will have to unwind whatever the previous iteration did to the lattice
                    foreach (var unsatisfiedPosition in tempPositions)
                    {
                        lattice[unsatisfiedPosition.x, unsatisfiedPosition.y, unsatisfiedPosition.z] = VacancyType.Empty;

                    }
                    tempPositions.Clear();

                    // find an unoccupied starting point
                    var start = FindRandomUnoccupiedCell(randomVacancySelector, lattice);
                    tempPositions.Add(start);
                    if (PlaceChainRandom(chainSize - 1, tempPositions, randomVacancySelector, chainType)) break;
                    requiredRestarts++;
                }

                foreach (var vec in tempPositions)
                {
                    lattice[vec.x, vec.y, vec.z] = chainType;
                }
                Array.Copy(tempPositions.ToArray(), 0, occupancies, currentChain * chainSize, chainSize);
                requiredRestarts = 0;

            }

            return occupancies;
        }

        private VacancyType VacancyTypeOfChain(int currentChain)
        {
            return currentChain % 2 == 0 ? VacancyType.A : VacancyType.B;
        }

        private bool PlaceChainRandom(int remainingUnplaced, List<Vector> vectors, Random randomVacancySelector, VacancyType chainType)
        {

            if (remainingUnplaced == 0) { return true; }
            else
            {
                var prior = vectors.Last();
                List<LatticeVector> untriedNeighbourCoordinates = new List<LatticeVector>(latticeVectors);
                while (untriedNeighbourCoordinates.Count > 0)
                {
                    var tryDirection = untriedNeighbourCoordinates[randomVacancySelector.Next(0, untriedNeighbourCoordinates.Count)];
                    var tryThisOne = (prior + tryDirection).Wrap(cellSize);
                    if (lattice[tryThisOne.x, tryThisOne.y, tryThisOne.z] == VacancyType.Empty)
                    {
                        vectors.Add(tryThisOne);
                        lattice[tryThisOne.x, tryThisOne.y, tryThisOne.z] = chainType;
                        return PlaceChainRandom(remainingUnplaced - 1, vectors, randomVacancySelector, chainType);
                    }
                    else
                    {
                        untriedNeighbourCoordinates.Remove(tryDirection);
                    }
                }
                return false;
            }
        }

        private Vector FindRandomUnoccupiedCell(Random randomVacancySelector, VacancyType[,,] lattice)
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
                    if (lattice[x, y, z] == VacancyType.Empty)
                    {
                        return new Vector(x, y, z);
                    }
                }
            }
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
            var directionalBias = new Vector();

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

                var first = occupancies[chainIndex * chainSize];
                var last = occupancies[(chainIndex + 1) * chainSize - 1];
                directionalBias.x += wrapDistance(last.x, first.x);
                directionalBias.y += wrapDistance(last.y, first.y);
                directionalBias.z += wrapDistance(last.z, first.z);
            }

            directionalBias.x /= numChains;
            directionalBias.y /= numChains;
            directionalBias.z /= numChains;

            Console.WriteLine($"Directional bias: ({directionalBias.x},{directionalBias.y},{directionalBias.z})");
        }

        private int squaredDistance(int x1, int x2)
        {
            var sum = Math.Abs(x1 - x2);
            if (sum > cellSize / 2) sum = cellSize - sum;
            return sum * sum;
        }

        private int wrapDistance(int x1, int x2)
        {
            var sum = Math.Abs(x1 - x2);
            if (sum > cellSize / 2) sum = (cellSize - sum) * Math.Sign(x1 - x2);
            return sum;
        }
    }
}