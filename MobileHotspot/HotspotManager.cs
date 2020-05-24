using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Networking.NetworkOperators;

namespace MobileHotspot
{
    public class HotspotManager : IHotspotManager
    {
        private NetworkOperatorTetheringManager tetheringManager;

        private HotspotManager() { }

        /// <summary>
        /// Creates a new HotspotManager with the default connection profile.
        /// Returns null if no profile exists.
        /// </summary>
        public static async Task<HotspotManager> CreateAsync(string ssid, string passphrase)
        {
            var profile = await SelectBestProfile();

            if (profile != null)
            {
                var config = new NetworkOperatorTetheringAccessPointConfiguration
                {
                    Ssid = ssid,
                    Passphrase = passphrase
                };
                return await CreateAsync(profile, config);
            }

            return null;
        }

        /// <summary>
        /// Looks for profiles matching certain criteria and returns the first match.
        /// </summary>
        /// <remarks>
        /// Criteria are:
        /// <list type="number">
        /// <item>Connected unrestricted WiFi</item>
        /// <item>Any connected unrestricted profile</item>
        /// <item>Any connected profile</item>
        /// <item>Any profile</item>
        /// </list>
        /// </remarks>
        /// <returns></returns>
        private static async Task<ConnectionProfile> SelectBestProfile()
        {
            var filters = new ConnectionProfileFilter[]
            {
                new ConnectionProfileFilter
                {
                    IsConnected = true,
                    IsWlanConnectionProfile = true,
                    NetworkCostType = NetworkCostType.Unrestricted,
                },
                new ConnectionProfileFilter
                {
                    IsConnected = true,
                    NetworkCostType = NetworkCostType.Unrestricted,
                },
                new ConnectionProfileFilter
                {
                    IsConnected = true,
                },
                new ConnectionProfileFilter()
            };

            foreach (var filter in filters)
            {
                var profiles = await NetworkInformation.FindConnectionProfilesAsync(filter);
                if (profiles.Any())
                {
                    return profiles.First();
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a new HotspotManager with the given connection profile and configuration.
        /// </summary>
        public static async Task<HotspotManager> CreateAsync(ConnectionProfile profile, NetworkOperatorTetheringAccessPointConfiguration configuration)
        {
            var hotspot = new HotspotManager
            {
                tetheringManager = NetworkOperatorTetheringManager.CreateFromConnectionProfile(profile)
            };
            await hotspot.tetheringManager.ConfigureAccessPointAsync(configuration);
            return hotspot;
        }

        public IEnumerable<NetworkOperatorTetheringClient> Clients => tetheringManager.GetTetheringClients();

        public event EventHandler<NetworkOperatorTetheringClient> ClientConnected;
        public event EventHandler<NetworkOperatorTetheringClient> ClientDisconnected;

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            await tetheringManager.StartTetheringAsync();
            var lastSeenClients = new HashSet<NetworkOperatorTetheringClient>(new ClientComparer());


            while (true)
            {
                // Continuously poll the connected clients
                var currentClients = tetheringManager.GetTetheringClients();

                // Compare with the saved clients to see which are new / which are gone
                var newClients = currentClients.Except(lastSeenClients, new ClientComparer()).ToArray();
                var goneClients = lastSeenClients.Except(currentClients, new ClientComparer()).ToArray();

                foreach (var client in goneClients)
                {
                    ClientDisconnected?.Invoke(this, client);
                    lastSeenClients.Remove(client);
                }

                foreach (var client in newClients)
                {
                    ClientConnected?.Invoke(this, client);
                    lastSeenClients.Add(client);
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    await tetheringManager.StopTetheringAsync();
                    return;
                }

                await Task.Delay(10000);
            }
        }
    }
}