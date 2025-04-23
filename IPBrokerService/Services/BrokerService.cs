using IpBroker.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
namespace IpBroker.Services
{
    public class BrokerService
    {
        private readonly List<ProviderStats> _providers;
        private readonly HttpClient _httpClient = new();
        private static int _requestCounter = 0;
        private static readonly object _consoleLock = new object();

        public BrokerService(List<ProviderStats> providers)
        {
            _providers = providers;
        }

        public async Task<string> GetLocationAsync(string ipAddress)
        {
            int requestNumber = System.Threading.Interlocked.Increment(ref _requestCounter);
            ProviderStats provider = null;

            var selectionLog = new StringBuilder();
            selectionLog.AppendLine($"\n=== REQUEST #{requestNumber} | IP: {ipAddress} ===");
            selectionLog.AppendLine("Selecting provider...");

            lock (_providers)
            {
                var availableProviders = _providers.Where(p => p.CanAcceptRequest()).ToList();

                selectionLog.AppendLine($"Available providers: {availableProviders.Count}");
                foreach (var p in availableProviders)
                {
                    var avgTimeFormatted = $"{p.GetAvgResponseTimeLast5Min():F2}";
                    var errorRateFormatted = $"{p.GetErrorRateLastMinute():P2}"; 

                    selectionLog.AppendLine($"- {p.ProviderName}: " +
                                            $"ErrorCount={p.GetErrorCountLast5Min()}, " +
                                            //$"AvgTime={avgTimeFormatted}ms, " +
                                            $"ErrorRate={errorRateFormatted}");
                }

                provider = availableProviders
                    .OrderBy(p => p.GetErrorCountLast5Min())
                    .ThenBy(p => p.GetAvgResponseTimeLast5Min())
                    .FirstOrDefault();
            }

            if (provider == null)
            {
                selectionLog.AppendLine("No available provider at the moment.");

                lock (_consoleLock)
                {
                    Console.Write(selectionLog.ToString());
                    Console.WriteLine($"=== END REQUEST #{requestNumber} (FAILED - NO PROVIDERS) ===\n");
                }

                throw new Exception("No available provider at the moment.");
            }

            selectionLog.AppendLine($"Selected: {provider.ProviderName}");

            var stopwatch = Stopwatch.StartNew();
            var formattedEndpoint = string.Format(provider.EndpointFormat, ipAddress);
            var finalUrl = new Uri(provider.BaseUrl, formattedEndpoint);

            selectionLog.AppendLine($"URL: {finalUrl}");

            lock (_consoleLock)
            {
                Console.Write(selectionLog.ToString());
            }

            try
            {
                // Apply artificial delay
                if (provider.ArtificialDelayMs > 0)
                {
                    await Task.Delay(provider.ArtificialDelayMs);
                }

                // Simulate errors if configured
                if (provider.ShouldSimulateError())
                {
                    throw new Exception($"Simulated error from {provider.ProviderName}");
                }
                var json = $"{{ \"provider\": \"{provider.ProviderName}\", \"ip\": \"{ipAddress}\", " +
                          $"\"location\": \"Simulated Location\", \"country\": \"Simulation\" }}";

                var elapsed = stopwatch.ElapsedMilliseconds;
                provider.RecordResponse(false, elapsed);

                lock (_consoleLock)
                {
                    Console.WriteLine($"Status: SUCCESS");
                    Console.WriteLine($"Response Time: {elapsed}ms");
                    Console.WriteLine($"Response: {json}");
                    Console.WriteLine($"=== END REQUEST #{requestNumber} ===\n");
                }

                return json;
            }
            catch (Exception ex)
            {
                var elapsed = stopwatch.ElapsedMilliseconds;
                provider.RecordResponse(true, elapsed);

                lock (_consoleLock)
                {
                    Console.WriteLine($"Status: FAILED");
                    Console.WriteLine($"Response Time: {elapsed}ms");
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine($"=== END REQUEST #{requestNumber} ===\n");
                }

                throw;
            }
        }
    }
}
