using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using AForge.Video.DirectShow;
using AForge.Video.VFW;

namespace RaspberryCam.CompressionLearning
{
    class Program
    {
        static void Main(string[] args)
        {

            foreach (FilterInfo videoDevice in new FilterInfoCollection(FilterCategory.VideoInputDevice))
                Console.WriteLine(videoDevice.Name);

            FilterInfo codec = null;

            foreach (FilterInfo videoDevice in new FilterInfoCollection(FilterCategory.VideoCompressorCategory))
            {
                codec = videoDevice;
                Console.WriteLine(videoDevice.Name);
            }

            // instantiate AVI writer, use WMV3 codec
            AVIWriter writer = new AVIWriter(codec.MonikerString);
            // create new AVI file and open it
            writer.Open("test.avi", 320, 240);
            // create frame image
            Bitmap image = new Bitmap(320, 240);

            int j = 1;

            for (int i = 0; i < 240; i++)
            {
                // update image
                image.SetPixel(i, i, Color.Red);
                // add the image as a new frame of video file
                writer.AddFrame(image);

                j = j == 1 ? -1 : 1;
            }
            
            //Bitmap bitmap = Image.FromFile("Files/grab.mjpeg");
            //writer.AddFrame(bitmap);
            
            


            writer.Close();
        }
    }
}
