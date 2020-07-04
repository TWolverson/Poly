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
                            case OccupationType.A:
                                type = 'A'; break;
                            case OccupationType.B:
                                type = 'B'; break;
                            case OccupationType.Empty:
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

    }
}
