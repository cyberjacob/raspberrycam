using System;
using System.IO;
using System.Runtime.InteropServices;
using RaspberryCam.Interop;
using RaspberryCam.Servers;

namespace RaspberryCam.TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var cameras = Cameras.DeclareDevice()
                .Named("Camera 1").WithDevicePath("/dev/video0")
                .Memorize();

            //cameras.Get("Camera 1").SavePicture(new PictureSize(640, 480), "Test1.jpg", 20);
            //cameras.Default.SavePicture(new PictureSize(640, 480), "Test2.jpg", 20);

            var videoServer = new TcpVideoServer(8080, cameras);
            Console.WriteLine("Server strating ...");
            videoServer.Start();
            Console.WriteLine("Server strated.");


            //var handle = RaspberryCamInterop.OpenCameraStream("/dev/video0", 640, 480, 20);
            //var pictureBuffer = RaspberryCamInterop.GrabVideoFrame(handle);
            //var data = new byte[pictureBuffer.Size];
            //Marshal.Copy(pictureBuffer.Data, data, 0, pictureBuffer.Size);
            //File.WriteAllBytes("grab.mjpeg", data);
            //RaspberryCamInterop.CloseCameraStream(handle);


            Console.WriteLine("Press any key to quit ...");
            Console.ReadKey(true);

            videoServer.Stop();
        }
    }
}
