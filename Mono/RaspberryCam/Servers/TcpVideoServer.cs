using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace RaspberryCam.Servers
{
    public static class VideoServerCommands
    {
        public const string TakePicture = "/picture.jpg";
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
            listener.Start();

            while (true)
            {
                Console.Write("Waiting for a connection... ");

                var client = listener.AcceptTcpClient();
                Console.WriteLine("Connected!");

                HandleClient(client);
            }
        }

        private void HandleClient(TcpClient client)
        {
            var bytes = new byte[256];

            var stream = client.GetStream();

            int i;
            var data = string.Empty;

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

            Console.WriteLine("httpMethodQuery.Query: {0}", httpMethodQuery.Query);

            var urlParameters = httpMethodQuery.Query.ParseUrlParameters();

            switch (httpMethodQuery.Path)
            {
                case VideoServerCommands.TakePicture:

                    Console.WriteLine("TakePicture: {0}", httpMethodQuery.PathAndQuery);

                    TakePicture(urlParameters, client.GetStream());
                    break;
            }
            

            client.Close();
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

            //context.Response.ContentType = "image/jpeg";

            using (var writer = new BinaryWriter(responseStream))
            {
                writer.Write(data);
                writer.Flush();
                writer.Close();
            }
        }
    }
}