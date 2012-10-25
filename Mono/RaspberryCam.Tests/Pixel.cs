using System.Drawing;

namespace RaspberryCam.Tests
{
    public class Pixel
    {
        private int x;
        private int y;
        private Color color;

        public Pixel(int x, int y, Color color)
        {
            this.x = x;
            this.y = y;
            this.color = color;
        }

        public int X
        {
            get { return x; }
        }

        public int Y
        {
            get { return y; }
        }

        public Color Color
        {
            get { return color; }
        }
    }
}