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

    class Program
    {
        static decimal fillFactor = 0.75M;
        static int chainSize = 12;
        static int cellSize = 100;

        static void Main(string[] args)
        {
            //Parallel.For(10, 20, (x) => { new Simulation(cellSize, x, fillFactor).Run(); });
            new Simulation(100, 12, 0.5M).Run();
        }
    }
}
