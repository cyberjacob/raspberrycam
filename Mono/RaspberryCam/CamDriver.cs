using System;
using System.IO;
using System.Runtime.InteropServices;
using RaspberryCam.Interop;

namespace RaspberryCam
{
    public class CamDriver
    {
        private readonly string devicePath;
        private readonly PicturesCache picturesCache;

        public CamDriver(string devicePath)
        {
            this.devicePath = devicePath;
            picturesCache = new PicturesCache(4000);
        }

        public byte[] TakePicture(PictureSize pictureSize, Percent jpegCompressionRate = default(Percent))
        {
            Console.WriteLine("TakePicture");

            if (picturesCache.HasPicture(devicePath, pictureSize, jpegCompressionRate))
            {
                Console.WriteLine("Read picture from cache");
                return picturesCache.GetPicture(devicePath, pictureSize, jpegCompressionRate);
            }

            var pictureBuffer = RaspberryCamInterop.TakePicture(devicePath, (uint) pictureSize.Width, 
                (uint) pictureSize.Height, (uint)(100 - jpegCompressionRate.Value));
            var data = new byte[pictureBuffer.Size];

            Marshal.Copy(pictureBuffer.Data, data, 0, pictureBuffer.Size);

            picturesCache.AddPicture(devicePath, pictureSize, jpegCompressionRate, data);

            return data;
        }
        
        public void SavePicture(PictureSize pictureSize, string filename, Percent jpegCompressionRate = default(Percent))
        {
            var data = TakePicture(pictureSize, jpegCompressionRate);
            File.WriteAllBytes(filename, data);
        }
    }
}