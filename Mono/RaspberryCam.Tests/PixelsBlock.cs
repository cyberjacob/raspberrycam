using System.Collections.Generic;

namespace RaspberryCam.Tests
{
    public class PixelsBlock
    {
        private List<Pixel> pixels;

        public PixelsBlock(List<Pixel> pixels)
        {
            this.pixels = pixels;
        }

        public List<Pixel> Pixels
        {
            get { return pixels; }
        }
    }
}