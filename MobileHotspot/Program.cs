using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.NetworkOperators;

namespace MobileHotspot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("usage: mobilehotspot.exe SSID PASSPHRASE");
                return;
            }

            var cancellationTokenSource = new CancellationTokenSource();

            var hotspot = await HotspotManager.CreateAsync(args[0], args[1]);

            hotspot.ClientConnected += (_, clientInfo) => Console.WriteLine($"CON | {ToString(clientInfo)}");
            hotspot.ClientDisconnected += (_, clientInfo) => Console.WriteLine($"DIS | {ToString(clientInfo)}");

            Console.WriteLine("Starting hotspot ... press \"q\" and Enter to quit");

            var hotspotTask = hotspot.RunAsync(cancellationTokenSource.Token);

            // wait for 'q'
            while (Convert.ToChar(Console.Read()) != 'q') ;

            Console.WriteLine("Stopping hotspot ...");
            cancellationTokenSource.Cancel();
            hotspotTask.GetAwaiter().GetResult();
        }

        private static string ToString(NetworkOperatorTetheringClient client)
        {
            string hostNames = string.Join(" | ", client.HostNames);
            return $"{client.MacAddress} | {hostNames}";
        }
    }
}
