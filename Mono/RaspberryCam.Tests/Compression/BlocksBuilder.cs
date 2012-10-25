using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace RaspberryCam.Tests.Compression
{
    public class BlocksBuilder
    {
        public const int BlockSize = 16;

        private readonly Bitmap bitmap;
        private readonly int blockCountX;
        private readonly int blockCountY;

        public BlocksBuilder(Bitmap bitmap)
        {
            this.bitmap = bitmap;

            blockCountX = bitmap.Width / BlockSize;
            blockCountY = bitmap.Height / BlockSize;
        }

        private const int bytesPerPixel = 4;

        public int GetBlockCount()
        {
            return blockCountX*blockCountY;
        }

        public Rectangle GetBlockBounds(int index)
        {
            var left = index % blockCountX;
            var top = index/blockCountX;// +(index % blockCountX == 0 ? 0 : 1);

            return new Rectangle(BlockSize * left, BlockSize * top, BlockSize, BlockSize);
        }

        public PixelsBlock GetPixelsBlock(Rectangle bounds)
        {
            var pixels = new List<Pixel>();

            for (int x = bounds.Left; x < bounds.Right; x++)
            {
                for (int y = bounds.Top; y < bounds.Bottom; y++)
                {
                    var color = bitmap.GetPixel(x, y);
                    pixels.Add(new Pixel(x, y, color));
                }
            }

            return new PixelsBlock(pixels);
        }

        public PixelsBlock GetPixelsBlock(int index)
        {
            var bounds = GetBlockBounds(index);
            
            return GetPixelsBlock(bounds);
        }

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
            var blocks = new List<PixelsBlock>();

            var imageData = GetImageData();

            for (int blockX = 0; blockX < blockCountX; blockX++)
            {
                for (int blockY = 0; blockY < blockCountY; blockY++)
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

        public List<PixelsBlock> GetBlocks2()
        {
            var pixelsBlocks = new List<PixelsBlock>();

            for (int i = 0; i < GetBlockCount(); i++)
            {
                var block = GetPixelsBlock(i);
                pixelsBlocks.Add(block);
            }

            return pixelsBlocks;
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
}