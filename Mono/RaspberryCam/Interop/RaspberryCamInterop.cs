using System;
using System.Runtime.InteropServices;

namespace RaspberryCam.Interop
{
    public class RaspberryCamInterop
    {
        [DllImport("RaspberryCam.so", EntryPoint = "TakePicture")]
        public static extern PictureBuffer TakePicture(string device, uint width, uint height, uint jpegQuantity);

        [DllImport("RaspberryCam.so", EntryPoint = "DisplaySrc")]
        public static extern void DisplaySrc(IntPtr src);

        [DllImport("RaspberryCam.so", EntryPoint = "FakeOpen")]
        public static extern IntPtr FakeOpen(string device, uint width, uint height);

    }
}
