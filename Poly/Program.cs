using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        static void Main(string[] args)
        {
            var latticeVectors = (LatticeVector[])Enum.GetValues(typeof(LatticeVector));

            //foreach (var vacancy in latticeVectors)
            //{
            //    var x = XUnit(vacancy);
            //    var y = YUnit(vacancy);
            //    var z = ZUnit(vacancy);
            //    Console.WriteLine($"({x},{y},{z})");
            //}

            var randomVacancySelector = new Random();

            var lattice = new VacancyType[100, 100, 100];
            var xIndex = 0;
            var yIndex = 0;
            var zIndex = 0;
            while (zIndex < 100)
            {
                while (yIndex < 100)
                {
                    while (xIndex < 100)
                    {
                        if ((xIndex + yIndex + zIndex) % 2 == 0)
                        {
                            var fillVacancy = (VacancyType)randomVacancySelector.Next(-1, 2);
                            // if ((xIndex + yIndex + zIndex) % 500 == 0) { Console.WriteLine($"Setting {xIndex},{yIndex},{zIndex} to type {fillVacancy}"); }
                            lattice[xIndex, yIndex, zIndex] = fillVacancy;
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
                            foreach (var vector in latticeVectors)
                            {
                                totalEntropy += (int)lattice[(zIndex + ZUnit(vector) + 100) % 100, (yIndex + YUnit(vector) + 100) % 100, (xIndex + XUnit(vector) + 100) % 100] * (int)thisVacancy;
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

            var numChains = 1000;
            var chainSize = 12;
            var occupancies = new long[numChains * chainSize];

            bool stop = false;
            var kT = 5000;
            var acceptanceFactors = new decimal[12];
            while (!stop)
            {
                // precompute boltzmann factors
                for (int i = 1; i < 12; i++)
                {
                    acceptanceFactors[i - 1] = (decimal)Math.Exp((double)-i / kT);
                }

                // perform annealing
                var chosenVacancy = randomVacancySelector.Next(0, numChains * chainSize);
                var chosenVacancyLocation = new int[]
                {
                    (int)(occupancies[chosenVacancy] >> 0) ^ 0b10000,
                    (int)(occupancies[chosenVacancy] >> 16) ^ 0b10000,
                    (int)(occupancies[chosenVacancy] >> 32) ^ 0b10000
                };

                kT /= 2;
                stop = kT <= 1;
            }

            Console.WriteLine($"Total entropy {totalEntropy}");

            Console.Read();
        }
    }
}
