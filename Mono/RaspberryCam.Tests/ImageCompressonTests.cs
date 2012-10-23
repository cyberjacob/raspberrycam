using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace RaspberryCam.Tests
{
    [TestFixture]
    public class ImageCompressonTests
    {
        [Test]
        public void LoadBitmap1()
        {
            var bytes1 = File.ReadAllBytes("Files/webcam_screenshot1.jpg");

            var stopwatch = Stopwatch.StartNew();

            var memoryStream = new MemoryStream(bytes1);
            var image = Image.FromStream(memoryStream);
            var bitmap = new Bitmap(image);

            var blocksBuilder = new BlocksBuilder(bitmap);
            var pixelsBlocks = blocksBuilder.GetBlocks();

            stopwatch.Stop();

            var elapsed = stopwatch.Elapsed;

            var fps = 1000/stopwatch.ElapsedMilliseconds;
        }
    }

    public class BlocksBuilder
    {
        public const int BlockSize = 16;

        private readonly Bitmap bitmap;

        public BlocksBuilder(Bitmap bitmap)
        {
            this.bitmap = bitmap;
        }

        public List<PixelsBlock> GetBlocks()
        {
            var maxX = bitmap.Width / BlockSize;
            var maxY = bitmap.Height / BlockSize;

            var blocks = new List<PixelsBlock>();

            for (int blockX = 0; blockX < maxX; blockX++)
            {
                for (int blockY = 0; blockY < maxY; blockY++)
                {
                    var pixels = new List<Pixel>();

                    for (int x = blockX * BlockSize; x < blockX * BlockSize + BlockSize; x++)
                    {
                        for (int y = blockY * BlockSize; y < blockY * BlockSize + BlockSize; y++)
                        {
                            var color = bitmap.GetPixel(x, y);
                            pixels.Add(new Pixel(x, y, color));
                        }
                    }

                    var block = new PixelsBlock(pixels);
                    blocks.Add(block);
                }
            }

            return blocks;
        }
    }

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
