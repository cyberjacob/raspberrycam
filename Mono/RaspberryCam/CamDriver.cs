using System.IO;
using System.Runtime.InteropServices;
using RaspberryCam.Interop;

namespace RaspberryCam
{
    public class CamDriver
    {
        private readonly string devicePath;

        public CamDriver(string devicePath)
        {
            this.devicePath = devicePath;
        }

        public byte[] TakePicture(PictureSize pictureSize, Percent jpegCompressionRate = default(Percent))
        {
            var pictureBuffer = RaspberryCamInterop.TakePicture(devicePath, (uint)pictureSize.Width, (uint)pictureSize.Height, 
                (uint)(100 - jpegCompressionRate.Value));
            var data = new byte[pictureBuffer.Size];

            Marshal.Copy(pictureBuffer.Data, data, 0, pictureBuffer.Size);

            return data;
        }
        
        public void SavePicture(PictureSize pictureSize, string filename, Percent jpegCompressionRate = default(Percent))
        {
            var data = TakePicture(pictureSize, jpegCompressionRate);
            File.WriteAllBytes(filename, data);
        }
    }
}