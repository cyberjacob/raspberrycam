using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using RaspberryCam.Interop;

namespace RaspberryCam.Benchmark
{
    class Program
    {
        private static void Main(string[] args)
        {

            Console.WriteLine("Starting...");

            var handle = RaspberryCamInterop.OpenCameraStream("/dev/video0", 640, 480, 20);


            var previousSize = 0;
            var data = new byte [0];

            for (int i = 0; i < 100; i++)
            {
                var watch = Stopwatch.StartNew();

                var pictureBuffer = RaspberryCamInterop.GrabVideoFrame(handle);
                watch.Stop();
                Console.WriteLine("GrabVideoFrame take: {0} ms", watch.ElapsedMilliseconds);

                /*
                    if we allocate a new buffer for each frame, the garbadge collector pause the app while collecting.
                */
                if (pictureBuffer.Size > previousSize)
                    data = new byte[pictureBuffer.Size];

                Marshal.Copy(pictureBuffer.Data, data, 0, pictureBuffer.Size);

                /*
                    if you uncomment this, you will solicit garbage collection and decrease performances
                */

                //MemoryStream s = new MemoryStream(data);
                //using (var fs = File.Open(string.Format("capture_{0}.raw", i), FileMode.Create))
                //{
                //    s.CopyTo(fs);
                //    s.Flush();
                //    fs.Flush();
                //}

            }

            RaspberryCamInterop.CloseCameraStream(handle);
        }

        
    }
}
