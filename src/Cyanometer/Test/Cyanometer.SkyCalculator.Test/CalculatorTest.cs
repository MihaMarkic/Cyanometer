using Cyanometer.Core.Core;
using Cyanometer.SkyCalculator.Services.Implementation;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace Cyanometer.SkyCalculator.Test
{
    public class CalculatorTest
    {
        protected Calculator calculator;

        [SetUp]
        public void Setup()
        {
            calculator = new Calculator(new NearestColorCalculator());
        }

        [TestFixture]
        public class GetBluenessIndexTopPixels : CalculatorTest
        {
            // TODO needs update for prunning
            //[TestCase("m0", ExpectedResult = 0)]
            //[TestCase("m1", ExpectedResult = 0)]
            //[TestCase("m2", ExpectedResult = 2)]
            //[TestCase("m3", ExpectedResult = 3)]
            //[TestCase("m4", ExpectedResult = 4)]
            //[TestCase("m5", ExpectedResult = 5)]
            //[TestCase("m6", ExpectedResult = 6)]
            //[TestCase("m7", ExpectedResult = 7)]
            //[TestCase("m8", ExpectedResult = 8)]
            //[TestCase("m9", ExpectedResult = 9)]
            //[TestCase("m10", ExpectedResult = 10)]
            //[TestCase("m11", ExpectedResult = 11)]
            //[TestCase("m12", ExpectedResult = 12)]
            //[TestCase("m13", ExpectedResult = 13)]
            //[TestCase("m14", ExpectedResult = 15)]
            //[TestCase("m15", ExpectedResult = 16)]
            //[TestCase("m16", ExpectedResult = 17)]
            //[TestCase("m17", ExpectedResult = 17)]
            //[TestCase("m18", ExpectedResult = 19)]
            //[TestCase("m19", ExpectedResult = 20)]
            //[TestCase("m20", ExpectedResult = 21)]
            //[TestCase("m21", ExpectedResult = 22)]
            //[TestCase("m22", ExpectedResult = 23)]
            //[TestCase("m23", ExpectedResult = 24)]
            //[TestCase("m24", ExpectedResult = 25)]
            //[TestCase("m25", ExpectedResult = 26)]
            //[TestCase("m26", ExpectedResult = 27)]
            //[TestCase("m27", ExpectedResult = 28)]
            //[TestCase("m28", ExpectedResult = 28)]
            //[TestCase("m29", ExpectedResult = 29)]
            //[TestCase("m30", ExpectedResult = 30)]
            //[TestCase("m31", ExpectedResult = 31)]
            //[TestCase("m32", ExpectedResult = 32)]
            //[TestCase("m33", ExpectedResult = 33)]
            //[TestCase("m34", ExpectedResult = 33)]
            //[TestCase("m35", ExpectedResult = 34)]
            //[TestCase("m36", ExpectedResult = 35)]
            //[TestCase("m37", ExpectedResult = 37)]
            //[TestCase("m38", ExpectedResult = 39)]
            //[TestCase("m39", ExpectedResult = 40)]
            //[TestCase("m40", ExpectedResult = 40)]
            //[TestCase("m41", ExpectedResult = 41)]
            //[TestCase("m42", ExpectedResult = 42)]
            //[TestCase("m43", ExpectedResult = 43)]
            //[TestCase("m44", ExpectedResult = 44)]
            //[TestCase("m45", ExpectedResult = 45)]
            //[TestCase("m46", ExpectedResult = 46)]
            //[TestCase("m47", ExpectedResult = 47)]
            //[TestCase("m48", ExpectedResult = 49)]
            //[TestCase("m49", ExpectedResult = 49)]
            //[TestCase("m50", ExpectedResult = 50)]
            //[TestCase("m51", ExpectedResult = 51)]
            //[TestCase("m52", ExpectedResult = 52)]

            //public int TestFromImage(string name)
            //{
            //    string path = Path.Combine(TestContext.CurrentContext.TestDirectory, $"Images\\{name}.png");
            //    var colors = GetColors(path);
            //    return calculator.GetBluenessIndexTopPixels(colors, pixelsCount: 30).Index;
            //}

            [Test]
            public void TestImage()
            {
                string path = Path.Combine(TestContext.CurrentContext.TestDirectory, $"Images\\sky-17.06.2016-09_31_13-small.jpg");
                var colors = GetColors(path);
                var index = calculator.GetBluenessIndexTopPixels(colors, pixelsCount: 30, orientation: 25).Index;
            }
        }

        protected List<Color> GetColors(string filename)
        {
            List<Color> colors = new List<Color>();

            using (var bitmap = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile(filename))
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        var pixel = bitmap.GetPixel(x, y);
                        colors.Add(Color.FromRgb(pixel.R, pixel.G, pixel.B));
                    }
                }
            }
            return colors;
        }
    }
}
