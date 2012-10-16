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
        public IntPtr Data;
    }

    public class RaspberryCamInterop
    {
        [DllImport("RaspberryCam.so", EntryPoint = "TakePicture")]
        public static extern PictureBuffer TakePicture(string device, uint width, uint height, uint jpegQuantity);
    }

    public sealed class PictureBufferMarshaler : ICustomMarshaler
    {
        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            throw new NotImplementedException();
        }

        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            throw new NotImplementedException();
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            throw new NotImplementedException();
        }

        public void CleanUpManagedData(object ManagedObj)
        {
            throw new NotImplementedException();
        }

        public int GetNativeDataSize()
        {
            throw new NotImplementedException();
        }
    }
}
