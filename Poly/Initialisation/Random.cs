using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poly
{
    public partial class Simulation
    {
        private Vector[] PopulateOccupanciesRandom(Random randomVacancySelector, int chainSize, int numChains)
        {
            occupancies = new Vector[numChains * chainSize];

            var tempPositions = new List<Vector>();

            List<LatticeVector> untriedNeighbourCoordinates = new List<LatticeVector>();

            int requiredRestarts = 0;

            for (int currentChain = 0; currentChain < numChains; currentChain++)
            {
                var chainType = OccupancyTypeOfChain(currentChain);
                Console.Clear();
                Console.WriteLine($"Placing chain {currentChain} (required restarts: {requiredRestarts})");
                while (true) // might need to do this several times if the lattice is getting full
                {
                    // if we've come round this loop again we will have to unwind whatever the previous iteration did to the lattice
                    foreach (var unsatisfiedPosition in tempPositions)
                    {
                        lattice[unsatisfiedPosition.x, unsatisfiedPosition.y, unsatisfiedPosition.z] = OccupationType.Empty;

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

        private bool PlaceChainRandom(int remainingUnplaced, List<Vector> vectors, Random randomVacancySelector, OccupationType chainType)
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
                    if (lattice[tryThisOne.x, tryThisOne.y, tryThisOne.z] == OccupationType.Empty)
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
    }
}
