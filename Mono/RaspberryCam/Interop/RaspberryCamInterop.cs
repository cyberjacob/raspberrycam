using System.Runtime.InteropServices;

namespace RaspberryCam.Interop
{
    public class RaspberryCamInterop
    {
        [DllImport("RaspberryCam.so", EntryPoint = "TakePicture")]
        public static extern PictureBuffer TakePicture(string device, uint width, uint height, uint jpegQuantity);
    }
}
