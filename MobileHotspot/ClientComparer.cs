using System.Collections.Generic;
using Windows.Networking.NetworkOperators;

namespace MobileHotspot
{
    /// <summary>
    /// Compares client instances for equality by comparing their MAC addresses.
    /// </summary>
    public class ClientComparer : IEqualityComparer<NetworkOperatorTetheringClient>
    {
        public bool Equals(NetworkOperatorTetheringClient x, NetworkOperatorTetheringClient y) => x.MacAddress == y.MacAddress;

        public int GetHashCode(NetworkOperatorTetheringClient obj) => obj.MacAddress.GetHashCode();
    }
}
