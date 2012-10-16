using System.IO;

namespace RaspberryCam.TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            //var camDriver = new CamDriver("/dev/video0");
            //var data = camDriver.TakePicture(new PictureSize(640, 480), Percent.MinValue);

            //File.WriteAllBytes("test2.jpg", data);

            var cameras = Cameras.DeclareDevice()
                .Named("Camera 1").WithDevicePath("/dev/video0")
                .Memorize();

            cameras.Get("Camera 1").SavePicture(new PictureSize(640, 480), "Test3.jpg", 20);
        }
    }
}
