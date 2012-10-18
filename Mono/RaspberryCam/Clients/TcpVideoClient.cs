using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace RaspberryCam.Clients
{
    public class TcpVideoClient
    {
        private readonly TcpClient tcpClient;
        private readonly WebClient webClient;
        public string ServerHostIp { get; private set; }
        public int ServerPort { get; private set; }

        

        public TcpVideoClient(string serverHostIp, int serverPort)
        {
            ServerPort = serverPort;
            ServerHostIp = serverHostIp;

            tcpClient = new TcpClient();
            webClient = new WebClient();
        }

        public void StartVideoStreaming(PictureSize pictureSize)
        {
            var url = string.Format("http://{0}:{1}/StartVideoStreaming?width={2}&height={3}",
                ServerHostIp, ServerPort, pictureSize.Width, pictureSize.Height);

            try
            {
                webClient.DownloadString(url);
            }
            catch
            {
                
            }
            

            //tcpClient.Connect(ServerHostIp, ServerPort);

            //var command = string.Format("/StartVideoStreaming?width={0}&height={1}", pictureSize.Width, pictureSize.Height);

            //SendHttpCommand(command);

            //tcpClient.Close();
        }

        private void SendHttpCommand(string command)
        {
            var httpGetRequest = string.Format("GET http://{0}:{1}{2} HTTP/1.1", 
                ServerHostIp, ServerPort, command);

            using (var networkStream = tcpClient.GetStream())
            using (var writer = new StreamWriter(networkStream))
            {
                writer.WriteLine(httpGetRequest);
                writer.WriteLine();
                writer.WriteLine();
                writer.Flush();
            }
        }

        public void StopVideoStreaming()
        {
            var url = string.Format("http://{0}:{1}/StopVideoStreaming", ServerHostIp, ServerPort);

            //webClient.DownloadString(url);
            try
            {
                webClient.DownloadString(url);
            }
            catch
            {

            }
        }

        public byte[] GetVideoFrame(Percent compressionRate)
        {
            var url = string.Format("http://{0}:{1}/GetVideoFrame?compressionRate={2}",
                ServerHostIp, ServerPort, compressionRate.Value);

            return webClient.DownloadData(url);
            //return new byte[0];
        }
    }
}
