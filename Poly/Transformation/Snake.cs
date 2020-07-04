using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poly
{
    public partial class Simulation
    {
        private bool TrySnake(int chosenVacancy)
        {

            var currentChain = chosenVacancy / chainSize;

            var chosenVacancyLocation = occupancies[chosenVacancy];

            var randomDirection = latticeVectors[randomVacancySelector.Next(12)];
            var randomlySelectedNeighbour = (chosenVacancyLocation + randomDirection).Wrap(cellSize);

            if (lattice[randomlySelectedNeighbour.x, randomlySelectedNeighbour.y, randomlySelectedNeighbour.z] != OccupationType.Empty)
            {
                // the neighbour is occupied; compute a swap
            }
            else
            {
                // the neighbour is unoccupied; determine whether we can move into this vacancy
                Vector[] updatedPositions = new Vector[chainSize];

                // 1) if we are the head or the tail we can always move into this vacancy
                if (chosenVacancy % chainSize == 0)
                {
                    Array.Copy(occupancies, chosenVacancy, updatedPositions, 1, chainSize - 1);

                    updatedPositions[0] = randomlySelectedNeighbour;

                    int vacatedIndex = chosenVacancy + chainSize - 1;

                    Vector vacatedPosition = occupancies[vacatedIndex]; // the position of the last element is vacated

                    Array.Copy(updatedPositions, 0, occupancies, chosenVacancy, chainSize);

                    var thisChainOccupancyType = OccupancyTypeOfChain(currentChain);

                    foreach (var vector in updatedPositions)
                    {
                        lattice[vector.x, vector.y, vector.z] = thisChainOccupancyType;
                    }

                    lattice[vacatedPosition.x, vacatedPosition.y, vacatedPosition.z] = OccupationType.Empty;

                    return true;
                }
            }

            return false;
        }
    }
}
