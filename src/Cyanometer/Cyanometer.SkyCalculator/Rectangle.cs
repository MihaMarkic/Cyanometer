
namespace Cynanometer.Calculator.Engine
{

    public struct Rectangle
    {
        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Right
        {
            get { return X + Width; }
        }
        public int Bottom
        {
            get { return Y + Height; }
        }
    }
}
