using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poly
{
    public partial class Simulation
    {
        /// <summary>
        /// Print a slice of the cell to the console indicating the occupancies and the current entropy
        /// </summary>
        private void RenderLayerToConsole()
        {
            Console.Clear();

            // var builder = new StringBuilder();

            short xIndex = 0;
            short yIndex = 0;
            short zIndex = 99;

            var colors = new ConsoleColor[100, 100];


            var idsInPlane = new List<int>();
            for (int occIndex = 0; occIndex < chainSize * numChains; occIndex++)
            {
                int chainId = occIndex / chainSize;
                var currentOccupancy = occupancies[occIndex];
                if (currentOccupancy.z == 99)
                {
                    if (!idsInPlane.Contains(chainId))
                    {
                        idsInPlane.Add(chainId);
                    }
                    var order = idsInPlane.IndexOf(chainId);
                    if (order < 15)
                    {
                        colors[currentOccupancy.x, currentOccupancy.y] = (ConsoleColor)order;
                    }
                    else
                    {
                        colors[currentOccupancy.x, currentOccupancy.y] = ConsoleColor.White;
                    }
                }
            }

            while (yIndex < cellSize)
            {
                while (xIndex < cellSize)
                {
                    if ((xIndex + yIndex + zIndex) % 2 == 0)
                    {
                        char type = '#';
                        switch (lattice[xIndex, yIndex, zIndex])
                        {
                            case OccupationType.A:
                                type = 'A'; break;
                            case OccupationType.B:
                                type = 'B'; break;
                            case OccupationType.Empty:
                                type = '#'; break;
                        }
                        //builder.Append(type);

                        Console.ForegroundColor = colors[xIndex, yIndex];
                        Console.Write(type);
                        Console.ResetColor();
                    }
                    else
                    {
                        //builder.Append(' ');
                        Console.Write(' ');
                    }
                    xIndex++;

                }
                xIndex = 0;

                yIndex++;
                // builder.AppendLine();
                Console.WriteLine();

            }



            //Console.Write(builder.ToString());

            Console.WriteLine("Total Entropy: " + CountEntropy());

        }
    }
}
