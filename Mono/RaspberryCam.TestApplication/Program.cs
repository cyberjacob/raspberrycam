using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace RaspberryCam.TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var pictureBuffer = RaspberryCamInterop.TakePicture("/dev/video0", 640, 480, 100);

            Console.WriteLine("Picture Buffer length: {0}", pictureBuffer.Size);
            //Console.WriteLine("Buffer data length: {0}", pictureBuffer.Data.Length);

            //var c = pictureBuffer.Data[pictureBuffer.Size - 1];

            var data = new byte[pictureBuffer.Size];

            Marshal.Copy(pictureBuffer.Data, data, 0, pictureBuffer.Size);

            Console.WriteLine("Buffer data length: {0}", data.Length);

            File.WriteAllBytes("test.jpg", data);
        }
    }
}
