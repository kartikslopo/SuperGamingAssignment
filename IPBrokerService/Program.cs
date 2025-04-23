using IpBroker.Models;
using IpBroker.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace IpBroker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Configure providers with different characteristics:
            // - Small rate limits to force switching
            // - Different artificial delays to affect selection priority 
            // - Some providers set to simulate errors
            var providers = new List<ProviderStats>
            {
                new ProviderStats("IpInfo", "https://ipinfo.io", 2, "{0}/json", 100, simulateErrorRate: 0),
                new ProviderStats("IpApi", "http://ip-api.com", 3, "json/{0}", 150, simulateErrorRate: 50),        // 50% errors
                new ProviderStats("IpData", "https://api.ipdata.co", 2, "{0}", 200, simulateErrorRate: 0),
                new ProviderStats("IpStack", "http://api.ipstack.com", 2, "{0}", 300, simulateErrorRate: 80),      // 80% errors
                new ProviderStats("GeoPlugin", "http://www.geoplugin.net", 3, "json.gp?ip={0}", 250, simulateErrorRate: 20)   // 20% errors
            };

            var broker = new BrokerService(providers);

            Console.WriteLine("\n=== PHASE 1: INITIAL DISTRIBUTION ===");
            Console.WriteLine("Running 12 requests to show initial distribution and error handling...");

            var tasks = new List<Task>();
            for (int i = 0; i < 12; i++)
            {
                int requestId = i;  // Capture for async
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var location = await broker.GetLocationAsync("8.8.8.8");
                    }
                    catch (Exception ex)
                    {
                        // Errors are already logged in broker service
                    }
                }));

                // Small delay between requests to see provider selection clearly
                await Task.Delay(50);
            }
            await Task.WhenAll(tasks);
            tasks.Clear();

            // Show intermediate statistics
            await ShowProviderStatistics(providers);

            Console.WriteLine("\n=== PHASE 2: RATE LIMIT & ERROR RECOVERY ===");
            Console.WriteLine("Running more requests to demonstrate how system recovers and rebalances...");

            // Wait a bit to show clear separation between phases
            await Task.Delay(500);

            for (int i = 0; i < 15; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var location = await broker.GetLocationAsync("1.1.1.1");
                    }
                    catch (Exception ex)
                    {
                        // Errors are already logged in broker service
                    }
                }));

                // Small delay between requests
                await Task.Delay(50);
            }
            await Task.WhenAll(tasks);

            // Final statistics
            await ShowProviderStatistics(providers);
        }

        private static async Task ShowProviderStatistics(List<ProviderStats> providers)
        {
            await Task.Delay(200); // Give a moment for all stats to settle

            Console.WriteLine("\n=== PROVIDER STATISTICS ===");
            foreach (var provider in providers)
            {
                Console.WriteLine($"Provider: {provider.ProviderName}");
                Console.WriteLine($"  Requests in last minute: {provider.GetRequestCountLastMinute()}");
                Console.WriteLine($"  Successful requests: {provider.GetSuccessCountLastMinute()}");
                Console.WriteLine($"  Error rate: {provider.GetErrorRateLastMinute():P}");
                Console.WriteLine($"  Avg response time: {provider.GetAvgResponseTimeLast5Min():F2}ms");
                Console.WriteLine();
            }
        }
    }
}