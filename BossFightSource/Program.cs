using System;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

namespace BossFight
{
    public class Program
    {

        public static void Main(string[] args)
        {
            //CreateHostBuilder(args).Build().Run();
            var httpsv = new HttpServer(5000, true);
            httpsv.Log.Level = LogLevel.Trace;

            // To provide the secure connection.
            httpsv.SslConfiguration.ServerCertificate = new X509Certificate2("/etc/letsencrypt/live/bossfight.ix.tc/fullchain.pem");


            // Set the HTTP GET request event.
            httpsv.OnGet += (sender, e) =>
            {
                var req = e.Request;
                var res = e.Response;

                var path = req.RawUrl;

                if (path == "/")
                    path += "index.html";

                byte[] contents = new byte[300];
                var stream = e.Response.OutputStream;
                stream.Read(contents);

                if (path.EndsWith(".html"))
                {
                    res.ContentType = "text/html";
                    res.ContentEncoding = Encoding.UTF8;
                }
                else if (path.EndsWith(".js"))
                {
                    res.ContentType = "application/javascript";
                    res.ContentEncoding = Encoding.UTF8;
                }

                res.ContentLength64 = contents.LongLength;

                res.Close(contents, true);
            };

            // Add the WebSocket services.
            httpsv.AddWebSocketService<Echo>("/Echo");
            httpsv.AddWebSocketService<Chat>("/Chat");

            httpsv.Start();

            if (httpsv.IsListening)
            {
                Console.WriteLine("Listening on port {0}, and providing WebSocket services:", httpsv.Port);

                foreach (var path in httpsv.WebSocketServices.Paths)
                    Console.WriteLine("- {0}", path);
            }

            Console.WriteLine("\nPress Enter key to stop the server...");
            Console.ReadLine();

            httpsv.Stop();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }

    public class Echo : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine(e.Data);
            Send(e.Data);
        }
    }

    public class Chat : WebSocketBehavior
    {
        private string _name;
        private static int _number = 0;
        private string _prefix;

        public Chat()
        {
            _prefix = "anon#";
        }

        public string Prefix
        {
            get
            {
                return _prefix;
            }

            set
            {
                _prefix = !value.IsNullOrEmpty() ? value : "anon#";
            }
        }

        private string getName()
        {
            var name = Context.QueryString["name"];

            return !name.IsNullOrEmpty() ? name : _prefix + getNumber();
        }

        private static int getNumber()
        {
            return Interlocked.Increment(ref _number);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            if (_name == null)
                return;

            var fmt = "{0} got logged off...";
            var msg = String.Format(fmt, _name);

            Sessions.Broadcast(msg);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            var fmt = "{0}: {1}";
            var msg = String.Format(fmt, _name, e.Data);

            Sessions.Broadcast(msg);
        }

        protected override void OnOpen()
        {
            _name = getName();

            var fmt = "{0} has logged in!";
            var msg = String.Format(fmt, _name);

            Sessions.Broadcast(msg);
        }
    }
}
