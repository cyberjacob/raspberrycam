using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using RaspberryCam.Interop;

namespace RaspberryCam
{
    public class CamDriver
    {
        private readonly string devicePath;
        private readonly PicturesCache picturesCache;
        private IntPtr videoStreamHandle;
        private int videoStreamFps;
        private int videoFramesInterval;
        private DateTime lastFrameTime;

        public CamDriver(string devicePath)
        {
            this.devicePath = devicePath;
            picturesCache = new PicturesCache(4000);
            videoStreamHandle = IntPtr.Zero;
        }

        public bool IsVideoStreamOpenned
        {
            get { return videoStreamHandle != IntPtr.Zero; }
        }

        public void StartVideoStreaming(PictureSize pictureSize, int fps = 20)
        {
            if (IsVideoStreamOpenned)
                return;
            videoStreamFps = fps;
            videoFramesInterval = 1000/videoStreamFps;
            videoStreamHandle = RaspberryCamInterop.OpenCameraStream(devicePath, 
                (uint)pictureSize.Width, (uint)pictureSize.Height, (uint) fps);
        }

        public void StopVideoStreaming()
        {
            if (!IsVideoStreamOpenned)
                return;
            RaspberryCamInterop.CloseCameraStream(videoStreamHandle);
            videoStreamHandle = IntPtr.Zero;
        }

        public byte[] GetVideoFrame(Percent compressionRate = default(Percent))
        {
            if (!IsVideoStreamOpenned)
                return new byte[0];

            if (!CanReadVideoFrame)
                Thread.Sleep(RemainingWaitingMilliseconds);

            var pictureBuffer = RaspberryCamInterop.ReadVideoFrame(videoStreamHandle, (uint)(100 - compressionRate.Value));

            var data = new byte[pictureBuffer.Size];
            Marshal.Copy(pictureBuffer.Data, data, 0, pictureBuffer.Size);

            return data;
        }

        private bool CanReadVideoFrame
        {
            get
            {
                var totalMilliseconds = RemainingWaitingMilliseconds;

                return totalMilliseconds >= videoFramesInterval;
            }
        }

        private int RemainingWaitingMilliseconds
        {
            get { return Math.Max(0, (int)DateTime.UtcNow.Subtract(lastFrameTime).TotalMilliseconds); }
        }

        public byte[] TakePicture(PictureSize pictureSize, Percent jpegCompressionRate = default(Percent))
        {
            if (IsVideoStreamOpenned)
                return new byte[0];

            Console.WriteLine("TakePicture");

            if (picturesCache.HasPicture(devicePath, pictureSize, jpegCompressionRate))
            {
                Console.WriteLine("Read picture from cache");
                return picturesCache.GetPicture(devicePath, pictureSize, jpegCompressionRate);
            }

            var pictureBuffer = RaspberryCamInterop.TakePicture(devicePath, (uint) pictureSize.Width, 
                (uint) pictureSize.Height, (uint)(100 - jpegCompressionRate.Value));
            var data = new byte[pictureBuffer.Size];

            Marshal.Copy(pictureBuffer.Data, data, 0, pictureBuffer.Size);

            picturesCache.AddPicture(devicePath, pictureSize, jpegCompressionRate, data);

            return data;
        }
        
        public void SavePicture(PictureSize pictureSize, string filename, Percent jpegCompressionRate = default(Percent))
        {
            var data = TakePicture(pictureSize, jpegCompressionRate);
            File.WriteAllBytes(filename, data);
        }
    }
}