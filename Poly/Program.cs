using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Poly
{
    enum X
    {
        Left = 0b00,
        Zero = 0b01,
        Right = 0b10
    }

    enum Y
    {
        Back = 0b0000,
        Zero = 0b0100,
        Fore = 0b1000
    }

    enum Z
    {
        Down = 0b000000,
        Zero = 0b010000,
        Up = 0b100000
    }

    enum LatticeVector
    {
        // lower plane x,y,-1
        LeftZeroDown = X.Left | Y.Zero | Z.Down,
        ZeroBackDown = X.Zero | Y.Back | Z.Down,
        RightZeroDown = X.Right | Y.Zero | Z.Down,
        ZeroForeDown = X.Zero | Y.Fore | Z.Down,
        // middle plane x,y,0
        LeftBackZero = X.Left | Y.Back | Z.Zero,
        LeftForeZero = X.Left | Y.Fore | Z.Zero,
        RightForeZero = X.Right | Y.Fore | Z.Zero,
        RightBackZero = X.Right | Y.Back | Z.Zero,
        // upper plane x,y,1
        LeftZeroUp = X.Left | Y.Zero | Z.Up,
        ZeroBackUp = X.Zero | Y.Back | Z.Up,
        RightZeroUp = X.Right | Y.Zero | Z.Up,
        ZeroForeUp = X.Zero | Y.Fore | Z.Up,
    }

    enum VacancyType
    {
        A = -1,
        Empty = 0,
        B = 1
    }



    class Program
    {
        struct Vector
        {
            public Vector(int x, int y, int z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }
            internal int x; internal int y; internal int z;

            public static Vector operator +(Vector v, LatticeVector delta)
            {
                return new Vector(v.x + XUnit(delta), v.y + YUnit(delta), v.z + ZUnit(delta));
            }
        }

        static int XUnit(LatticeVector vector)
        {
            return (((int)vector >> 0) & 0b11) - 1;
        }

        static int YUnit(LatticeVector vector)
        {
            return (((int)vector >> 2) & 0b11) - 1;
        }

        static int ZUnit(LatticeVector vector)
        {
            return (((int)vector >> 4) & 0b11) - 1;
        }

        static decimal fillFactor = 0.5M;
        static int chainSize = 12;
        static int cellSize = 100;
        static VacancyType[,,] lattice;

        static LatticeVector[] latticeVectors = (LatticeVector[])Enum.GetValues(typeof(LatticeVector));

        static void Main(string[] args)
        {


            var randomVacancySelector = new Random();

            var totalVacancies = cellSize * cellSize * cellSize / 2; // only half the number of cubic cells are occupiable
            var numChains = (int)(totalVacancies * fillFactor / chainSize);
            numChains = numChains - numChains % 2; // must be even as we want equal distribution of As and Bs
            Console.WriteLine($"Fill factor: {fillFactor} Computed fill factor: {(double)numChains * chainSize / totalVacancies} Chain size: {chainSize} Computed number of chains: {numChains}");

            lattice = new VacancyType[100, 100, 100];
            short xIndex = 0;
            short yIndex = 0;
            short zIndex = 0;

            var chainsPerRun = cellSize / chainSize;
            var chainsPerPlane = chainsPerRun * cellSize / 2;

            Console.WriteLine($"Chains per run: {chainsPerRun}");
            Console.WriteLine($"Chains per plane: {chainsPerPlane}");
            // Console.Read();
            Vector[] occupancies = PopulateOccupancies(randomVacancySelector, chainSize, numChains);

            var currentChain = 0;
            var currentLink = 0;

            while (zIndex < cellSize)
            {
                while (yIndex < cellSize)
                {
                    //Console.Write($"{yIndex})");
                    while (xIndex < cellSize)
                    {
                        if ((xIndex + yIndex + zIndex) % 2 == 0)
                        {


                            currentChain =
                               xIndex / chainSize  // x
                               + chainsPerRun * (yIndex / 2)
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
            var kT = 5000;
            var acceptanceFactors = new decimal[12];
            var numberOfAdjustments = 0;
            Vector[] updatedPositions = new Vector[chainSize];
            while (!stop)
            {
                // precompute boltzmann factors
                //for (int i = 1; i < 12; i++)
                //{
                //    acceptanceFactors[i - 1] = (decimal)Math.Exp((double)-i / kT);
                //}

                // perform annealing
                currentChain = randomVacancySelector.Next(0, numChains);

                // fudge this so that we always choose the head, for now
                var chosenVacancy = currentChain * chainSize; //randomVacancySelector.Next(0, numChains * chainSize);
                var chosenVacancyLocation = occupancies[chosenVacancy];

                var randomDirection = (LatticeVector)randomVacancySelector.Next(12);
                var randomlySelectedNeighbour = chosenVacancyLocation + randomDirection;
                randomlySelectedNeighbour.x = (randomlySelectedNeighbour.x + cellSize) % cellSize;
                randomlySelectedNeighbour.y = (randomlySelectedNeighbour.y + cellSize) % cellSize;
                randomlySelectedNeighbour.z = (randomlySelectedNeighbour.z + cellSize) % cellSize;

                //currentChain = chosenVacancy / chainSize;

                if (lookup.Contains(randomlySelectedNeighbour))
                {
                    // the neighbour is occupied; compute a swap
                }
                else
                {
                    //RenderLayerToConsole();
                    // the neighbour is unoccupied; determine whether we can move into this vacancy

                    // 1) if we are the head or the tail we can always move into this vacancy
                    if (chosenVacancy % chainSize == 0)
                    {

                        Array.Copy(occupancies, chosenVacancy, updatedPositions, 1, chainSize - 1);
                        updatedPositions[0] = randomlySelectedNeighbour;
                        lookup.Add(randomlySelectedNeighbour);

                        // does not work if the vacancy chosen happens to be the very last one
                        Vector vacatedPosition = occupancies[chosenVacancy];

                        // compare the neighbour interactions between the two configurations
                        int currentBindings = 0;

                        for (int linkIndex = 0; linkIndex < chainSize; linkIndex++)
                        {
                            Vector linkCoords = occupancies[chosenVacancy + linkIndex];
                            VacancyType linkType = lattice[linkCoords.x, linkCoords.y, linkCoords.z];
                            for (int neighbourIndex = 0; neighbourIndex < 12; neighbourIndex++)
                            {
                                LatticeVector neighbourOrientation = (LatticeVector)neighbourIndex;
                                VacancyType neighbourType = lattice[
                                    (linkCoords.x + XUnit(neighbourOrientation) + cellSize) % cellSize,
                                    (linkCoords.y + YUnit(neighbourOrientation) + cellSize) % cellSize,
                                    (linkCoords.z + ZUnit(neighbourOrientation) + cellSize) % cellSize];

                                currentBindings += (int)neighbourType * (int)linkType;
                            }
                        }

                        int newBindings = 0;

                        for (int linkIndex = 0; linkIndex < chainSize; linkIndex++)
                        {
                            Vector linkCoords = updatedPositions[linkIndex];
                            VacancyType linkType = lattice[linkCoords.x, linkCoords.y, linkCoords.z];
                            for (int neighbourIndex = 0; neighbourIndex < 12; neighbourIndex++)
                            {
                                LatticeVector neighbourOrientation = (LatticeVector)neighbourIndex;
                                VacancyType neighbourType = lattice[
                                    (linkCoords.x + XUnit(neighbourOrientation) + cellSize) % cellSize,
                                    (linkCoords.y + YUnit(neighbourOrientation) + cellSize) % cellSize,
                                    (linkCoords.z + ZUnit(neighbourOrientation) + cellSize) % cellSize];

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

                        if (numberOfAdjustments % 1000 == 0) RenderLayerToConsole();

                    }
                }


                //kT /= 2;
                //stop = kT <= 1;
            }

            Console.WriteLine($"Total entropy {totalEntropy}");

            Console.ReadLine();
        }

        private static int CountEntropy()
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
                                totalEntropy += (int)lattice[(zIndex + ZUnit(vector) + cellSize) % cellSize, (yIndex + YUnit(vector) + cellSize) % cellSize, (xIndex + XUnit(vector) + cellSize) % cellSize] * (int)thisVacancy;
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

        private static Vector[] PopulateOccupancies(Random randomVacancySelector, int chainSize, int numChains)
        {
            // fill with some fake occupancies for now
            // these are unphysical as they can be anywhere and are not adjacent

            //var numChains = 1000;
            //var chainSize = 12;
            //return Enumerable.Repeat(0, numChains * chainSize - 1).Select(_ =>
            //    new Vector((short)randomVacancySelector.Next(0, 100), (short)randomVacancySelector.Next(0, 100), (short)randomVacancySelector.Next(0, 100)))
            //    .ToArray();

            return new Vector[numChains * chainSize];
        }


        static void RenderLayerToConsole()
        {
            Console.Clear();

            var builder = new StringBuilder();

            short xIndex = 0;
            short yIndex = 0;
            short zIndex = 99;

            //while (zIndex < cellSize)
            //{
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
                        //Console.Write(type);
                    }
                    else
                    {
                        builder.Append(' ');
                        //Console.Write(' ');
                    }
                    xIndex++;

                }
                xIndex = 0;

                yIndex++;
                //Console.WriteLine();
                builder.AppendLine();

            }

            Console.Write(builder.ToString());
            //    yIndex = 0;
            //    zIndex++;
            //Console.Read();
            //}
            //zIndex = 0;

            Console.WriteLine("Total Entropy: " + CountEntropy());

        }

    }
}
