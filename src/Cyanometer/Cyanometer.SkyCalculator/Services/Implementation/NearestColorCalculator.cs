using Cyanometer.Core.Core;
using Cyanometer.SkyCalculator.Services.Abstract;
using System;
using System.Collections.Generic;

namespace Cyanometer.SkyCalculator.Services.Implementation
{
    public class NearestColorCalculator: IColorCalculator
    {
        private IList<Color> colors;

        public void Init(IList<Color> colors)
        {
            this.colors = colors;
        }
        public Color CalculateNearest(Color source)
        {
            Color result = new Color();
            double current = double.MaxValue;
            foreach (var color in colors)
            {
                double diff = CalculateDistance(color, source);
                if (diff < current)
                {
                    result = color;
                    current = diff;
                }
            }
            return result;
        }

        /// <summary>
        /// http://stackoverflow.com/questions/7846286/c-sharp-finding-similar-colors
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        private double CalculateDistance(Color first, Color second)
        {
            double dbl_input_red = (double)second.R;
            double dbl_input_green = (double)second.G;
            double dbl_input_blue = (double)second.B;
            // compute the Euclidean distance between the two colors
            // note, that the alpha-component is not used in this example
            double dbl_test_red = Math.Pow(Convert.ToDouble((first).R) - dbl_input_red, 2.0);
            double dbl_test_green = Math.Pow(Convert.ToDouble
                ((first).G) - dbl_input_green, 2.0);
            double dbl_test_blue = Math.Pow(Convert.ToDouble
                ((first).B) - dbl_input_blue, 2.0);

            double temp = Math.Sqrt(dbl_test_blue + dbl_test_green + dbl_test_red);
            // explore the result and store the nearest color
            return temp;
        }
    }
}
