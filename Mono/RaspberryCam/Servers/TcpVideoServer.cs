using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RaspberryCam.Servers
{
    public static class VideoServerCommands
    {
        public const string TakePicture = "/picture.jpg";
        public const string StartVideoStreaming = "/StartVideoStreaming";
        public const string StopVideoStreaming = "/StopVideoStreaming";
        public const string GetVideoFrame = "/GetVideoFrame";
    }

    public class TcpVideoServer
    {
        private readonly int port;
        private readonly Cameras cameras;
        private readonly TcpListener listener;

        public TcpVideoServer(int port, Cameras cameras)
        {
            this.port = port;
            this.cameras = cameras;

            listener = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            Task.Factory.StartNew(() => 
            { 

                listener.Start();

                while (true)
                {
                    var client = listener.AcceptTcpClient();

                    Task.Factory.StartNew(() => HandleClient(client));
                }

            });
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                var bytes = new byte[256];
                int i;
                var data = string.Empty;
                using (var stream = client.GetStream())
                {

                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        data += System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        if (data.EndsWith("\r\n\r\n"))
                            break;
                    }

                    if (string.IsNullOrWhiteSpace(data))
                    {
                        client.Close();
                        return;
                    }

                    var lines = data.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
                    var httpMethodQuery = lines[0].ParseHttpMethodQuery();
                    var urlParameters = httpMethodQuery.Query.ParseUrlParameters();

                    switch (httpMethodQuery.Path)
                    {
                        case VideoServerCommands.TakePicture:
                            Console.WriteLine("TakePicture: {0}", httpMethodQuery.PathAndQuery);
                            TakePicture(urlParameters, stream);
                            break;
                        case VideoServerCommands.StartVideoStreaming:
                            StartVideoStreaming(urlParameters, stream);
                            Console.WriteLine("VideoStreaming started");
                            break;
                        case VideoServerCommands.StopVideoStreaming:
                            StopVideoStreaming(urlParameters, stream);
                            Console.WriteLine("VideoStreaming stopped");
                            break;
                        case VideoServerCommands.GetVideoFrame:
                            GetVideoFrame(urlParameters, stream);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex.Message);
            }
            finally
            {
                client.Close();   
            }
        }

        private void StopVideoStreaming(NameValueCollection parameters, NetworkStream responseStream)
        {
            var camDriver = cameras.Default;
            if (camDriver == null)
                return;

            camDriver.StopVideoStreaming();

            using (var writer = new StreamWriter(responseStream))
            {
                WriteHttpReponseHeader(writer, "text/txt");

                writer.Write("ok\n\n");
                writer.Flush();
                writer.Close();
            }
        }

        private void StartVideoStreaming(NameValueCollection parameters, NetworkStream responseStream)
        {
            if (!parameters.AllKeys.Contains("width") || !parameters.AllKeys.Contains("height"))
                throw new Exception("you have to specify width and height parameters.");

            var width = int.Parse(parameters["width"]);
            var height = int.Parse(parameters["height"]);
            var fps = parameters.AllKeys.Contains("fps") ? int.Parse(parameters["fps"]) : 20;
            
            var camDriver = cameras.Default;
            if (camDriver == null)
                return;

            camDriver.StartVideoStreaming(new PictureSize(width, height), fps);

            using (var writer = new StreamWriter(responseStream))
            {
                WriteHttpReponseHeader(writer, "text/txt");

                writer.Write("ok\r\n");
                writer.Flush();
                writer.Close();
            }
        }

        private void WriteHttpReponseHeader(StreamWriter writer, string contentType)
        {
            writer.Write("HTTP/1.0 200 OK\r\n");
            writer.Write("Server: RaspberryCam\r\n");
            writer.Write("Content-Type: {0}\r\n", contentType);
            writer.Write("\r\n");
            writer.Flush();
        }

        private void GetVideoFrame(NameValueCollection parameters, NetworkStream responseStream)
        {
            if (!parameters.AllKeys.Contains("compressionRate"))
                throw new Exception("you have to specify compressionRate parameter.");

            var compressionRate = int.Parse(parameters["compressionRate"]);
            
            var camDriver = cameras.Default;
            if (camDriver == null)
                return;

            using (var headWriter = new StreamWriter(responseStream))
            using (var writer = new BinaryWriter(responseStream))
            {
                WriteHttpReponseHeader(headWriter, "image/jpeg");

                var data = camDriver.GetVideoFrame(compressionRate);
                writer.Write(data);
                writer.Flush();
                writer.Close();
            }
        }

        public void Stop()
        {
            listener.Stop();
        }

        private void TakePicture(NameValueCollection parameters, NetworkStream responseStream)
        {
            if (!parameters.AllKeys.Contains("width") || !parameters.AllKeys.Contains("height"))
                throw new Exception("you have to specify width and height parameters.");

            var width = int.Parse(parameters["width"]);
            var height = int.Parse(parameters["height"]);

            var camDriver = cameras.Default;
            if (camDriver == null)
                return;

            var data = camDriver.TakePicture(new PictureSize(width, height));

            using (var writer = new BinaryWriter(responseStream))
            {
                writer.Write(data);
                writer.Flush();
                writer.Close();
            }
        }
    }
}