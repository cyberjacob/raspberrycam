using System.Net;

namespace RaspberryCam.Clients
{
    public class TcpVideoClient
    {
        private readonly WebClient webClient;
        public string ServerHostIp { get; private set; }
        public int ServerPort { get; private set; }

        public TcpVideoClient(string serverHostIp, int serverPort)
        {
            ServerPort = serverPort;
            ServerHostIp = serverHostIp;

            webClient = new WebClient();
        }

        public void StartVideoStreaming(PictureSize pictureSize, int fps = 2)
        {
            var url = string.Format("http://{0}:{1}/StartVideoStreaming?width={2}&height={3}&fps={4}",
                ServerHostIp, ServerPort, pictureSize.Width, pictureSize.Height, fps);

            webClient.DownloadString(url);
        }
        
        public void StopVideoStreaming()
        {
            var url = string.Format("http://{0}:{1}/StopVideoStreaming", ServerHostIp, ServerPort);
            webClient.DownloadString(url);
        }

        public byte[] GetVideoFrame(Percent compressionRate)
        {
            var url = string.Format("http://{0}:{1}/GetVideoFrame?compressionRate={2}",
                ServerHostIp, ServerPort, compressionRate.Value);

            return webClient.DownloadData(url);
        }
    }
}
