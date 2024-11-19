using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Ntp.Common;

public class TcpListenerService(ILogger<TcpListenerService> logger)
{
    public async Task Execute(IPAddress ipAddress, int port, CancellationToken stoppingToken)
    {
        logger.LogDebug("{msg}", $"Executing TCP listener for address '{ipAddress}:{port}'");

        using var listener = new UdpClient(port);

        var endPoint = new IPEndPoint(ipAddress, port);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = await listener.ReceiveAsync(stoppingToken);
                logger.LogInformation("{msg}", $"{Encoding.ASCII.GetString(result.Buffer, 0, result.Buffer.Length)}");
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore just means console app was closed
        }
        catch (SocketException)
        {
            // Ignore just means client forcibly closed
        }
        catch (Exception ex)
        {
            logger.LogError(ex);
        }
    }
}
