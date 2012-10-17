using System;

namespace RaspberryCam
{
    public struct PictureSize
    {
        public PictureSize(int width, int height) : this()
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width", "must be greater than 0");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height", "must be greater than 0");

            Width = Math.Min(width, 640);
            Height = Math.Min(height, 480);
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        #region Equlity Members

        public bool Equals(PictureSize other)
        {
            return Width == other.Width && Height == other.Height;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is PictureSize && Equals((PictureSize) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Width*397) ^ Height;
            }
        }

        #endregion

    }
}