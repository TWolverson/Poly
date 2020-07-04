using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poly
{
    public partial class Simulation
    {
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
                                    var fillVacancy = OccupancyTypeOfChain(currentChain);
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
    }
}
