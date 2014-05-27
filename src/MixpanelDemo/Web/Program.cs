using System;
using Nancy.Hosting.Self;

namespace Web
{
    class Program
    {
        static void Main(string[] args)
        {
            var hostConfiguration = new HostConfiguration
            {
                UrlReservations = new UrlReservations
                {
                    CreateAutomatically = true
                }
            };
            var baseUri = new Uri("http://localhost:1234");

            using (var host = new NancyHost(hostConfiguration, baseUri))
            {
                host.Start();
                Console.WriteLine("Listening on '{0}'", baseUri.AbsoluteUri);
                Console.ReadLine();
            }
        }
    }
}
