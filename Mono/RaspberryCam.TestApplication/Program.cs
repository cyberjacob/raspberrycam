using System;
using System.IO;
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

            //cameras.Get("Camera 1").SavePicture(new PictureSize(640, 480), "Test3.jpg", 20);

            //var videoServer = new HttpVideoServer(80, cameras);
            
            //var videoServer = new TcpVideoServer(8080, cameras);
            //Console.WriteLine("Server strating ...");
            //videoServer.Start();
            //Console.WriteLine("Server strated.");

            IntPtr src = RaspberryCamInterop.FakeOpen("/dev/video0", 640, 480);

            Console.WriteLine("src adress: {0}", src);

            RaspberryCamInterop.DisplaySrc(src);


            Console.WriteLine("Press any key to quit ...");
            Console.ReadKey(true);
        }
    }
}
