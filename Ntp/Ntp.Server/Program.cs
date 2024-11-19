using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ntp.Common;
using System.Net;

namespace Ntp.Server;

internal class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddTransient<TcpListenerService>();
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        try
        {
            // Create stopping token
            var stoppingTokenSource = new CancellationTokenSource();

            // Handle ctrl-c to stop the app
            Console.CancelKeyPress += (object? sender, ConsoleCancelEventArgs e) =>
            {
                stoppingTokenSource.Cancel();

                // Return true so that process is not automatically terminated. We want to clean up
                e.Cancel = true;
            };


            // Get and start the listener service
            var tcpListenerService = host.Services.GetRequiredService<TcpListenerService>();
            await tcpListenerService.Execute(IPAddress.Any, 123, stoppingTokenSource.Token);

            // Loop while process not closed
            while (!stoppingTokenSource.Token.IsCancellationRequested)
            {
                // Sleep a bit to release control
                await Task.Delay(100, stoppingTokenSource.Token);
            }

            // If was not stopped through a handler then cancel so that all dependants are stopped
            if (!stoppingTokenSource.Token.IsCancellationRequested)
            {
                stoppingTokenSource.Cancel(true);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex);
        }
    }
}

