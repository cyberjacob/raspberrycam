using System;

namespace RaspberryCam
{
    public class CamManagerBuilder
    {
        private string name;
        private string devicePath;
        private readonly CamManager camManager;

        public CamManagerBuilder(CamManager camManager)
        {
            this.camManager = camManager;
        }

        public CamManagerBuilder Named(string name)
        {
            this.name = name;
            return this;
        }
        
        public CamManagerBuilder WithDevicePath(string devicePath)
        {
            this.devicePath = devicePath;
            return this;
        }

        public CamManagerBuilder AndDevice()
        {
            AddCamera();

            return new CamManagerBuilder(camManager);
        }

        private void AddCamera()
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Please use Named to define a device alias (example: camera 1)");

            if (string.IsNullOrWhiteSpace(devicePath))
                throw new InvalidOperationException("Please use WithDevicePath to define device path (example: /dev/video0)");

            camManager.AddCamera(name, devicePath);
        }

        public Cameras Memorize()
        {
            AddCamera();

            return new Cameras(camManager);
        }
    }
}