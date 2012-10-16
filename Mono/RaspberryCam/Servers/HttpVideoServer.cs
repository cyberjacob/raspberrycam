using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace RaspberryCam.Servers
{
    public class HttpVideoServer
    {
        private readonly Cameras cameras;
        private readonly HttpListener listener;

        public HttpVideoServer(int port, Cameras cameras) : this(new []{"http://+:" + port + "/"})
        {
            this.cameras = cameras;
        }

        // HttpListener are "specials" in ARM version, so we will hide custom prefixes for the moment
        private HttpVideoServer(string[] prefixes)
        {
            if (!HttpListener.IsSupported)
                throw new InvalidOperationException("This OS does not support Http listeners.");

            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");
            
            listener = new HttpListener();
            
            foreach (var prefix in prefixes)
                listener.Prefixes.Add(prefix);
        }

        public void Start()
        {
            listener.Start();
            listener.BeginGetContext(OnGetContext, null);
        }

        public void Stop()
        {
            listener.Stop();
        }

        private void OnGetContext(IAsyncResult result)
        {
            HttpListenerContext context;
            try
            {

                context = listener.EndGetContext(result);
            }
            catch (HttpListenerException ex)
            {
                if (ex.ErrorCode == 995) //The I/O operation has been aborted because of either a thread exit or an application request.
                    return;

                if (ex.ErrorCode == 1229) //An operation was attempted on a nonexistent network connection
                    return; 

                throw;
            }

            listener.BeginGetContext(OnGetContext, null);
            
            try
            {
                ProcessRequest(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                using (var writer = new StreamWriter(context.Response.OutputStream))
                {
                    writer.WriteLine(ex.Message);
                    writer.Flush();
                }
            }

            try
            {
                context.Response.Close();
            }
            catch (HttpListenerException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            Console.WriteLine(context.Request.Url);
            Console.WriteLine(context.Request.Url.AbsoluteUri);
            Console.WriteLine(context.Request.Url.AbsolutePath);

            if (context.Request.Url.AbsolutePath.Equals("/picture.jpg", StringComparison.InvariantCultureIgnoreCase))
                TakePicture(context);

        }

        private void TakePicture(HttpListenerContext context)
        {
            HttpListenerRequest httpListenerRequest = context.Request;

            var parameters = GetParameters(httpListenerRequest.Url.Query);

            var width = int.Parse(parameters["width"]);
            var height = int.Parse(parameters["height"]);

            cameras.Default.
        }

        // I do that instead of HttpUtility.ParseQueryString, because I don't want System.Web as depency
        public static NameValueCollection GetParameters(string rawQuery)
        {
            var collection = new NameValueCollection();

            var query = rawQuery.StartsWith("?") ? rawQuery.Substring(1) : rawQuery;

            foreach(var item in query.Split('&'))
            {
                var parts = item.Split('=');
                collection.Add(parts[0], parts[1]);
            }

            return collection;
        }
    }
}
