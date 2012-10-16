using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace RaspberryCam
{
    public struct PictureBuffer
    {
        public int Size;
        public string Data;
    }

    public class RaspberryCamInterop
    {
        [DllImport("RaspberryCam.so", EntryPoint = "TakePicture")]
        public static extern PictureBuffer TakePicture(string device, uint width, uint height, uint jpegQuantity);
    }
}
