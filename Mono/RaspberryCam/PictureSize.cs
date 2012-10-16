using System;

namespace RaspberryCam
{
    public struct PictureSize
    {
        public PictureSize(int width, int height) : this()
        {
            Width = width;
            Height = height;

            if (width <= 0)
                throw new ArgumentOutOfRangeException("width", "must be greater than 0");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height", "must be greater than 0");
        }

        public int Width { get; private set; }

        public int Height { get; private set; }
    }
}