namespace RaspberryCam
{
    public class Cameras
    {
        private readonly CamManager camManager;

        public Cameras(CamManager camManager)
        {
            this.camManager = camManager;
        }

        public static CamManagerBuilder DeclareDevice()
        {
            return new CamManagerBuilder(new CamManager());
        }

        public CamDriver Get(string name)
        {
            return camManager.Get(name);
        }

        public CamDriver Default
        {
            get { return camManager.Default; }
        }
    }
}