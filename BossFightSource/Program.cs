using System;
using System.Diagnostics;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Hosting;

namespace BossFight
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webHostBuilder =>
                {
                    webHostBuilder.UseKestrel(kestrelServerOptions =>
                    {
                        kestrelServerOptions.ConfigureEndpointDefaults(listenOptions =>
                        {
                            #if DEBUG
                                return;
                            #endif
                            var httpsConnectionAdapterOptions = new HttpsConnectionAdapterOptions()
                            {
                                SslProtocols = SslProtocols.Tls13,
                                CheckCertificateRevocation = false,  //  should this be true?
                                ClientCertificateMode = ClientCertificateMode.NoCertificate
                            };
                            httpsConnectionAdapterOptions.AllowAnyClientCertificate();

                            var cert = X509Certificate2.CreateFromPemFile("/etc/letsencrypt/live/bossfight.ix.tc/fullchain.pem", "/etc/letsencrypt/live/bossfight.ix.tc/privkey.pem");
                            if (cert == null)
                            {
                                throw new Exception("FAILED TO CREATE CERTIFICATE");
                            }
                            httpsConnectionAdapterOptions.ServerCertificate = cert;

                            listenOptions.UseHttps(httpsConnectionAdapterOptions);
                            listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                        });
                    });
                    webHostBuilder.UseStartup<Startup>();
                });
    }
}
