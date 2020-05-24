using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.NetworkOperators;

namespace MobileHotspot
{
    public interface IHotspotManager
    {
        /// <summary>
        /// Fires when a client connects to the network.
        /// </summary>
        event EventHandler<NetworkOperatorTetheringClient> ClientConnected;

        /// <summary>
        /// Fires when a clients disconnects from the hotspot.
        /// </summary>
        event EventHandler<NetworkOperatorTetheringClient> ClientDisconnected;

        /// <summary>
        /// Gets the currently connected clients.
        /// </summary>
        IEnumerable<NetworkOperatorTetheringClient> Clients { get; }

        /// <summary>
        /// Enable the mobile hotspot and regularly poll the API
        /// for connected clients. This method will usually not return.
        /// Use the CancellationToken to terminate the method.
        /// </summary>
        Task RunAsync(CancellationToken cancellationToken);
    }
}