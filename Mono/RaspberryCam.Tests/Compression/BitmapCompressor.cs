using System;
using System.Drawing;

namespace RaspberryCam.Tests.Compression
{
    public class BitmapCompressor
    {
        public int colorEncodingRate = 8; // coding with 5 bits (255/8 = 31)
        public int bits = 5;

        public ushort CompressColor(Color color)
        {
            var red = (ushort)Math.Max(0, color.R / colorEncodingRate * colorEncodingRate);
            var green = (ushort)Math.Max(0, color.G / colorEncodingRate * colorEncodingRate);
            var blue = (ushort)Math.Max(0, color.B / colorEncodingRate * colorEncodingRate);

            int pixelColor = red;

            pixelColor = (pixelColor << bits) + green;
            pixelColor = (pixelColor << bits) + blue;

            return (ushort) pixelColor;
        }

        public Color DecompressColor(ushort pixelColor)
        {
            int pixelColor2 = pixelColor;

            ushort blue2 = (ushort)(pixelColor2 & 0x0000FF);
            pixelColor2 = pixelColor2 >> bits;
            ushort green2 = (ushort)(pixelColor2 & 0x0000FF);
            pixelColor2 = pixelColor2 >> bits;
            ushort red2 = (ushort)(pixelColor2 & 0x0000FF);

            return Color.FromArgb(red2, green2, blue2);
        }
    }
}