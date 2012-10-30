using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RaspberryCam.Tests.Compression;

namespace RaspberryCam.Tests
{
    [TestFixture]
    public class ImageCompressionTests
    {
        const int BlockSize = 16;

        [Test]
        public void When_store_with_BitmapCompressor()
        {
            var bytes1 = File.ReadAllBytes("Files/webcam_screenshot1.jpg");

            var stopwatch = Stopwatch.StartNew();

            var memoryStream = new MemoryStream(bytes1);
            var image = Image.FromStream(memoryStream);
            var bitmap = new Bitmap(image);
            var blocksBuilder = new BlocksBuilder(bitmap);
            var pixelsBlocks = new List<PixelsBlock>();

            for (int i = 0; i < blocksBuilder.GetBlockCount(); i++)
            {
                var block = blocksBuilder.GetPixelsBlock(i);
                pixelsBlocks.Add(block);
            }

            var compressor = new BitmapCompressor();

            using (FileStream fileStream = File.OpenWrite("out.compressed"))
            using (var writer = new BinaryWriter(fileStream))
            {
                foreach (var block in pixelsBlocks)
                {
                    foreach (Pixel pixel in block.Pixels)
                    {
                        ushort color = compressor.CompressColor(pixel.Color);
                        writer.Write(color);
                    }
                }

                fileStream.Flush(true);
            }

            stopwatch.Stop();
            var elapsed = stopwatch.Elapsed;
        }

        [Test]
        public void When_rounding_colors()
        {
            var bytes1 = File.ReadAllBytes("Files/webcam_screenshot1.jpg");

            var stopwatch = Stopwatch.StartNew();

            var memoryStream = new MemoryStream(bytes1);
            var image = Image.FromStream(memoryStream);
            var bitmap = new Bitmap(image);
            var blocksBuilder = new BlocksBuilder(bitmap);
            var pixelsBlocks = new List<PixelsBlock>();

            for (int i = 0; i < blocksBuilder.GetBlockCount(); i++)
            {
                var block = blocksBuilder.GetPixelsBlock(i);
                pixelsBlocks.Add(block);
            }

            var output = new Bitmap(bitmap.Width, bitmap.Height);

            foreach (var block in pixelsBlocks)
            {
                foreach (Pixel pixel in block.Pixels)
                {
                    //var colorEncodingRate = 4; // coding with 6 bits (255/4 = 63)
                    //var bits = 6;
                    
                    var colorEncodingRate = 8; // coding with 5 bits (255/8 = 31)
                    var bits = 5;
                    
                    ushort red = (ushort)Math.Max(0, pixel.Color.R / colorEncodingRate * colorEncodingRate);
                    ushort green = (ushort)Math.Max(0, pixel.Color.G / colorEncodingRate * colorEncodingRate);
                    ushort blue = (ushort)Math.Max(0, pixel.Color.B / colorEncodingRate * colorEncodingRate);

                    int pixelColor = red;
                    
                    pixelColor = (pixelColor << bits) + green;
                    pixelColor = (pixelColor << bits) + blue;

                    int pixelColor2 = pixelColor;

                    ushort blue2 = (ushort) (pixelColor2 & 0x0000FF);
                    pixelColor2 = pixelColor2 >> bits;
                    ushort green2 = (ushort) (pixelColor2 & 0x0000FF);
                    pixelColor2 = pixelColor2 >> bits;
                    ushort red2 = (ushort) (pixelColor2 & 0x0000FF);


                    //var color = Color.FromArgb(red, green, blue);
                    var color = Color.FromArgb(red2, green2, blue2);

                    output.SetPixel(pixel.X, pixel.Y, color);
                }
            }

            output.Save("When_rounding_colors.bmp");

            stopwatch.Stop();
            var elapsed = stopwatch.Elapsed;
        }

        [Test]
        public void When_use_Fast_block_bluider()
        {
            var bytes1 = File.ReadAllBytes("Files/webcam_screenshot1.jpg");

            var stopwatch = Stopwatch.StartNew();

            var memoryStream = new MemoryStream(bytes1);
            var image = Image.FromStream(memoryStream);
            var bitmap = new Bitmap(image);

            var blocksBuilder = new BlocksBuilder(bitmap);
            //var pixelsBlocks = blocksBuilder.GetBlocks();
            //var pixelsBlocks = blocksBuilder.GetBlocksFast();

            


            stopwatch.Stop();
            var elapsed = stopwatch.Elapsed;
        }

        [Test]
        public void When_drawing_image()
        {
            var bytes1 = File.ReadAllBytes("Files/webcam_screenshot1.jpg");

            var stopwatch = Stopwatch.StartNew();

            var memoryStream = new MemoryStream(bytes1);
            var image = Image.FromStream(memoryStream);
            var bitmap = new Bitmap(image);

            var blocksBuilder = new BlocksBuilder(bitmap);
            //var pixelsBlocks = blocksBuilder.GetBlocks();
            //var pixelsBlocks = blocksBuilder.GetBlocksFast();

            var pixelsBlocks = new List<PixelsBlock>();

            bool altern = false;
            for (int i = 0; i < blocksBuilder.GetBlockCount(); i++)
            {
                altern = !altern;
                //if (altern)
                //    continue;
                var block = blocksBuilder.GetPixelsBlock(i);
                pixelsBlocks.Add(block);
            }

            var output = new Bitmap(bitmap.Width, bitmap.Height);

            foreach (var block in pixelsBlocks)
            {
                foreach (var pixel in block.Pixels)
                {
                    output.SetPixel(pixel.X, pixel.Y, pixel.Color);
                }
            }

            output.Save("When_drawing_image.bmp");

            stopwatch.Stop();
            var elapsed = stopwatch.Elapsed;   
        }

        [Test]
        public void When_generating_combinaisons()
        {
            int colorPalette = 255 / 8;

            var stopwatch = Stopwatch.StartNew();

            var combinaisons = new List<ushort>();
            var compressor = new BitmapCompressor();

            Int64 count = 0;

            for (int x = 0; x < BlockSize; x++)
            {
                for (int y = 0; y < BlockSize; y++)
                {
                    for (byte red = 0; red < colorPalette; red++)
                    {
                        for (byte green = 0; green < colorPalette; green++)
                        {
                            for (byte blue = 0; blue < colorPalette; blue++)
                            {
                                //combinaisons.Add(new ColorCombinaison
                                //                        {
                                //                            Red = red,
                                //                            Green = green,
                                //                            Blue = blue
                                //                        });

                                combinaisons.Add(compressor.CompressColor(Color.FromArgb(red, green, blue)));

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

            Assert.IsTrue(pixelsBlocks.All(b => b.Pixels.Count == BlocksBuilder.BlockSize*BlocksBuilder.BlockSize));

            for (int i = 0; i < pixelsBlocks.Count; i++)
            {
                for (int j = 0; j < pixelsBlocks[i].Pixels.Count; j++)
                {
                    Assert.AreEqual(pixelsBlocks[i].Pixels[j].X, pixelsBlocks2[i].Pixels[j].X);
                    Assert.AreEqual(pixelsBlocks[i].Pixels[j].Y, pixelsBlocks2[i].Pixels[j].Y);
                    Assert.AreEqual(pixelsBlocks[i].Pixels[j].Color, pixelsBlocks2[i].Pixels[j].Color);
                }
            }

        }
    }
}
