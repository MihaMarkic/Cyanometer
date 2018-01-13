using Cyanometer.Core.Core;
using Cyanometer.SkyCalculator.Services.Abstract;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cyanometer.SkyCalculator.Services.Implementation
{
    public class Calculator : ISkyCalculator
    {
        public const int DefaultIndexOrientation = 25;
        private readonly IColorCalculator colorCalculator;
        public static int[] ColorValues = 
            new int[] {
                0xF4FBFE,
                0xF5FAFC,
                0xEDF8FE,
                0xE6F5FD,
                0xDEF3FD,
                0xD7F0FC,
                0xCFEBFB,
                0xC8E5F8,
                0xC1DFF4,
                0xBADAEE,
                0xB3D5E9,
                0xACD0E4,
                0xA5CBE0,
                0x9FC6DB,
                0x98C2D7,
                0x92BDD3,
                0x86B8CF,
                0x7EB2CC,
                0x78AED2,
                0x6FA9C7,
                0x63A4C4,
                0x5B9EBE,
                0x5399BD,
                0x4694BA,
                0x3B8EB7,
                0x388BB3,
                0x3586AE,
                0x2E81A9,
                0x297DA6,
                0x2677A1,
                0x24739C,
                0x1E6E96,
                0x1C6991,
                0x10648E,
                0x0C5F8A,
                0x065B86,
                0x005682,
                0x00537B,
                0x004F75,
                0x064C6E,
                0x0A4768,
                0x0E4361,
                0x113E57,
                0x143A52,
                0x14354B,
                0x173245,
                0x182F40,
                0x192C3C,
                0x192941,
                0x192633,
                0x18232E,
                0x171F29,
                0x101822
            };
        public Calculator(IColorCalculator colorCalculator)
        {
            this.colorCalculator = colorCalculator;
        }
        public GetBluenessIndexResult GetBluenessIndexTopPixels(IList<Color> image, int pixelsCount)
        {
            return GetBluenessIndexTopPixels(image, pixelsCount, DefaultIndexOrientation);
        }
        public GetBluenessIndexResult GetBluenessIndexTopPixels(IList<Color> image, int pixelsCount, int orientation)
        {
            Color[] colors = ColorValues.Select(c => Color.FromHex(c)).ToArray();
            BlockingCollection<int> result = new BlockingCollection<int>();
            colorCalculator.Init(colors);

            Parallel.ForEach(Partitioner.Create(0, image.Count),
                (range, loopState) =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        Color nearest = colorCalculator.CalculateNearest(image[i]);
                        result.Add(Array.IndexOf(colors, nearest));
                    }
                });

            var pruned = result.Where(r => r <= 35 && r >= 11).ToArray();
            double index = pruned.Length > 0 ? pruned.Sum() / pruned.Length: 0;

            return new GetBluenessIndexResult
            {
                Index = Convert.ToInt32(Math.Round(index, MidpointRounding.AwayFromZero)),
                Indexes = result.ToArray()
            };
        }
    }
}
