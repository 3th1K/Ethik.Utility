using System.Net.NetworkInformation;

namespace Ethik.Utility.Api.Network;

public static class NetworkUtility
{
    public static async Task<bool> CheckNetworkAvailabilityAsync(string host)
    {
        try
        {
            Ping ping = new();
            PingReply reply = await ping.SendPingAsync(host);

            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }
}