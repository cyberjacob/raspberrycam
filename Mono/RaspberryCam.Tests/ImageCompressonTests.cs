using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace RaspberryCam.Tests
{
    public class ColorCombinaison
    {
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }
    }

    [TestFixture]
    public class ImageCompressonTests
    {
        [Test]
        public void When_generating_combinaisons()
        {
            const int BlockSize = 2;

            var stopwatch = Stopwatch.StartNew();

            var combinaisons = new List<ColorCombinaison>();

            Int64 count = 0;

            for (int x = 0; x < BlockSize; x++)
            {
                for (int y = 0; y < BlockSize; y++)
                {
                    for (byte red = 0; red < 255; red++)
                    {
                        for (byte green = 0; green < 255; green++)
                        {
                            for (byte blue = 0; blue < 255; blue++)
                            {
                                //combinaisons.Add(new ColorCombinaison
                                //                        {
                                //                            Red = red,
                                //                            Green = green,
                                //                            Blue = blue
                                //                        });

                                count++;
                            }
                        }
                    }
                }
            }

            stopwatch.Stop();
            var elapsed = stopwatch.Elapsed;   
        }

        [Test]
        public void When_building_blocks()
        {
            var bytes1 = File.ReadAllBytes("Files/webcam_screenshot1.jpg");

            var stopwatch = Stopwatch.StartNew();

            var memoryStream = new MemoryStream(bytes1);
            var image = Image.FromStream(memoryStream);
            var bitmap = new Bitmap(image);

            //var graphics = Graphics.FromImage(bitmap);


            var blocksBuilder = new BlocksBuilder(bitmap);
            var pixelsBlocks = blocksBuilder.GetBlocks();

            //var imageData = blocksBuilder.GetImageData();

            var pixelsBlocks2 = blocksBuilder.GetBlocksFast();


            stopwatch.Stop();

            var elapsed = stopwatch.Elapsed;

            var fps = 1000/stopwatch.ElapsedMilliseconds;

            Assert.AreEqual(pixelsBlocks.First().Pixels.First().Color, pixelsBlocks2.First().Pixels.First().Color);
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

        private const int bytesPerPixel = 4;

        public byte[] GetImageData()
        {
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            // This code is specific to a bitmap with 32 bits per pixels 
            // (32 bits = 4 bytes, 3 for RGB and 1 byte for alpha).
            int numBytes = bitmap.Width * bitmap.Height * bytesPerPixel;
            var argbValues = new byte[numBytes];

            // Copy the ARGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, argbValues, 0, numBytes);

            // Manipulate the bitmap, such as changing the
            // RGB values for all pixels in the the bitmap.
            //for (int counter = 0; counter < argbValues.Length; counter += bytesPerPixel)
            //{
            //    // argbValues is in format BGRA (Blue, Green, Red, Alpha)

            //    // If 100% transparent, skip pixel
            //    if (argbValues[counter + bytesPerPixel - 1] == 0)
            //        continue;

            //    int pos = 0;
            //    pos++; // B value
            //    pos++; // G value
            //    pos++; // R value

            //    argbValues[counter + pos] = (byte)(argbValues[counter + pos] * 1.0);
            //}

            //// Copy the ARGB values back to the bitmap
            //System.Runtime.InteropServices.Marshal.Copy(argbValues, 0, ptr, numBytes);

            // Unlock the bits.
            bitmap.UnlockBits(bmpData);

            return argbValues;
        }

        public List<PixelsBlock> GetBlocksFast()
        {
            var maxX = bitmap.Width / BlockSize;
            var maxY = bitmap.Height / BlockSize;

            var blocks = new List<PixelsBlock>();

            var imageData = GetImageData();

            for (int blockX = 0; blockX < maxX; blockX++)
            {
                for (int blockY = 0; blockY < maxY; blockY++)
                {
                    var pixels = new List<Pixel>();

                    for (int x = blockX * BlockSize; x < blockX * BlockSize + BlockSize; x++)
                    {
                        for (int y = blockY * BlockSize; y < blockY * BlockSize + BlockSize; y++)
                        {
                            //var color = bitmap.GetPixel(x, y);
                            var index = x*y;
                            byte b = imageData[index];
                            byte g = imageData[index+1];
                            byte r = imageData[index+2];

                            pixels.Add(new Pixel(x, y, Color.FromArgb(r, g, b)));
                        }
                    }

                    var block = new PixelsBlock(pixels);
                    blocks.Add(block);
                }
            }

            return blocks;
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
