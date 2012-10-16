using System.Collections.Generic;

namespace RaspberryCam
{
    public class CamManager
    {
        private readonly Dictionary<string, CamDriver> drivers;

        public CamManager()
        {
            drivers = new Dictionary<string, CamDriver>();
        }

        public CamManager(Dictionary<string, CamDriver> drivers)
        {
            this.drivers = drivers;
        }

        public void AddCamera(string name, string devicePath)
        {
            drivers.Add(name, new CamDriver(devicePath));
        }

        public CamDriver Get(string name)
        {
            CamDriver driver;
            if (!drivers.TryGetValue(name, out driver))
                return null;

            return driver;
        }
    }
}